using UnityEngine;

namespace VoxelSandbox.World
{
    public static class Mesher
    {
        public static int CountVisibleFaces(Chunk chunk)
        {
            int count = 0;
            for (int x = 0; x < Chunk.Size; x++)
            for (int y = 0; y < Chunk.Size; y++)
            for (int z = 0; z < Chunk.Size; z++)
            {
                byte id = chunk.GetBlock(x, y, z);
                if (id == 0) continue;
                if (chunk.GetBlock(x-1, y, z) == 0) count++;
                if (chunk.GetBlock(x+1, y, z) == 0) count++;
                if (chunk.GetBlock(x, y-1, z) == 0) count++;
                if (chunk.GetBlock(x, y+1, z) == 0) count++;
                if (chunk.GetBlock(x, y, z-1) == 0) count++;
                if (chunk.GetBlock(x, y, z+1) == 0) count++;
            }
            return count;
        }
    }
}
