using System.Runtime.CompilerServices;
using UnityEngine;

namespace VoxelSandbox.World
{
    public class Chunk
    {
        public const int Size = 16;
        public Vector3Int Position { get; private set; }
        public byte[,,] Blocks { get; private set; }

        public Chunk(Vector3Int position)
        {
            Position = position;
            Blocks = new byte[Size, Size, Size];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBlock(int x, int y, int z)
        {
            if ((uint)x >= (uint)Size || (uint)y >= (uint)Size || (uint)z >= (uint)Size || x < 0 || y < 0 || z < 0)
                return 0;
            return Blocks[x, y, z];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(int x, int y, int z, byte id)
        {
            if ((uint)x >= (uint)Size || (uint)y >= (uint)Size || (uint)z >= (uint)Size || x < 0 || y < 0 || z < 0)
                return;
            Blocks[x, y, z] = id;
        }

        public void Clear()
        {
            // Clear without reallocating the array to avoid GC pressure
            System.Array.Clear(Blocks, 0, Blocks.Length);
        }

        public void SetPosition(Vector3Int position)
        {
            Position = position;
        }
    }
}
