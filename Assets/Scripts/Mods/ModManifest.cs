using UnityEngine;

namespace VoxelSandbox.Mods
{
    [CreateAssetMenu(menuName = "VoxelSandbox/Mod Manifest")]
    public class ModManifest : ScriptableObject
    {
        public string ModName;
        public string Version;
        public string Author;
    }
}
