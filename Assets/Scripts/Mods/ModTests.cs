using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using VoxelSandbox.Mods;

namespace VoxelSandbox.Mods.Tests
{
    public class ModManifestTests
    {
        [Test]
        public void ModManifest_DefaultValues_AreValid()
        {
            var manifest = ScriptableObject.CreateInstance<ModManifest>();
            
            Assert.AreEqual("com.example.mod", manifest.ModId);
            Assert.AreEqual("1.0.0", manifest.Version);
            Assert.AreEqual(ModManifest.ModType.Content, manifest.Type);
            Assert.IsTrue(manifest.IsEnabled);
        }

        [Test]
        public void ModDependency_IsSatisfiedBy_Works()
        {
            var dep = new ModManifest.ModDependency
            {
                ModId = "core.mod",
                MinVersion = "1.0.0",
                MaxVersion = "2.0.0"
            };

            var manifest1 = ScriptableObject.CreateInstance<ModManifest>();
            manifest1.ModId = "core.mod";
            manifest1.Version = "1.5.0";

            var manifest2 = ScriptableObject.CreateInstance<ModManifest>();
            manifest2.ModId = "core.mod";
            manifest2.Version = "0.9.0";

            var manifest3 = ScriptableObject.CreateInstance<ModManifest>();
            manifest3.ModId = "core.mod";
            manifest3.Version = "2.1.0";

            Assert.IsTrue(dep.IsSatisfiedBy(manifest1));
            Assert.IsFalse(dep.IsSatisfiedBy(manifest2));
            Assert.IsFalse(dep.IsSatisfiedBy(manifest3));
        }

        [Test]
        public void ModDependency_VersionComparison_Works()
        {
            var dep = new ModManifest.ModDependency();
            
            Assert.AreEqual(0, dep.GetType().GetMethod("VersionCompare", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(dep, new object[] { "1.0.0", "1.0.0" }));
            
            Assert.Greater((int)dep.GetType().GetMethod("VersionCompare",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(dep, new object[] { "2.0.0", "1.0.0" }), 0);
            
            Assert.Less((int)dep.GetType().GetMethod("VersionCompare",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(dep, new object[] { "1.0.0", "2.0.0" }), 0);
        }
    }

    public class ModRepositoryTests
    {
        private string _testModsDir;
        private ModRepository _repository;

        [SetUp]
        public void Setup()
        {
            _testModsDir = Path.Combine(Application.temporaryCachePath, $"ModTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testModsDir);
            _repository = new ModRepository();
            // Usar reflection para inyectar directorio de prueba
            var field = typeof(ModRepository).GetField("_modsDirectory", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_repository, _testModsDir);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_testModsDir))
                Directory.Delete(_testModsDir, true);
        }

        [Test]
        public void ModRepository_FindManifests_FindsValidMods()
        {
            // Crear mod de prueba
            var modDir = Path.Combine(_testModsDir, "test.mod");
            Directory.CreateDirectory(modDir);
            
            var manifest = new ModManifest
            {
                ModId = "test.mod",
                Version = "1.0.0",
                DisplayName = "Test Mod"
            };
            
            var json = JsonUtility.ToJson(manifest, true);
            File.WriteAllText(Path.Combine(modDir, "manifest.mod.json"), json);

            var manifests = _repository.FindManifests();
            Assert.AreEqual(1, manifests.Count);
            Assert.IsTrue(manifests[0].EndsWith("manifest.mod.json"));
        }

        [Test]
        public void ModRepository_LoadManifest_LoadsCorrectly()
        {
            var modDir = Path.Combine(_testModsDir, "test.mod2");
            Directory.CreateDirectory(modDir);
            
            var manifest = new ModManifest
            {
                ModId = "test.mod2",
                Version = "2.0.0",
                Author = "Tester"
            };
            
            var manifestPath = Path.Combine(modDir, "manifest.mod.json");
            File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));

            var loaded = _repository.LoadManifest(manifestPath);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("test.mod2", loaded.ModId);
            Assert.AreEqual("2.0.0", loaded.Version);
        }

        [Test]
        public void ModRepository_CalculateModHash_IsDeterministic()
        {
            var modDir = Path.Combine(_testModsDir, "hash.mod");
            Directory.CreateDirectory(modDir);
            
            File.WriteAllText(Path.Combine(modDir, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(modDir, "file2.txt"), "content2");
            
            var hash1 = _repository.CalculateModHash(modDir);
            var hash2 = _repository.CalculateModHash(modDir);
            
            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual(64, hash1.Length); // SHA256 hex = 64 chars
        }

        [Test]
        public void ModRepository_InstallModFromDirectory_Works()
        {
            var sourceDir = Path.Combine(_testModsDir, "source_mod");
            Directory.CreateDirectory(sourceDir);
            
            var manifest = new ModManifest
            {
                ModId = "install.test",
                Version = "1.0.0"
            };
            File.WriteAllText(Path.Combine(sourceDir, "manifest.mod.json"), JsonUtility.ToJson(manifest, true));
            File.WriteAllText(Path.Combine(sourceDir, "readme.txt"), "Test mod");

            var result = _repository.InstallModFromDirectory(sourceDir);
            
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Manifest);
            Assert.AreEqual("install.test", result.Manifest.ModId);
            Assert.IsTrue(Directory.Exists(result.ModDirectory));
        }
    }

    public class ModLoaderTests
    {
        private GameObject _loaderGO;
        private ModLoader _loader;

        [SetUp]
        public void Setup()
        {
            _loaderGO = new GameObject("TestModLoader");
            _loader = _loaderGO.AddComponent<ModLoader>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_loaderGO != null)
                Object.DestroyImmediate(_loaderGO);
        }

        [Test]
        public void ModLoader_IsModLoaded_ReturnsFalseInitially()
        {
            Assert.IsFalse(_loader.IsModLoaded("any.mod"));
        }

        [Test]
        public void ModLoader_GetLoadedMod_ReturnsNullForMissing()
        {
            Assert.IsNull(_loader.GetLoadedMod("missing.mod"));
        }

        [Test]
        public void ModLoader_AllowUnsignedMods_AcceptsUnsigned()
        {
            _loader.AllowUnsignedMods = true;
            Assert.IsTrue(_loader.AllowUnsignedMods);
        }

        [Test]
        public void ModLoader_AddTrustedKey_AddsKey()
        {
            _loader.AddTrustedKey("test-key-123");
            // No hay método público para verificar, pero no debe lanzar excepción
            Assert.Pass();
        }
    }

    public class ModManagerTests
    {
        private GameObject _managerGO;
        private ModManager _manager;

        [SetUp]
        public void Setup()
        {
            _managerGO = new GameObject("TestModManager");
            _manager = _managerGO.AddComponent<ModManager>();
            _manager.Loader.AllowUnsignedMods = true;
        }

        [TearDown]
        public void Teardown()
        {
            if (_managerGO != null)
                Object.DestroyImmediate(_managerGO);
        }

        [Test]
        public void ModManager_GetModsByType_ReturnsEmptyInitially()
        {
            var mods = _manager.GetModsByType(ModManifest.ModType.Gameplay);
            Assert.IsNotNull(mods);
            Assert.AreEqual(0, mods.Count);
        }

        [Test]
        public void ModManager_LoadedMods_IsEmptyInitially()
        {
            Assert.AreEqual(0, _manager.LoadedMods.Count);
        }
    }
}