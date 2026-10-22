using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Mods
{
    /// <summary>
    /// Coordinador principal del sistema de mods.
    /// </summary>
    public class ModManager : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private ModLoader modLoader;
        [SerializeField] private ModRepository modRepository;

        [Header("Configuración")]
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private bool enableHotReload = false;

        public event Action<ModManifest> OnModLoaded;
        public event Action<string> OnModUnloaded;
        public event Action<string, string> OnModError;

        public ModLoader Loader => modLoader;
        public ModRepository Repository => modRepository;
        public IReadOnlyDictionary<string, ModLoader.LoadedMod> LoadedMods => modLoader.LoadedMods;

        private void Awake()
        {
            if (modLoader == null) modLoader = GetComponent<ModLoader>();
            if (modRepository == null) modRepository = new ModRepository();
            
            modLoader.OnModLoaded += manifest => OnModLoaded?.Invoke(manifest);
            modLoader.OnModLoadFailed += (id, err) => OnModError?.Invoke(id, err);
            modLoader.OnModUnloaded += id => OnModUnloaded?.Invoke(id);
        }

        private void Start()
        {
            if (autoLoadOnStart)
                LoadAllMods();
        }

        private void Update()
        {
            if (enableHotReload)
                CheckForChanges();
        }

        /// <summary>
        /// Carga todos los mods disponibles.
        /// </summary>
        public void LoadAllMods()
        {
            modLoader.LoadAllMods();
        }

        /// <summary>
        /// Instala y carga un mod desde un directorio.
        /// </summary>
        public bool InstallAndLoadMod(string sourceDirectory)
        {
            var result = modRepository.InstallModFromDirectory(sourceDirectory);
            if (!result.Success)
            {
                OnModError?.Invoke("install", result.Error);
                return false;
            }

            return modLoader.LoadMod(Path.Combine(result.ModDirectory, "manifest.mod.json"));
        }

        /// <summary>
        /// Descarga e instala un mod desde URL.
        /// </summary>
        public async System.Threading.Tasks.Task<bool> DownloadAndInstallMod(string url, string expectedHash = null)
        {
            var result = await modRepository.DownloadModAsync(url, expectedHash);
            if (!result.Success)
            {
                OnModError?.Invoke("download", result.Error);
                return false;
            }

            return modLoader.LoadMod(Path.Combine(result.ModDirectory, "manifest.mod.json"));
        }

        /// <summary>
        /// Descarga un mod.
        /// </summary>
        public void UnloadMod(string modId)
        {
            modLoader.UnloadMod(modId);
        }

        /// <summary>
        /// Recarga un mod (útil para desarrollo).
        /// </summary>
        public bool ReloadMod(string modId)
        {
            var mod = modLoader.GetLoadedMod(modId);
            if (mod == null) return false;

            var manifestPath = Path.Combine(mod.Directory, "manifest.mod.json");
            modLoader.UnloadMod(modId);
            return modLoader.LoadMod(manifestPath);
        }

        /// <summary>
        /// Obtiene todos los mods cargados de un tipo específico.
        /// </summary>
        public List<ModManifest> GetModsByType(ModManifest.ModType type)
        {
            return LoadedMods.Values
                .Where(m => m.Manifest.Type == type)
                .Select(m => m.Manifest)
                .ToList();
        }

        /// <summary>
        /// Verifica si hay actualizaciones para mods instalados.
        /// </summary>
        public List<ModUpdateInfo> CheckForUpdates()
        {
            var updates = new List<ModUpdateInfo>();
            // TODO: Implementar consulta a servidor de mods
            return updates;
        }

        private void CheckForChanges()
        {
            // TODO: FileSystemWatcher para hot reload en desarrollo
        }

        public class ModUpdateInfo
        {
            public string ModId;
            public string CurrentVersion;
            public string AvailableVersion;
            public string DownloadUrl;
            public string Changelog;
        }
    }
}