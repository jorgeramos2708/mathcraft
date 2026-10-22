using NUnit.Framework;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Multiplayer.Tests
{
    public class DedicatedServerTests
    {
        [Test]
        public void ServerConfig_DefaultValues()
        {
            var config = ScriptableObject.CreateInstance<ServerConfig>();
            Assert.AreEqual(7777, config.Port);
            Assert.AreEqual(16, config.MaxPlayers);
        }
    }
}
