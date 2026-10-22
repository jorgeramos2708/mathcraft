using System.Collections.Generic;
using UnityEngine;

namespace VoxelSandbox.Mods
{
    public class ModLoader
    {
        private readonly List<string> loadedMods = new List<string>();

        public void LoadMod(string modName)
        {
            if (!loadedMods.Contains(modName))
            {
                loadedMods.Add(modName);
                Debug.Log($"Mod loaded: {modName}");
            }
        }

        public bool IsModLoaded(string modName)
        {
            return loadedMods.Contains(modName);
        }
    }
}
