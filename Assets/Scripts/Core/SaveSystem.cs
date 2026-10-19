using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelSandbox.Player;

namespace VoxelSandbox.Core
{
    [System.Serializable]
    public class AchievementProgress
    {
        public int Level;
        public int Progress;
    }

    public static class SaveSystem
    {
        public static string SavePath => Path.Combine(Application.persistentDataPath, "world.json");
        public static string PlayerStatsPath => Path.Combine(Application.persistentDataPath, "player_stats.json");
        public static string InventoryPath => Path.Combine(Application.persistentDataPath, "inventory.json");
        public static string RewardsPath => Path.Combine(Application.persistentDataPath, "rewards.json");
        public static string AchievementsPath => Path.Combine(Application.persistentDataPath, "achievements.json");

        public static void SavePlayerStats(Stats stats)
        {
            var data = new StatsData { Health = stats.Health, Hunger = stats.Hunger };
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(PlayerStatsPath, json);
        }

        public static Stats LoadPlayerStats()
        {
            if (!File.Exists(PlayerStatsPath)) return new Stats();
            string json = File.ReadAllText(PlayerStatsPath);
            var data = JsonUtility.FromJson<StatsData>(json);
            return new Stats(data.Health, data.Hunger);
        }

        public static void SaveInventory(Inventory inventory)
        {
            var items = new List<InventoryItem>();
            foreach (var kv in inventory.Items)
            {
                items.Add(new InventoryItem { Id = kv.Key, Count = kv.Value });
            }
            var data = new InventoryData { Items = items };
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(InventoryPath, json);
        }

        public static Inventory LoadInventory()
        {
            var inventory = new Inventory();
            if (!File.Exists(InventoryPath)) return inventory;
            string json = File.ReadAllText(InventoryPath);
            var data = JsonUtility.FromJson<InventoryData>(json);
            if (data?.Items != null)
            {
                foreach (var item in data.Items)
                {
                    inventory.AddItem(item.Id, item.Count);
                }
            }
            return inventory;
        }

        public static void SaveChunk(Chunk chunk, string path)
        {
            var data = new ChunkData
            {
                PositionX = chunk.Position.x,
                PositionY = chunk.Position.y,
                PositionZ = chunk.Position.z,
                Blocks = new byte[Chunk.Size * Chunk.Size * Chunk.Size]
            };

            int idx = 0;
            for (int x = 0; x < Chunk.Size; x++)
            for (int y = 0; y < Chunk.Size; y++)
            for (int z = 0; z < Chunk.Size; z++)
                data.Blocks[idx++] = chunk.GetBlock(x, y, z);

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
        }

        public static Chunk LoadChunk(string path)
        {
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<ChunkData>(json);
            var chunk = new Chunk(new Vector3Int(data.PositionX, data.PositionY, data.PositionZ));
            int idx = 0;
            for (int x = 0; x < Chunk.Size; x++)
            for (int y = 0; y < Chunk.Size; y++)
            for (int z = 0; z < Chunk.Size; z++)
                chunk.SetBlock(x, y, z, data.Blocks[idx++]);
            return chunk;
        }

        public static void SaveRewards(HashSet<string> rewards)
        {
            string json = JsonUtility.ToJson(new RewardsData { Items = new List<string>(rewards) });
            File.WriteAllText(RewardsPath, json);
        }

        public static HashSet<string> LoadRewards()
        {
            if (!File.Exists(RewardsPath)) return new HashSet<string>();
            string json = File.ReadAllText(RewardsPath);
            var data = JsonUtility.FromJson<RewardsData>(json);
            return new HashSet<string>(data?.Items ?? new List<string>());
        }

        public static void SaveAchievements(Dictionary<string, AchievementProgress> achievements)
        {
            var items = achievements.Select(kv => new AchievementData { Id = kv.Key, Level = kv.Value.Level, Progress = kv.Value.Progress }).ToList();
            string json = JsonUtility.ToJson(new AchievementsData { Items = items });
            File.WriteAllText(AchievementsPath, json);
        }

        public static Dictionary<string, AchievementProgress> LoadAchievements()
        {
            if (!File.Exists(AchievementsPath)) return new Dictionary<string, AchievementProgress>();
            string json = File.ReadAllText(AchievementsPath);
            var data = JsonUtility.FromJson<AchievementsData>(json);
            var dict = new Dictionary<string, AchievementProgress>();
            if (data?.Items != null)
            {
                foreach (var item in data.Items)
                {
                    dict[item.Id] = new AchievementProgress { Level = item.Level, Progress = item.Progress };
                }
            }
            return dict;
        }

        [System.Serializable]
        private class AchievementData
        {
            public string Id;
            public int Level;
            public int Progress;
        }

        [System.Serializable]
        private class AchievementsData
        {
            public List<AchievementData> Items;
        }

        [System.Serializable]
        private class RewardsData
        {
            public List<string> Items;
        }

        [System.Serializable]
        private class StatsData
        {
            public int Health;
            public int Hunger;
        }

        [System.Serializable]
        private class InventoryItem
        {
            public int Id;
            public int Count;
        }

        [System.Serializable]
        private class InventoryData
        {
            public List<InventoryItem> Items;
        }

        [System.Serializable]
        private class ChunkData
        {
            public int PositionX, PositionY, PositionZ;
            public byte[] Blocks;
        }
    }
}
