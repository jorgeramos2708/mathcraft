using System.IO;
using UnityEngine;

namespace VoxelSandbox.Core
{
    public static class SaveSystem
    {
        public static string SavePath => Path.Combine(Application.persistentDataPath, "world.json");

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
            string path = Path.Combine(Application.persistentDataPath, "rewards.json");
            string json = JsonUtility.ToJson(new RewardsData { Items = new List<string>(rewards) });
            File.WriteAllText(path, json);
        }

        public static HashSet<string> LoadRewards()
        {
            string path = Path.Combine(Application.persistentDataPath, "rewards.json");
            if (!File.Exists(path)) return new HashSet<string>();
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<RewardsData>(json);
            return new HashSet<string>(data.Items);
        }

        [System.Serializable]
        private class RewardsData
        {
            public List<string> Items;
        }

        [System.Serializable]
        private class ChunkData
        {
            public int PositionX, PositionY, PositionZ;
            public byte[] Blocks;
        }
    }
}
