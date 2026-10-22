using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Mods
{
    /// <summary>
    /// Cargador de mods con verificación de integridad y firmas.
    /// </summary>
    public class ModLoader : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private string modsDirectory = "Mods";
        [SerializeField] private bool verifySignatures = true;
        [SerializeField] private bool allowUnsignedMods = false;
        [SerializeField] private List<string> trustedPublicKeys = new List<string>();

        private Dictionary<string, LoadedMod> _loadedMods = new Dictionary<string, LoadedMod>();
        private ModRepository _repository;

        public event Action<ModManifest> OnModLoaded;
        public event Action<string, string> OnModLoadFailed; // modId, error
        public event Action<string> OnModUnloaded;

        public IReadOnlyDictionary<string, LoadedMod> LoadedMods => _loadedMods;

        private void Awake()
        {
            _repository = new ModRepository(modsDirectory);
            EnsureModsDirectory();
        }

        private void EnsureModsDirectory()
        {
            var fullPath = GetModsPath();
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
        }

        public string GetModsPath()
        {
            return Path.Combine(Application.persistentDataPath, modsDirectory);
        }

        /// <summary>
        /// Carga todos los mods válidos del directorio.
        /// </summary>
        public void LoadAllMods()
        {
            var manifestFiles = _repository.FindManifests();
            foreach (var manifestPath in manifestFiles)
            {
                LoadMod(manifestPath);
            }
            Debug.Log($"[ModLoader] Cargados {_loadedMods.Count} mods");
        }

        /// <summary>
        /// Carga un mod específico desde su manifest.
        /// </summary>
        public bool LoadMod(string manifestPath)
        {
            try
            {
                var manifest = _repository.LoadManifest(manifestPath);
                if (manifest == null)
                {
                    OnModLoadFailed?.Invoke(Path.GetFileName(manifestPath), "Manifest inválido");
                    return false;
                }

                // Verificar dependencias
                if (!CheckDependencies(manifest))
                {
                    OnModLoadFailed?.Invoke(manifest.ModId, "Dependencias no satisfechas");
                    return false;
                }

                // Verificar firma si está habilitado
                if (verifySignatures && !VerifyModSignature(manifest))
                {
                    if (!allowUnsignedMods)
                    {
                        OnModLoadFailed?.Invoke(manifest.ModId, "Firma inválida o no confiable");
                        return false;
                    }
                    Debug.LogWarning($"[ModLoader] Mod sin firma válida (permitido): {manifest.ModId}");
                }

                // Verificar conflictos
                if (_loadedMods.ContainsKey(manifest.ModId))
                {
                    var existing = _loadedMods[manifest.ModId];
                    if (existing.Manifest.Version.CompareTo(manifest.Version) >= 0)
                    {
                        Debug.Log($"[ModLoader] Mod ya cargado con versión igual o superior: {manifest.ModId}");
                        return true;
                    }
                    // Actualizar a versión superior
                    UnloadMod(manifest.ModId);
                }

                // Cargar assemblies del mod
                var assemblies = _repository.LoadModAssemblies(manifest);
                if (assemblies == null || assemblies.Length == 0)
                {
                    OnModLoadFailed?.Invoke(manifest.ModId, "No se pudieron cargar assemblies");
                    return false;
                }

                var loadedMod = new LoadedMod
                {
                    Manifest = manifest,
                    Assemblies = assemblies,
                    LoadedAt = DateTime.UtcNow,
                    Directory = Path.GetDirectoryName(manifestPath)
                };

                _loadedMods[manifest.ModId] = loadedMod;
                OnModLoaded?.Invoke(manifest);

                Debug.Log($"[ModLoader] Mod cargado: {manifest.ModId} v{manifest.Version} ({assemblies.Length} assemblies)");
                return true;
            }
            catch (Exception ex)
            {
                OnModLoadFailed?.Invoke(Path.GetFileName(manifestPath), ex.Message);
                Debug.LogError($"[ModLoader] Error cargando mod: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Descarga un mod.
        /// </summary>
        public void UnloadMod(string modId)
        {
            if (_loadedMods.TryGetValue(modId, out var loadedMod))
            {
                // TODO: Llamar a métodos de cleanup del mod si existen
                _loadedMods.Remove(modId);
                OnModUnloaded?.Invoke(modId);
                Debug.Log($"[ModLoader] Mod descargado: {modId}");
            }
        }

        /// <summary>
        /// Verifica si un mod está cargado.
        /// </summary>
        public bool IsModLoaded(string modId)
        {
            return _loadedMods.ContainsKey(modId);
        }

        /// <summary>
        /// Obtiene información de un mod cargado.
        /// </summary>
        public LoadedMod GetLoadedMod(string modId)
        {
            return _loadedMods.TryGetValue(modId, out var mod) ? mod : null;
        }

        /// <summary>
        /// Verifica dependencias de un mod.
        /// </summary>
        private bool CheckDependencies(ModManifest manifest)
        {
            if (manifest.Dependencies == null) return true;

            foreach (var dep in manifest.Dependencies)
            {
                if (!_loadedMods.ContainsKey(dep.ModId))
                {
                    Debug.LogWarning($"[ModLoader] Dependencia faltante: {dep.ModId} para {manifest.ModId}");
                    return false;
                }

                var loaded = _loadedMods[dep.ModId];
                if (loaded.Manifest.Version.CompareTo(dep.MinVersion) < 0)
                {
                    Debug.LogWarning($"[ModLoader] Versión de dependencia insuficiente: {dep.ModId} {loaded.Manifest.Version} < {dep.MinVersion}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Verifica la firma criptográfica del mod.
        /// </summary>
        private bool VerifyModSignature(ModManifest manifest)
        {
            if (string.IsNullOrEmpty(manifest.Signature) || string.IsNullOrEmpty(manifest.PublicKey))
                return false;

            if (!trustedPublicKeys.Contains(manifest.PublicKey))
            {
                Debug.LogWarning($"[ModLoader] Clave pública no confiable: {manifest.PublicKey}");
                return false;
            }

            try
            {
                // Recalcular hash del contenido del mod
                var modDir = Path.GetDirectoryName(manifest.GetType().Assembly.Location);
                var calculatedHash = _repository.CalculateModHash(modDir);
                
                // Verificar firma (simplificado - en producción usar RSA/ECDSA)
                var expectedSignature = ComputeHash(calculatedHash + manifest.PublicKey);
                return string.Equals(expectedSignature, manifest.Signature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// Añade una clave pública confiable.
        /// </summary>
        public void AddTrustedKey(string publicKey)
        {
            if (!trustedPublicKeys.Contains(publicKey))
                trustedPublicKeys.Add(publicKey);
        }

        [Serializable]
        public class LoadedMod
        {
            public ModManifest Manifest;
            public System.Reflection.Assembly[] Assemblies;
            public DateTime LoadedAt;
            public string Directory;
        }
    }
}