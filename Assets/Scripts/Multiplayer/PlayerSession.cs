using Unity.Netcode;
using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    public class PlayerSession : NetworkBehaviour
    {
        public ulong PlayerId => OwnerClientId;

        public override void OnNetworkSpawn()
        {
            Debug.Log($"PlayerSession spawned for {PlayerId}");
        }

        public override void OnNetworkDespawn()
        {
            Debug.Log($"PlayerSession despawned for {PlayerId}");
        }
    }
}
