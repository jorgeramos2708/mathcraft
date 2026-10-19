using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    public class ChunkSync : MonoBehaviour
    {
        public void SyncChunk(Vector3Int pos)
        {
            Debug.Log($"Syncing chunk {pos}");
        }
    }
}
