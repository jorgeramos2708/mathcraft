using System.Collections.Generic;
using UnityEngine;

namespace VoxelSandbox.World
{
    public static class Mesher
    {
        private static readonly Dictionary<Chunk, int> _visibleFaceCache = new();
        private static readonly Dictionary<Chunk, int> _greedyQuadCache = new();

        public static void InvalidateCache(Chunk chunk)
        {
            _visibleFaceCache.Remove(chunk);
            _greedyQuadCache.Remove(chunk);
        }

        public static int CountVisibleFaces(Chunk chunk)
        {
            if (_visibleFaceCache.TryGetValue(chunk, out int cached))
                return cached;

            int result = CountVisibleFacesOptimized(chunk);
            _visibleFaceCache[chunk] = result;
            return result;
        }

        private static int CountVisibleFacesOptimized(Chunk chunk)
        {
            var blocks = chunk.Blocks;
            int size = Chunk.Size;
            int count = 0;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        byte id = blocks[x, y, z];
                        if (id == 0) continue;

                        if (x == 0 || blocks[x - 1, y, z] == 0) count++;
                        if (x == size - 1 || blocks[x + 1, y, z] == 0) count++;
                        if (y == 0 || blocks[x, y - 1, z] == 0) count++;
                        if (y == size - 1 || blocks[x, y + 1, z] == 0) count++;
                        if (z == 0 || blocks[x, y, z - 1] == 0) count++;
                        if (z == size - 1 || blocks[x, y, z + 1] == 0) count++;
                    }
                }
            }

            return count;
        }

        public static int CountGreedyQuads(Chunk chunk)
        {
            if (_greedyQuadCache.TryGetValue(chunk, out int cached))
                return cached;

            var blocks = chunk.Blocks;
            int size = Chunk.Size;
            int total = 0;

            total += CountQuadsXPositive(blocks, size);
            total += CountQuadsXNegative(blocks, size);
            total += CountQuadsYPositive(blocks, size);
            total += CountQuadsYNegative(blocks, size);
            total += CountQuadsZPositive(blocks, size);
            total += CountQuadsZNegative(blocks, size);

            _greedyQuadCache[chunk] = total;
            return total;
        }

        private static int CountQuadsXPositive(byte[,,] blocks, int size)
        {
            var mask = new bool[size, size];
            var maskId = new byte[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    int x = size - 1;
                    while (x >= 0 && blocks[x, y, z] == 0) x--;
                    if (x >= 0 && (x == size - 1 || blocks[x + 1, y, z] == 0))
                    {
                        mask[y, z] = true;
                        maskId[y, z] = blocks[x, y, z];
                    }
                }
            }

            return GreedyMerge(mask, maskId, size);
        }

        private static int CountQuadsXNegative(byte[,,] blocks, int size)
        {
            var mask = new bool[size, size];
            var maskId = new byte[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    int x = 0;
                    while (x < size && blocks[x, y, z] == 0) x++;
                    if (x < size && (x == 0 || blocks[x - 1, y, z] == 0))
                    {
                        mask[y, z] = true;
                        maskId[y, z] = blocks[x, y, z];
                    }
                }
            }

            return GreedyMerge(mask, maskId, size);
        }

        private static int CountQuadsYPositive(byte[,,] blocks, int size)
        {
            var mask = new bool[size, size];
            var maskId = new byte[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    int y = size - 1;
                    while (y >= 0 && blocks[x, y, z] == 0) y--;
                    if (y >= 0 && (y == size - 1 || blocks[x, y + 1, z] == 0))
                    {
                        mask[x, z] = true;
                        maskId[x, z] = blocks[x, y, z];
                    }
                }
            }

            return GreedyMerge(mask, maskId, size);
        }

        private static int CountQuadsYNegative(byte[,,] blocks, int size)
        {
            var mask = new bool[size, size];
            var maskId = new byte[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    int y = 0;
                    while (y < size && blocks[x, y, z] == 0) y++;
                    if (y < size && (y == 0 || blocks[x, y - 1, z] == 0))
                    {
                        mask[x, z] = true;
                        maskId[x, z] = blocks[x, y, z];
                    }
                }
            }

            return GreedyMerge(mask, maskId, size);
        }

        private static int CountQuadsZPositive(byte[,,] blocks, int size)
        {
            var mask = new bool[size, size];
            var maskId = new byte[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int z = size - 1;
                    while (z >= 0 && blocks[x, y, z] == 0) z--;
                    if (z >= 0 && (z == size - 1 || blocks[x, y, z + 1] == 0))
                    {
                        mask[x, y] = true;
                        maskId[x, y] = blocks[x, y, z];
                    }
                }
            }

            return GreedyMerge(mask, maskId, size);
        }

        private static int CountQuadsZNegative(byte[,,] blocks, int size)
        {
            var mask = new bool[size, size];
            var maskId = new byte[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int z = 0;
                    while (z < size && blocks[x, y, z] == 0) z++;
                    if (z < size && (z == 0 || blocks[x, y, z - 1] == 0))
                    {
                        mask[x, y] = true;
                        maskId[x, y] = blocks[x, y, z];
                    }
                }
            }

            return GreedyMerge(mask, maskId, size);
        }

        private static int GreedyMerge(bool[,] mask, byte[,] maskId, int size)
        {
            var visited = new bool[size, size];
            int quads = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (!mask[i, j] || visited[i, j]) continue;

                    byte id = maskId[i, j];

                    int width = 1;
                    while (j + width < size && mask[i, j + width] && maskId[i, j + width] == id && !visited[i, j + width])
                        width++;

                    int height = 1;
                    bool canExtend = true;
                    while (i + height < size && canExtend)
                    {
                        for (int k = 0; k < width; k++)
                        {
                            if (!mask[i + height, j + k] || maskId[i + height, j + k] != id || visited[i + height, j + k])
                            {
                                canExtend = false;
                                break;
                            }
                        }
                        if (canExtend) height++;
                    }

                    quads++;

                    for (int di = 0; di < height; di++)
                    {
                        for (int dj = 0; dj < width; dj++)
                        {
                            visited[i + di, j + dj] = true;
                        }
                    }
                }
            }

            return quads;
        }
    }
}
