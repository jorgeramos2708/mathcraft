using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelSandbox.Mods
{
    /// <summary>
    /// Metadatos completos de un mod.
    /// </summary>
    [CreateAssetMenu(menuName = "VoxelSandbox/Mod Manifest", fileName = "mod_manifest")]
    public class ModManifest : ScriptableObject
    {
        [Header("Identidad")]
        public string ModId = "com.example.mod";
        public string DisplayName = "Example Mod";
        public string Version = "1.0.0";
        public string Author = "Author Name";
        public string Description = "Mod description";

        [Header("Compatibilidad")]
        public string GameVersion = "0.6.0"; // Versión del juego compatible
        public int MinGameVersion = 6; // Versión mínima (major.minor como int: 0.6 = 6)
        public int MaxGameVersion = 999;

        [Header("Dependencias")]
        public List<ModDependency> Dependencies = new List<ModDependency>();

        [Header("Seguridad")]
        public string PublicKey = ""; // Clave pública del autor (hex)
        public string Signature = ""; // Firma del contenido (hex)

        [Header("Contenido")]
        public List<string> AssemblyNames = new List<string>(); // Nombres de DLLs a cargar
        public List<string> ResourcePaths = new List<string>(); // Rutas de assets adicionales
        public ModType Type = ModType.Content;

        [Header("Configuración")]
        public bool IsEnabled = true;
        public int LoadPriority = 0; // Mayor prioridad = carga primero
        public ModCategory Category = ModCategory.Gameplay;

        public enum ModType
        {
            Content,      // Bloques, items, biomas
            Gameplay,     // Mecánicas, sistemas
            UI,           // Interfaces, HUD
            WorldGen,     // Generación de mundo
            Education,    // Contenido educativo
            Utility       // Herramientas, comandos
        }

        public enum ModCategory
        {
            Vanilla,
            Gameplay,
            Cosmetic,
            QualityOfLife,
            Education,
            Experimental
        }
    }

    [Serializable]
    public class ModDependency
    {
        public string ModId;
        public string MinVersion = "1.0.0";
        public string MaxVersion = "999.999.999";
        public bool IsOptional = false;

        public bool IsSatisfiedBy(ModManifest other)
        {
            if (other.ModId != ModId) return false;
            return VersionCompare(other.Version, MinVersion) >= 0 && 
                   VersionCompare(other.Version, MaxVersion) <= 0;
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
    }
}