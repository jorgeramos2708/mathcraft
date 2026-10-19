using Unity.Netcode;
using UnityEngine;
using VoxelSandbox.World;
using VoxelSandbox.Core;

namespace VoxelSandbox.Multiplayer
{
    public class ChunkNetworkBehaviour : NetworkBehaviour
    {
        [SerializeField] private Vector3Int _initialPosition = Vector3Int.zero;
        [SerializeField] private Chunk _chunk;

        public NetworkVariable<Vector3Int> ChunkPosition { get; private set; }
        public NetworkVariable<uint> DataVersion { get; private set; }

        public override void OnNetworkSpawn()
        {
            ChunkPosition = new NetworkVariable<Vector3Int>(
                _initialPosition,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

            DataVersion = new NetworkVariable<uint>(
                0,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

            if (IsServer && _chunk == null)
            {
                _chunk = new Chunk(_initialPosition);
            }
        }

        public override void OnNetworkDespawn()
        {
            ChunkPosition = null;
            DataVersion = null;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestChunkDataServerRpc(ServerRpcParams rpcParams = default)
        {
            if (_chunk == null) return;
            byte[] flat = ChunkSerializer.Flatten(_chunk.Blocks);
            SendChunkDataClientRpc(flat, ChunkPosition.Value);
        }

        [ClientRpc]
        private void SendChunkDataClientRpc(byte[] flatData, Vector3Int position)
        {
            if (!IsClient) return;
            Debug.Log($"[ChunkNetworkBehaviour] Received chunk data for {position} ({flatData?.Length ?? 0} bytes)");
            if (flatData == null || flatData.Length == 0)
            {
                Debug.LogWarning("[ChunkNetworkBehaviour] Received empty chunk data");
                return;
            }

            if (_chunk == null)
            {
                _chunk = new Chunk(position);
            }

            Core.ChunkSerializer.Unflatten(flatData, _chunk.Blocks);
            Debug.Log($"[ChunkNetworkBehaviour] Applied chunk data for {position} on client");
        }

        [ServerRpc(RequireOwnership = true)]
        public void UpdateBlockServerRpc(Vector3Int localPos, byte blockId)
        {
            if (_chunk == null) return;
            _chunk.SetBlock(localPos.x, localPos.y, localPos.z, blockId);
            DataVersion.Value++;
            BroadcastBlockUpdateClientRpc(localPos, blockId, DataVersion.Value);
        }

        [ClientRpc]
        private void BroadcastBlockUpdateClientRpc(Vector3Int localPos, byte blockId, uint version)
        {
            Debug.Log($"[ChunkNetworkBehaviour] Block update at {localPos} -> {blockId} v{version}");
        }

        public void SetChunk(Chunk chunk)
        {
            if (!IsServer) return;
            _chunk = chunk;
            ChunkPosition.Value = chunk?.Position ?? Vector3Int.zero;
        }
    }
}
