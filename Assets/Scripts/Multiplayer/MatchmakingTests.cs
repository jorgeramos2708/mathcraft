using NUnit.Framework;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Multiplayer.Tests
{
    public class MatchmakingTests
    {
        [Test]
        public void MatchmakingQueue_AddAndMatch()
        {
            var service = new MatchmakingService();
            var player = new PlayerSession();
            service.JoinQueue(player);
            var match = service.GetNextMatch(1);
            Assert.AreEqual(1, match.Count);
        }

        [Test]
        public void LobbyManager_CreateAndJoin()
        {
            var lobby = new LobbyManager();
            var host = new PlayerSession();
            lobby.CreateLobby("lobby1", host);
            var player = new PlayerSession();
            var joined = lobby.JoinLobby("lobby1", player);
            Assert.IsTrue(joined);
            Assert.AreEqual(2, lobby.GetLobby("lobby1").Count);
        }
    }
}
