using Unity.Netcode;
using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Validador de autoridad de servidor para operaciones críticas.
    /// Garantiza que solo el servidor puede modificar estado autoritativo.
    /// </summary>
    public class ServerAuthority : NetworkBehaviour
    {
        [Header("Validación")]
        [SerializeField] private float maxMoveDistancePerTick = 10f;
        [SerializeField] private float maxBuildDistance = 6f;

        public static ServerAuthority Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Valida si un cliente puede realizar un movimiento.
        /// ServerRpc - solo el servidor puede validar.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ValidateMovementServerRpc(ulong clientId, Vector3 newPosition, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            var senderId = rpcParams.Receive.SenderClientId;
            if (senderId != clientId)
            {
                Debug.LogWarning($"[ServerAuthority] Client {senderId} intentó mover a {clientId}");
                return;
            }

            // Obtener posición actual del jugador
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var currentPos = client.PlayerObject.transform.position;
                float distance = Vector3.Distance(currentPos, newPosition);
                
                if (distance > maxMoveDistancePerTick)
                {
                    // Rechazar movimiento - teletransportar de vuelta
                    Debug.LogWarning($"[ServerAuthority] Movimiento inválido de {clientId}: {distance}m > {maxMoveDistancePerTick}m");
                    client.PlayerObject.transform.position = currentPos;
                    return;
                }

                // Movimiento válido - aplicar
                client.PlayerObject.transform.position = newPosition;
            }
        }

        /// <summary>
        /// Valida colocación/destrucción de bloques.
        /// ServerRpc - solo el servidor puede validar.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ValidateBlockActionServerRpc(ulong clientId, Vector3Int blockPos, byte blockId, bool isPlacement, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            var senderId = rpcParams.Receive.SenderClientId;
            if (senderId != clientId)
            {
                Debug.LogWarning($"[ServerAuthority] Client {senderId} intentó acción de bloque para {clientId}");
                return;
            }

            // Verificar distancia del jugador al bloque
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerPos = client.PlayerObject.transform.position;
                var blockWorldPos = new Vector3(blockPos.x, blockPos.y, blockPos.z);
                float distance = Vector3.Distance(playerPos, blockWorldPos);

                if (distance > maxBuildDistance)
                {
                    Debug.LogWarning($"[ServerAuthority] Acción de bloque muy lejana: {distance}m > {maxBuildDistance}m");
                    return;
                }

                // Acción válida - delegar al ChunkNetworkBehaviour
                var chunkNet = FindObjectOfType<ChunkNetworkBehaviour>();
                if (chunkNet != null)
                {
                    if (isPlacement)
                        chunkNet.UpdateBlockServerRpc(blockPos, blockId);
                    else
                        chunkNet.UpdateBlockServerRpc(blockPos, 0); // 0 = aire
                }
            }
        }

        /// <summary>
        /// Valida transacciones de inventario.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ValidateInventoryTransactionServerRpc(ulong clientId, int itemId, int amount, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            var senderId = rpcParams.Receive.SenderClientId;
            if (senderId != clientId) return;

            // TODO: Integrar con sistema de inventario del servidor
            Debug.Log($"[ServerAuthority] Transacción inventario: {clientId} - Item: {itemId}, Cantidad: {amount}");
        }
    }
}