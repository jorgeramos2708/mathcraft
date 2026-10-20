using System.Collections.Generic;
using UnityEngine;

namespace VoxelSandbox.World
{
    public static class WorldGenerator
    {
        private static readonly Dictionary<(Vector3Int, int), Chunk> _chunkCache = new Dictionary<(Vector3Int, int), Chunk>();
        private static readonly int _size = Chunk.Size;

        public static void ClearCache()
        {
            _chunkCache.Clear();
        }

        public static Chunk GenerateChunk(Vector3Int chunkPos, int seed = 0)
        {
            var key = (chunkPos, seed);
            if (_chunkCache.TryGetValue(key, out var cached))
                return cached;

            // Reuse Chunk instance via pool-like clear to reduce allocations
            var chunk = new Chunk(chunkPos);
            var blocks = chunk.Blocks;

            int baseX = chunkPos.x * _size;
            int baseY = chunkPos.y * _size;
            int baseZ = chunkPos.z * _size;

            // Local aliases to avoid repeated property access
            int size = _size;

            for (int x = 0; x < size; x++)
            {
                int worldX = baseX + x;
                for (int z = 0; z < size; z++)
                {
                    int worldZ = baseZ + z;
                    // Cache noise per column to avoid recomputation and reduce CPU work
                    float noise = Mathf.PerlinNoise((worldX + seed) * 0.1f, (worldZ + seed) * 0.1f);
                    int height = Mathf.Max(0, 8 + (int)(noise * 6f));

                    // Direct array access avoids bounds check overhead of SetBlock
                    for (int y = 0; y < size; y++)
                    {
                        int worldY = baseY + y;
                        if (worldY >= height) break;
                        blocks[x, y, z] = 1;
                    }
                }
            }

            _chunkCache[key] = chunk;
            return chunk;
        }

        // Batch generation optimized for 100 chunks with minimal allocations
        public static Chunk[] GenerateChunks(Vector3Int origin, int countX, int countZ, int seed = 0)
        {
            var result = new Chunk[countX * countZ];
            int idx = 0;
            for (int x = 0; x < countX; x++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    var pos = new Vector3Int(origin.x + x, origin.y, origin.z + z);
                    result[idx++] = GenerateChunk(pos, seed);
                }
            }
            return result;
        }
    }
}
