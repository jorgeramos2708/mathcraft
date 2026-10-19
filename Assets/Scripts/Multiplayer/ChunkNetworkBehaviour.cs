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
            // Apply placeholder for iteration 0.2: real apply deferred to 0.3
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
