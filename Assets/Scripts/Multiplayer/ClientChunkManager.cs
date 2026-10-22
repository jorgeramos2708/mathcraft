using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VoxelSandbox.World;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Gestor de chunks en el cliente.
    /// Cache local, solicitud de chunks faltantes, renderizado.
    /// </summary>
    public class ClientChunkManager : NetworkBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private int renderDistance = 4;
        [SerializeField] private float requestInterval = 0.5f; // No spamear requests

        private Dictionary<Vector3Int, ChunkNetworkBehaviour> _cachedChunks = new Dictionary<Vector3Int, ChunkNetworkBehaviour>();
        private HashSet<Vector3Int> _requestedChunks = new HashSet<Vector3Int>();
        private float _lastRequestTime;
        private ChunkNetworkBehaviour _chunkPrefab;

        public event Action<Vector3Int, Chunk> OnChunkReceived;
        public event Action<Vector3Int> OnChunkUnloaded;

        public IReadOnlyDictionary<Vector3Int, ChunkNetworkBehaviour> CachedChunks => _cachedChunks;

        private void Awake()
        {
            _chunkPrefab = Resources.Load<ChunkNetworkBehaviour>("ChunkNetworkBehaviour");
        }

        public override void OnNetworkSpawn()
        {
            if (!IsClient || IsServer) return; // Solo cliente puro
            _lastRequestTime = Time.time;
        }

        private void Update()
        {
            if (!IsClient || IsServer) return;

            // Solicitar chunks faltantes periódicamente
            if (Time.time - _lastRequestTime >= requestInterval)
            {
                RequestMissingChunks();
                _lastRequestTime = Time.time;
            }
        }

        /// <summary>
        /// Registra un chunk recibido del servidor.
        /// </summary>
        public void RegisterChunk(ChunkNetworkBehaviour chunkNet)
        {
            var pos = chunkNet.ChunkPosition.Value;
            if (!_cachedChunks.ContainsKey(pos))
            {
                _cachedChunks[pos] = chunkNet;
                _requestedChunks.Remove(pos);
                OnChunkReceived?.Invoke(pos, chunkNet.Chunk);
            }
        }

        /// <summary>
        /// Solicita chunks alrededor del jugador local.
        /// </summary>
        private void RequestMissingChunks()
        {
            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (player == null) return;

            var playerChunkPos = ChunkManager.WorldToChunkPos(player.transform.position);
            var neededChunks = new List<Vector3Int>();

            for (int x = -renderDistance; x <= renderDistance; x++)
            for (int z = -renderDistance; z <= renderDistance; z++)
            for (int y = -1; y <= 1; y++)
            {
                var chunkPos = new Vector3Int(
                    playerChunkPos.x + x,
                    playerChunkPos.y + y,
                    playerChunkPos.z + z
                );

                if (!_cachedChunks.ContainsKey(chunkPos) && !_requestedChunks.Contains(chunkPos))
                {
                    neededChunks.Add(chunkPos);
                }
            }

            // Solicitar chunks faltantes (máximo 10 por intervalo)
            foreach (var pos in neededChunks.Take(10))
            {
                RequestChunk(pos);
            }
        }

        /// <summary>
        /// Solicita un chunk específico al servidor.
        /// </summary>
        public void RequestChunk(Vector3Int position)
        {
            // Buscar si ya existe el ChunkNetworkBehaviour en la escena
            var existing = FindExistingChunkNetworkBehaviour(position);
            if (existing != null)
            {
                existing.RequestChunkDataServerRpc();
                _requestedChunks.Add(position);
                return;
            }

            // Si no existe, el servidor debería haberlo spawneado
            // Pero por si acaso, loggeamos
            Debug.LogWarning($"[ClientChunkManager] No ChunkNetworkBehaviour found for {position}, waiting for spawn");
        }

        private ChunkNetworkBehaviour FindExistingChunkNetworkBehaviour(Vector3Int position)
        {
            var allChunks = FindObjectsOfType<ChunkNetworkBehaviour>();
            foreach (var chunk in allChunks)
            {
                if (chunk.ChunkPosition.Value == position)
                    return chunk;
            }
            return null;
        }

        /// <summary>
        /// Elimina chunk del cache local.
        /// </summary>
        public void UnloadChunk(Vector3Int position)
        {
            if (_cachedChunks.TryGetValue(position, out var chunkNet))
            {
                _cachedChunks.Remove(position);
                OnChunkUnloaded?.Invoke(position);
            }
        }

        /// <summary>
        /// Obtiene chunk del cache local.
        /// </summary>
        public Chunk GetChunk(Vector3Int position)
        {
            return _cachedChunks.TryGetValue(position, out var chunkNet) ? chunkNet.Chunk : null;
        }

        /// <summary>
        /// Obtiene bloque en posición mundo (busca en chunk correspondiente).
        /// </summary>
        public byte GetBlock(Vector3Int worldPos)
        {
            var chunkPos = ChunkManager.WorldToChunkPos(worldPos);
            var localPos = WorldToLocalInChunk(worldPos);

            if (_cachedChunks.TryGetValue(chunkPos, out var chunkNet))
            {
                return chunkNet.Chunk.GetBlock(localPos.x, localPos.y, localPos.z);
            }
            return 0; // Aire si no cargado
        }

        /// <summary>
        /// Solicita colocación de bloque (envía al servidor).
        /// </summary>
        public void RequestPlaceBlock(Vector3Int worldPos, byte blockId)
        {
            var chunkPos = ChunkManager.WorldToChunkPos(worldPos);
            var localPos = WorldToLocalInChunk(worldPos);

            // Buscar ChunkNetworkBehaviour
            var chunkNet = FindExistingChunkNetworkBehaviour(chunkPos);
            if (chunkNet != null)
            {
                chunkNet.UpdateBlockServerRpc(localPos, blockId);
            }
        }

        /// <summary>
        /// Solicita destrucción de bloque.
        /// </summary>
        public void RequestDestroyBlock(Vector3Int worldPos)
        {
            RequestPlaceBlock(worldPos, 0); // 0 = aire
        }

        private Vector3Int WorldToLocalInChunk(Vector3Int worldPos)
        {
            return new Vector3Int(
                Mathf.Mod(worldPos.x, Chunk.Size),
                Mathf.Mod(worldPos.y, Chunk.Size),
                Mathf.Mod(worldPos.z, Chunk.Size)
            );
        }

        /// <summary>
        /// Limpia cache (ej. al desconectar).
        /// </summary>
        public void ClearCache()
        {
            _cachedChunks.Clear();
            _requestedChunks.Clear();
        }
    }
}