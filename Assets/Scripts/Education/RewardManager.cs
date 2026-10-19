using UnityEngine;
using System.Collections.Generic;
using VoxelSandbox.Core;

namespace VoxelSandbox.Education
{
    public class RewardManager
    {
        private const int PointsPerLevel = 10;

        private readonly HashSet<string> _unlocked = new HashSet<string>();
        private readonly Dictionary<string, AchievementProgress> _achievements = new Dictionary<string, AchievementProgress>();

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

            var achData = SaveSystem.LoadAchievements();
            _achievements.Clear();
            foreach (var kv in achData)
            {
                _achievements[kv.Key] = kv.Value;
            }
        }

        public void AddProgress(string achievementId, int amount)
        {
            if (amount <= 0) return;
            if (!_achievements.TryGetValue(achievementId, out var state))
            {
                state = new AchievementProgress { Level = 0, Progress = 0 };
                _achievements[achievementId] = state;
            }

            int previousLevel = state.Level;
            state.Progress += amount;

            while (state.Progress >= PointsPerLevel)
            {
                state.Progress -= PointsPerLevel;
                state.Level++;
                // Auto-unlock level reward for convenience
                Unlock($"{achievementId}_level{state.Level}");
            }

            if (state.Level > previousLevel)
            {
                Debug.Log($"Achievement {achievementId} advanced to level {state.Level} ({state.Progress}/{PointsPerLevel})");
            }

            SaveSystem.SaveAchievements(_achievements);
        }

        public int GetProgress(string achievementId)
        {
            return _achievements.TryGetValue(achievementId, out var state) ? state.Progress : 0;
        }

        public int GetLevel(string achievementId)
        {
            return _achievements.TryGetValue(achievementId, out var state) ? state.Level : 0;
        }

        public bool IsLevelUnlocked(string achievementId, int level)
        {
            return GetLevel(achievementId) >= level;
        }

        public void SaveAchievements()
        {
            SaveSystem.SaveAchievements(_achievements);
        }
    }
}
