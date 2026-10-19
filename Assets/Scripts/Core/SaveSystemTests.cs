using NUnit.Framework;
using UnityEngine;
using System.IO;
using VoxelSandbox.Player;

namespace VoxelSandbox.Core
{
    public class SaveSystemTests
    {
        [Test]
        public void SaveAndLoadChunk_RoundTrip()
        {
            var chunk = new Chunk(new Vector3Int(1,2,3));
            chunk.SetBlock(5,5,5, 7);
            chunk.SetBlock(0,0,0, 9);

            string path = Path.Combine(Application.temporaryCachePath, "test_chunk.json");
            SaveSystem.SaveChunk(chunk, path);

            var loaded = SaveSystem.LoadChunk(path);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(chunk.Position, loaded.Position);
            Assert.AreEqual(7, loaded.GetBlock(5,5,5));
            Assert.AreEqual(9, loaded.GetBlock(0,0,0));

            File.Delete(path);
        }

        [Test]
        public void SaveAndLoadPlayerStats_RoundTrip()
        {
            var stats = new Stats(75, 40);
            SaveSystem.SavePlayerStats(stats);
            var loaded = SaveSystem.LoadPlayerStats();
            Assert.AreEqual(75, loaded.Health);
            Assert.AreEqual(40, loaded.Hunger);
            // Cleanup
            File.Delete(SaveSystem.PlayerStatsPath);
        }

        [Test]
        public void SaveAndLoadInventory_RoundTrip()
        {
            var inv = new Inventory();
            inv.AddItem(1, 10);
            inv.AddItem(2, 5);
            SaveSystem.SaveInventory(inv);
            var loaded = SaveSystem.LoadInventory();
            Assert.AreEqual(10, loaded.GetCount(1));
            Assert.AreEqual(5, loaded.GetCount(2));
            Assert.AreEqual(0, loaded.GetCount(999));
            // Cleanup
            File.Delete(SaveSystem.InventoryPath);
        }

        [Test]
        public void SaveAndLoadRewards_RoundTrip()
        {
            var rewards = new System.Collections.Generic.HashSet<string> { "reward_a", "reward_b" };
            SaveSystem.SaveRewards(rewards);
            var loaded = SaveSystem.LoadRewards();
            Assert.IsTrue(loaded.Contains("reward_a"));
            Assert.IsTrue(loaded.Contains("reward_b"));
            Assert.AreEqual(2, loaded.Count);
            // Cleanup
            File.Delete(SaveSystem.RewardsPath);
        }
    }
}
