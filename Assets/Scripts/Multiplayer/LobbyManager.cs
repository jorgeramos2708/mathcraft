using System.Collections.Generic;
using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    public class LobbyManager
    {
        private readonly Dictionary<string, List<PlayerSession>> lobbies = new Dictionary<string, List<PlayerSession>>();

        public void CreateLobby(string lobbyId, PlayerSession host)
        {
            if (!lobbies.ContainsKey(lobbyId))
            {
                lobbies[lobbyId] = new List<PlayerSession> { host };
            }
        }

        public bool JoinLobby(string lobbyId, PlayerSession player)
        {
            if (lobbies.TryGetValue(lobbyId, out var list))
            {
                list.Add(player);
                return true;
            }
            return false;
        }

        public List<PlayerSession> GetLobby(string lobbyId)
        {
            return lobbies.TryGetValue(lobbyId, out var list) ? list : null;
        }
    }
}
