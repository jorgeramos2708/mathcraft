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

        public byte GetBlock(int x, int y, int z)
        {
            if (x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size) return 0;
            return Blocks[x, y, z];
        }

        public void SetBlock(int x, int y, int z, byte id)
        {
            if (x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size) return;
            Blocks[x, y, z] = id;
        }
    }
}
