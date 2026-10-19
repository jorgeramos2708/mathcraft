using UnityEngine;
using System.Collections.Generic;
using VoxelSandbox.Core;

namespace VoxelSandbox.Education
{
    public class RewardManager
    {
        private readonly HashSet<string> _unlocked = new HashSet<string>();

        public bool Unlock(string rewardId)
        {
            _unlocked.Add(rewardId);
            Debug.Log($"Unlocked {rewardId}");
            SaveSystem.SaveRewards(_unlocked);
            return true;
        }

        public bool IsUnlocked(string rewardId) => _unlocked.Contains(rewardId);

        public void Load()
        {
            var data = SaveSystem.LoadRewards();
            _unlocked.Clear();
            foreach (var id in data) _unlocked.Add(id);
        }
    }
}
