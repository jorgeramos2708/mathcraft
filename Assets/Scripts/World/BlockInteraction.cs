using UnityEngine;

namespace VoxelSandbox.World
{
    public static class BlockInteraction
    {
        public static bool TryBreakBlock(Chunk chunk, Vector3Int localPos)
        {
            if (localPos.x < 0 || localPos.x >= Chunk.Size) return false;
            if (localPos.y < 0 || localPos.y >= Chunk.Size) return false;
            if (localPos.z < 0 || localPos.z >= Chunk.Size) return false;
            if (chunk.GetBlock(localPos.x, localPos.y, localPos.z) == 0) return false;
            chunk.SetBlock(localPos.x, localPos.y, localPos.z, 0);
            return true;
        }

        public static bool TryPlaceBlock(Chunk chunk, Vector3Int localPos)
        {
            if (localPos.x < 0 || localPos.x >= Chunk.Size) return false;
            if (localPos.y < 0 || localPos.y >= Chunk.Size) return false;
            if (localPos.z < 0 || localPos.z >= Chunk.Size) return false;
            if (chunk.GetBlock(localPos.x, localPos.y, localPos.z) != 0) return false;
            chunk.SetBlock(localPos.x, localPos.y, localPos.z, 1);
            return true;
        }
    }
}
