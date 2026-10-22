using System.Collections.Generic;
using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    public class MatchmakingService
    {
        private readonly List<PlayerSession> queue = new List<PlayerSession>();

        public void JoinQueue(PlayerSession player)
        {
            if (!queue.Contains(player))
            {
                queue.Add(player);
                Debug.Log($"Player {player.PlayerId} joined matchmaking");
            }
        }

        public void LeaveQueue(PlayerSession player)
        {
            queue.Remove(player);
        }

        public List<PlayerSession> GetNextMatch(int maxPlayers)
        {
            var match = queue.GetRange(0, Mathf.Min(maxPlayers, queue.Count));
            queue.RemoveRange(0, match.Count);
            return match;
        }
    }
}
