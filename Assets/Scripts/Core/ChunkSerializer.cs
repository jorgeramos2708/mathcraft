using UnityEngine;
using VoxelSandbox.World;

namespace VoxelSandbox.Core
{
    public static class ChunkSerializer
    {
        private static readonly int Size = Chunk.Size;
        private static readonly int Stride = Size * Size * Size;

        public static byte[] Flatten(byte[,,] blocks)
        {
            if (blocks == null) return new byte[0];
            byte[] flat = new byte[Stride];
            int idx = 0;
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    for (int z = 0; z < Size; z++)
                        flat[idx++] = blocks[x, y, z];
            return flat;
        }

        public static void Unflatten(byte[] flat, byte[,,] blocks)
        {
            if (flat == null || flat.Length != Stride) return;
            int idx = 0;
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    for (int z = 0; z < Size; z++)
                        blocks[x, y, z] = flat[idx++];
        }
    }
}
