using UnityEngine;

namespace VoxelSandbox.World
{
    public static class WorldGenerator
    {
        public static Chunk GenerateChunk(Vector3Int chunkPos, int seed = 0)
        {
            var chunk = new Chunk(chunkPos);
            for (int x = 0; x < Chunk.Size; x++)
            for (int z = 0; z < Chunk.Size; z++)
            {
                int worldX = chunkPos.x * Chunk.Size + x;
                int worldZ = chunkPos.z * Chunk.Size + z;
                int height = Mathf.Max(0, 8 + Mathf.PerlinNoise((worldX + seed) * 0.1f, (worldZ + seed) * 0.1f) * 6);
                for (int y = 0; y < Chunk.Size; y++)
                {
                    int worldY = chunkPos.y * Chunk.Size + y;
                    if (worldY < height) chunk.SetBlock(x, y, z, 1);
                }
            }
            return chunk;
        }
    }
}
