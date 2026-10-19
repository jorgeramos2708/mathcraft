using UnityEngine;
using System.Collections.Generic;

namespace VoxelSandbox.Education
{
    public class RewardManager
    {
        private readonly HashSet<string> _unlocked = new HashSet<string>();

        public bool Unlock(string rewardId)
        {
            _unlocked.Add(rewardId);
            Debug.Log($"Unlocked {rewardId}");
            return true;
        }

        public bool IsUnlocked(string rewardId) => _unlocked.Contains(rewardId);
    }
}
