using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace VoxelSandbox.Mods
{
    /// <summary>
    /// Repositorio de mods: descubrimiento local, descarga remota, caché.
    /// </summary>
    public class ModRepository
    {
        private readonly string _modsDirectory;
        private readonly string _cacheDirectory;
        private readonly string _tempDirectory;

        public ModRepository(string modsDirectoryName = "Mods")
        {
            _modsDirectory = Path.Combine(Application.persistentDataPath, modsDirectoryName);
            _cacheDirectory = Path.Combine(Application.persistentDataPath, "ModCache");
            _tempDirectory = Path.Combine(Application.temporaryCachePath, "ModTemp");

            EnsureDirectories();
        }

        private void EnsureDirectories()
        {
            Directory.CreateDirectory(_modsDirectory);
            Directory.CreateDirectory(_cacheDirectory);
            Directory.CreateDirectory(_tempDirectory);
        }

        /// <summary>
        /// Encuentra todos los archivos manifest.mod.json en el directorio de mods.
        /// </summary>
        public List<string> FindManifests()
        {
            var manifests = new List<string>();
            if (!Directory.Exists(_modsDirectory)) return manifests;

            var dirs = Directory.GetDirectories(_modsDirectory);
            foreach (var dir in dirs)
            {
                var manifestPath = Path.Combine(dir, "manifest.mod.json");
                if (File.Exists(manifestPath))
                    manifests.Add(manifestPath);
            }
            return manifests;
        }

        /// <summary>
        /// Carga un manifest desde archivo JSON.
        /// </summary>
        public ModManifest LoadManifest(string manifestPath)
        {
            try
            {
                var json = File.ReadAllText(manifestPath);
                return JsonUtility.FromJson<ModManifest>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModRepository] Error cargando manifest: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Guarda un manifest a archivo JSON.
        /// </summary>
        public void SaveManifest(ModManifest manifest, string directory)
        {
            var manifestPath = Path.Combine(directory, "manifest.mod.json");
            var json = JsonUtility.ToJson(manifest, true);
            File.WriteAllText(manifestPath, json);
        }

        /// <summary>
        /// Carga los assemblies (.dll) de un mod.
        /// </summary>
        public Assembly[] LoadModAssemblies(ModManifest manifest)
        {
            var assemblies = new List<Assembly>();
            var modDir = Path.GetDirectoryName(FindManifestForMod(manifest.ModId));

            if (manifest.AssemblyNames != null)
            {
                foreach (var assemblyName in manifest.AssemblyNames)
                {
                    var dllPath = Path.Combine(modDir, assemblyName);
                    if (File.Exists(dllPath))
                    {
                        try
                        {
                            var asm = Assembly.LoadFrom(dllPath);
                            assemblies.Add(asm);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ModRepository] Error cargando assembly {assemblyName}: {ex.Message}");
                        }
                    }
                }
            }

            return assemblies.ToArray();
        }

        private string FindManifestForMod(string modId)
        {
            var manifests = FindManifests();
            foreach (var path in manifests)
            {
                var manifest = LoadManifest(path);
                if (manifest != null && manifest.ModId == modId)
                    return path;
            }
            return null;
        }

        /// <summary>
        /// Calcula el hash SHA256 de todo el contenido de un mod.
        /// </summary>
        public string CalculateModHash(string modDirectory)
        {
            using var sha256 = SHA256.Create();
            var files = Directory.GetFiles(modDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith("manifest.mod.json"))
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in files)
            {
                var fileBytes = File.ReadAllBytes(file);
                sha256.TransformBlock(fileBytes, 0, fileBytes.Length, null, 0);
            }
            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return Convert.ToHexString(sha256.Hash);
        }

        /// <summary>
        /// Descarga un mod desde una URL remota.
        /// </summary>
        public async Task<ModDownloadResult> DownloadModAsync(string downloadUrl, string expectedHash = null)
        {
            var tempPath = Path.Combine(_tempDirectory, $"mod_{Guid.NewGuid():N}.zip");
            
            try
            {
                using var request = UnityWebRequest.Get(downloadUrl);
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return new ModDownloadResult { Success = false, Error = request.error };
                }

                File.WriteAllBytes(tempPath, request.downloadHandler.data);

                // Verificar hash si se proporcionó
                if (!string.IsNullOrEmpty(expectedHash))
                {
                    var actualHash = CalculateFileHash(tempPath);
                    if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(tempPath);
                        return new ModDownloadResult { Success = false, Error = "Hash mismatch" };
                    }
                }

                // Extraer ZIP al directorio de mods
                var extractDir = Path.Combine(_modsDirectory, $"mod_{Guid.NewGuid():N}");
                System.IO.Compression.ZipFile.ExtractToDirectory(tempPath, extractDir);
                File.Delete(tempPath);

                // Verificar que tiene manifest
                var manifestPath = Path.Combine(extractDir, "manifest.mod.json");
                if (!File.Exists(manifestPath))
                {
                    Directory.Delete(extractDir, true);
                    return new ModDownloadResult { Success = false, Error = "No manifest found" };
                }

                var manifest = LoadManifest(manifestPath);
                return new ModDownloadResult 
                { 
                    Success = true, 
                    ModDirectory = extractDir,
                    Manifest = manifest
                };
            }
            catch (Exception ex)
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                return new ModDownloadResult { Success = false, Error = ex.Message };
            }
        }

        /// <summary>
        /// Instala un mod desde un directorio local (arrastrar y soltar).
        /// </summary>
        public ModInstallResult InstallModFromDirectory(string sourceDirectory)
        {
            var manifestPath = Path.Combine(sourceDirectory, "manifest.mod.json");
            if (!File.Exists(manifestPath))
                return new ModInstallResult { Success = false, Error = "No manifest.mod.json found" };

            var manifest = LoadManifest(manifestPath);
            if (manifest == null)
                return new ModInstallResult { Success = false, Error = "Invalid manifest" };

            // Verificar si ya existe
            var existingDir = FindModDirectory(manifest.ModId);
            if (existingDir != null)
            {
                var existingManifest = LoadManifest(Path.Combine(existingDir, "manifest.mod.json"));
                if (existingManifest != null && VersionCompare(existingManifest.Version, manifest.Version) >= 0)
                {
                    return new ModInstallResult { Success = false, Error = "Same or newer version already installed" };
                }
                // Backup y reemplazar
                Directory.Delete(existingDir, true);
            }

            // Copiar al directorio de mods
            var targetDir = Path.Combine(_modsDirectory, manifest.ModId);
            CopyDirectory(sourceDirectory, targetDir);

            return new ModInstallResult { Success = true, ModDirectory = targetDir, Manifest = manifest };
        }

        private string FindModDirectory(string modId)
        {
            var dirs = Directory.GetDirectories(_modsDirectory);
            foreach (var dir in dirs)
            {
                if (Path.GetFileName(dir) == modId)
                    return dir;
            }
            return null;
        }

        private void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (var file in Directory.GetFiles(source))
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            foreach (var dir in Directory.GetDirectories(source))
                CopyDirectory(dir, Path.Combine(target, Path.GetFileName(dir)));
        }

        private string CalculateFileHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return Convert.ToHexString(hash);
        }

        private int VersionCompare(string a, string b)
        {
            var va = a.Split('.').Select(int.Parse).ToArray();
            var vb = b.Split('.').Select(int.Parse).ToArray();
            for (int i = 0; i < Math.Max(va.Length, vb.Length); i++)
            {
                int ai = i < va.Length ? va[i] : 0;
                int bi = i < vb.Length ? vb[i] : 0;
                if (ai != bi) return ai.CompareTo(bi);
            }
            return 0;
        }

        public class ModDownloadResult
        {
            public bool Success;
            public string Error;
            public string ModDirectory;
            public ModManifest Manifest;
        }

        public class ModInstallResult
        {
            public bool Success;
            public string Error;
            public string ModDirectory;
            public ModManifest Manifest;
        }
    }
}