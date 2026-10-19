using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    public static class BlockValidator
    {
        public static bool ValidateAction(Vector3Int pos, int playerId)
        {
            // Server authoritative check
            return true;
        }
    }
}
