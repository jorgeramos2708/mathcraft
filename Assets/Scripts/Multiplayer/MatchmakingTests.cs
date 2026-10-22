using NUnit.Framework;
using UnityEngine;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Multiplayer.Tests
{
    public class MatchmakingTests
    {
        private GameObject _matchmakingGO;
        private MatchmakingService _matchmaking;

        [SetUp]
        public void Setup()
        {
            _matchmakingGO = new GameObject("TestMatchmaking");
            _matchmaking = _matchmakingGO.AddComponent<MatchmakingService>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_matchmakingGO != null)
                Object.DestroyImmediate(_matchmakingGO);
        }

        [Test]
        public void Matchmaking_JoinQueue_AddsPlayer()
        {
            var player = CreateTestPlayer(1);
            var prefs = new MatchmakingService.MatchmakingPreferences { PreferredEducationLevel = 1 };

            _matchmaking.JoinQueue(player, prefs);

            var queue = _matchmaking.GetQueueSnapshot();
            Assert.AreEqual(1, queue.Count);
            Assert.AreEqual(player.ClientId, queue[0].Player.ClientId);
        }

        [Test]
        public void Matchmaking_LeaveQueue_RemovesPlayer()
        {
            var player = CreateTestPlayer(2);
            var prefs = new MatchmakingService.MatchmakingPreferences();

            _matchmaking.JoinQueue(player, prefs);
            _matchmaking.LeaveQueue(player.ClientId);

            var queue = _matchmaking.GetQueueSnapshot();
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void Matchmaking_DuplicateJoin_Ignored()
        {
            var player = CreateTestPlayer(3);
            var prefs = new MatchmakingService.MatchmakingPreferences();

            _matchmaking.JoinQueue(player, prefs);
            _matchmaking.JoinQueue(player, prefs); // Segunda vez

            var queue = _matchmaking.GetQueueSnapshot();
            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void Matchmaking_Preferences_AreStored()
        {
            var player = CreateTestPlayer(4);
            var prefs = new MatchmakingService.MatchmakingPreferences 
            { 
                PreferredEducationLevel = 2, 
                PreferredMode = MatchmakingService.GameMode.Education 
            };

            _matchmaking.JoinQueue(player, prefs);

            var queue = _matchmaking.GetQueueSnapshot();
            Assert.AreEqual(2, queue[0].Preferences.PreferredEducationLevel);
            Assert.AreEqual(MatchmakingService.GameMode.Education, queue[0].Preferences.PreferredMode);
        }

        private PlayerSession CreateTestPlayer(ulong id)
        {
            return new PlayerSession { ClientId = id, ConnectedAt = DateTime.UtcNow, IsAuthenticated = true };
        }
    }

    public class LobbyManagerTests
    {
        private GameObject _lobbyGO;
        private LobbyManager _lobbyManager;

        [SetUp]
        public void Setup()
        {
            _lobbyGO = new GameObject("TestLobbyManager");
            _lobbyManager = _lobbyGO.AddComponent<LobbyManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_lobbyGO != null)
                Object.DestroyImmediate(_lobbyGO);
        }

        [Test]
        public void LobbyManager_CreateLobby_ReturnsValidLobby()
        {
            var owner = CreateTestPlayer(10);
            var lobby = _lobbyManager.CreateLobby(owner);

            Assert.IsNotNull(lobby);
            Assert.AreEqual(owner.ClientId, lobby.Owner.ClientId);
            Assert.AreEqual(1, lobby.CurrentPlayers);
            Assert.AreEqual(LobbyManager.LobbyState.Waiting, lobby.State);
        }

        [Test]
        public void LobbyManager_JoinLobby_AddsPlayer()
        {
            var owner = CreateTestPlayer(10);
            var player = CreateTestPlayer(11);
            var lobby = _lobbyManager.CreateLobby(owner);

            var result = _lobbyManager.JoinLobby(lobby.LobbyId, player);

            Assert.IsTrue(result);
            Assert.AreEqual(2, lobby.CurrentPlayers);
            Assert.Contains(player, lobby.Players);
        }

        [Test]
        public void LobbyManager_JoinNonExistentLobby_Fails()
        {
            var player = CreateTestPlayer(12);
            var result = _lobbyManager.JoinLobby("lobby_inexistente", player);

            Assert.IsFalse(result);
        }

        [Test]
        public void LobbyManager_JoinFullLobby_Fails()
        {
            var owner = CreateTestPlayer(20);
            var settings = new LobbyManager.LobbySettings { MaxPlayers = 2 };
            var lobby = _lobbyManager.CreateLobby(owner, settings);

            var player1 = CreateTestPlayer(21);
            var player2 = CreateTestPlayer(22);

            _lobbyManager.JoinLobby(lobby.LobbyId, player1);
            var result = _lobbyManager.JoinLobby(lobby.LobbyId, player2);

            Assert.IsFalse(result); // Lleno (owner + player1 = 2)
        }

        [Test]
        public void LobbyManager_LeaveLobby_RemovesPlayer()
        {
            var owner = CreateTestPlayer(30);
            var player = CreateTestPlayer(31);
            var lobby = _lobbyManager.CreateLobby(owner);
            _lobbyManager.JoinLobby(lobby.LobbyId, player);

            var result = _lobbyManager.LeaveLobby(lobby.LobbyId, player.ClientId);

            Assert.IsTrue(result);
            Assert.AreEqual(1, lobby.CurrentPlayers);
            Assert.IsFalse(lobby.Players.Contains(player));
        }

        [Test]
        public void LobbyManager_OwnerLeaves_TransfersOwnership()
        {
            var owner = CreateTestPlayer(40);
            var player = CreateTestPlayer(41);
            var lobby = _lobbyManager.CreateLobby(owner);
            _lobbyManager.JoinLobby(lobby.LobbyId, player);

            _lobbyManager.LeaveLobby(lobby.LobbyId, owner.ClientId);

            Assert.AreEqual(player.ClientId, lobby.Owner.ClientId);
        }

        [Test]
        public void LobbyManager_SetState_UpdatesState()
        {
            var owner = CreateTestPlayer(50);
            var lobby = _lobbyManager.CreateLobby(owner);

            _lobbyManager.SetLobbyState(lobby.LobbyId, LobbyManager.LobbyState.Starting);

            Assert.AreEqual(LobbyManager.LobbyState.Starting, lobby.State);
        }

        [Test]
        public void LobbyManager_DestroyLobby_RemovesFromList()
        {
            var owner = CreateTestPlayer(60);
            var lobby = _lobbyManager.CreateLobby(owner);

            _lobbyManager.DestroyLobby(lobby.LobbyId);

            Assert.IsNull(_lobbyManager.GetLobby(lobby.LobbyId));
            Assert.AreEqual(0, _lobbyManager.Lobbies.Count);
        }

        [Test]
        public void LobbyManager_GetPublicLobbies_FiltersPrivate()
        {
            var owner1 = CreateTestPlayer(70);
            var owner2 = CreateTestPlayer(71);
            
            var publicLobby = _lobbyManager.CreateLobby(owner1, new LobbyManager.LobbySettings { IsPrivate = false });
            var privateLobby = _lobbyManager.CreateLobby(owner2, new LobbyManager.LobbySettings { IsPrivate = true });

            var publicLobbies = _lobbyManager.GetPublicLobbies();

            Assert.Contains(publicLobby, publicLobbies);
            Assert.IsFalse(publicLobbies.Contains(privateLobby));
        }

        private PlayerSession CreateTestPlayer(ulong id)
        {
            return new PlayerSession { ClientId = id, ConnectedAt = DateTime.UtcNow, IsAuthenticated = true };
        }
    }
}