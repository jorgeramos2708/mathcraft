using NUnit.Framework;
using VoxelSandbox.Mods;

namespace VoxelSandbox.Mods.Tests
{
    public class ModLoaderTests
    {
        [Test]
        public void ModLoader_LoadAndCheck()
        {
            var loader = new ModLoader();
            loader.LoadMod("TestMod");
            Assert.IsTrue(loader.IsModLoaded("TestMod"));
        }
    }
}
