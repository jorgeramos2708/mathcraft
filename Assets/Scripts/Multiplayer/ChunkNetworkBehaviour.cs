using Unity.Netcode;
using UnityEngine;
using VoxelSandbox.World;
using VoxelSandbox.Core;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Comportamiento de red para sincronización de chunks.
    /// Servidor: autoritativo, gestiona chunks y broadcast cambios.
    /// Cliente: solicita chunks, recibe updates, cache local.
    /// </summary>
    public class ChunkNetworkBehaviour : NetworkBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private Vector3Int _initialPosition = Vector3Int.zero;
        [SerializeField] private Chunk _chunk;

        // Estado de red
        public NetworkVariable<Vector3Int> ChunkPosition { get; private set; }
        public NetworkVariable<uint> DataVersion { get; private set; }
        public NetworkVariable<bool> IsLoaded { get; private set; }

        // Referencia al chunk local
        public Chunk Chunk => _chunk;
        public Vector3Int Position => _initialPosition;

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

            IsLoaded = new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

            if (IsServer && _chunk == null)
            {
                _chunk = new Chunk(_initialPosition);
            }

            // Cliente: solicitar chunk al spawnear si está cerca
            if (IsClient && !IsServer)
            {
                RequestChunkDataServerRpc();
            }
        }

        public override void OnNetworkDespawn()
        {
            ChunkPosition = null;
            DataVersion = null;
            IsLoaded = null;
        }

        #region Server RPCs

        /// <summary>
        /// Cliente solicita datos completos del chunk.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RequestChunkDataServerRpc(ServerRpcParams rpcParams = default)
        {
            if (_chunk == null) return;

            byte[] flat = ChunkSerializer.Flatten(_chunk.Blocks);
            SendChunkDataClientRpc(flat, ChunkPosition.Value, DataVersion.Value, rpcParams.Receive.SenderClientId);
        }

        /// <summary>
        /// Cliente solicita actualización de un bloque específico.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void UpdateBlockServerRpc(Vector3Int localPos, byte blockId, ServerRpcParams rpcParams = default)
        {
            if (_chunk == null) return;

            // Validación básica de límites
            if (localPos.x < 0 || localPos.x >= Chunk.Size ||
                localPos.y < 0 || localPos.y >= Chunk.Size ||
                localPos.z < 0 || localPos.z >= Chunk.Size)
            {
                Debug.LogWarning($"[ChunkNetworkBehaviour] UpdateBlock fuera de límites: {localPos}");
                return;
            }

            _chunk.SetBlock(localPos.x, localPos.y, localPos.z, blockId);
            DataVersion.Value++;

            // Broadcast a TODOS los clientes (incluyendo sender para confirmación)
            BroadcastBlockUpdateClientRpc(localPos, blockId, DataVersion.Value);
        }

        /// <summary>
        /// Batch update para múltiples bloques (útil para herramientas grandes).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void UpdateBlocksBatchServerRpc(Vector3Int[] positions, byte[] blockIds, ServerRpcParams rpcParams = default)
        {
            if (_chunk == null || positions.Length != blockIds.Length) return;

            for (int i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                if (pos.x >= 0 && pos.x < Chunk.Size &&
                    pos.y >= 0 && pos.y < Chunk.Size &&
                    pos.z >= 0 && pos.z < Chunk.Size)
                {
                    _chunk.SetBlock(pos.x, pos.y, pos.z, blockIds[i]);
                }
            }

            DataVersion.Value++;
            BroadcastBlocksBatchClientRpc(positions, blockIds, DataVersion.Value);
        }

        #endregion

        #region Client RPCs

        /// <summary>
        /// Envía datos completos del chunk a un cliente específico.
        /// </summary>
        [ClientRpc]
        private void SendChunkDataClientRpc(byte[] flatData, Vector3Int position, uint version, ClientRpcParams rpcParams = default)
        {
            if (!IsClient) return;
            if (flatData == null || flatData.Length == 0)
            {
                Debug.LogWarning($"[ChunkNetworkBehaviour] Received empty chunk data for {position}");
                return;
            }

            if (_chunk == null)
            {
                _chunk = new Chunk(position);
            }

            Core.ChunkSerializer.Unflatten(flatData, _chunk.Blocks);
            IsLoaded.Value = true;

            Debug.Log($"[ChunkNetworkBehaviour] Chunk loaded: {position} v{version} ({flatData.Length} bytes)");
        }

        /// <summary>
        /// Broadcast de actualización de bloque individual a todos los clientes.
        /// </summary>
        [ClientRpc]
        private void BroadcastBlockUpdateClientRpc(Vector3Int localPos, byte blockId, uint version)
        {
            if (!IsClient || _chunk == null) return;

            _chunk.SetBlock(localPos.x, localPos.y, localPos.z, blockId);
            // DataVersion se actualiza automáticamente via NetworkVariable
        }

        /// <summary>
        /// Broadcast de actualización en batch.
        /// </summary>
        [ClientRpc]
        private void BroadcastBlocksBatchClientRpc(Vector3Int[] positions, byte[] blockIds, uint version)
        {
            if (!IsClient || _chunk == null) return;

            for (int i = 0; i < positions.Length; i++)
            {
                var pos = positions[i];
                _chunk.SetBlock(pos.x, pos.y, pos.z, blockIds[i]);
            }
        }

        #endregion

        /// <summary>
        /// Asigna chunk (solo servidor).
        /// </summary>
        public void SetChunk(Chunk chunk)
        {
            if (!IsServer) return;
            _chunk = chunk;
            if (chunk != null)
            {
                ChunkPosition.Value = chunk.Position;
                IsLoaded.Value = true;
            }
        }

        /// <summary>
        /// Marca chunk como modificado para sync (servidor).
        /// </summary>
        public void MarkDirty()
        {
            if (IsServer)
                DataVersion.Value++;
        }
    }
}