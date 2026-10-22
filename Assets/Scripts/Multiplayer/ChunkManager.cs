using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using VoxelSandbox.World;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Gestor de chunks en el servidor.
    /// Maneja carga/descarga, tracking de chunks sucios, broadcast periódico.
    /// </summary>
    public class ChunkManager : NetworkBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private int renderDistance = 4; // chunks alrededor del jugador
        [SerializeField] private float updateInterval = 0.1f; // 10 Hz broadcast
        [SerializeField] private int maxChunksPerUpdate = 50;

        private Dictionary<Vector3Int, ChunkNetworkBehaviour> _loadedChunks = new Dictionary<Vector3Int, ChunkNetworkBehaviour>();
        private HashSet<Vector3Int> _dirtyChunks = new HashSet<Vector3Int>();
        private float _lastUpdateTime;
        private ChunkNetworkBehaviour _chunkPrefab;

        public event Action<Vector3Int> OnChunkLoaded;
        public event Action<Vector3Int> OnChunkUnloaded;

        public IReadOnlyDictionary<Vector3Int, ChunkNetworkBehaviour> LoadedChunks => _loadedChunks;
        public int LoadedChunkCount => _loadedChunks.Count;

        private void Awake()
        {
            _chunkPrefab = Resources.Load<ChunkNetworkBehaviour>("ChunkNetworkBehaviour");
            if (_chunkPrefab == null)
            {
                Debug.LogError("[ChunkManager] ChunkNetworkBehaviour prefab no encontrado en Resources");
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            _lastUpdateTime = Time.time;
        }

        private void Update()
        {
            if (!IsServer) return;

            // Broadcast periódico de chunks sucios
            if (Time.time - _lastUpdateTime >= updateInterval)
            {
                BroadcastDirtyChunks();
                _lastUpdateTime = Time.time;
            }

            // Gestión de distancia de render (cargar/descargar chunks)
            ManageChunkLoading();
        }

        /// <summary>
        /// Obtiene o crea un chunk en la posición dada.
        /// </summary>
        public ChunkNetworkBehaviour GetOrCreateChunk(Vector3Int position)
        {
            if (_loadedChunks.TryGetValue(position, out var existing))
                return existing;

            if (_chunkPrefab == null) return null;

            var chunkObj = Instantiate(_chunkPrefab);
            chunkObj.transform.position = new Vector3(position.x * Chunk.Size, position.y * Chunk.Size, position.z * Chunk.Size);
            chunkObj.SetChunk(new Chunk(position));
            chunkObj.GetComponent<NetworkObject>().Spawn();

            _loadedChunks[position] = chunkObj;
            OnChunkLoaded?.Invoke(position);

            Debug.Log($"[ChunkManager] Chunk creado: {position} (Total: {_loadedChunks.Count})");
            return chunkObj;
        }

        /// <summary>
        /// Marca un chunk como modificado para broadcast en siguiente update.
        /// </summary>
        public void MarkChunkDirty(Vector3Int position)
        {
            if (_loadedChunks.ContainsKey(position))
                _dirtyChunks.Add(position);
        }

        /// <summary>
        /// Broadcast de todos los chunks sucios a clientes cercanos.
        /// </summary>
        private void BroadcastDirtyChunks()
        {
            if (_dirtyChunks.Count == 0) return;

            var chunksToSend = _dirtyChunks.Take(maxChunksPerUpdate).ToList();
            _dirtyChunks.ExceptWith(chunksToSend);

            foreach (var pos in chunksToSend)
            {
                if (_loadedChunks.TryGetValue(pos, out var chunkNet))
                {
                    // El chunk ya se encarga de broadcast via UpdateBlockServerRpc
                    // Aquí podríamos forzar re-envío completo si es necesario
                    chunkNet.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Carga chunks alrededor de un jugador.
        /// </summary>
        public void LoadChunksAroundPlayer(ulong clientId, Vector3 playerPosition)
        {
            var playerChunkPos = WorldToChunkPos(playerPosition);

            for (int x = -renderDistance; x <= renderDistance; x++)
            for (int z = -renderDistance; z <= renderDistance; z++)
            for (int y = -1; y <= 1; y++) // Altura limitada por ahora
            {
                var chunkPos = new Vector3Int(
                    playerChunkPos.x + x,
                    playerChunkPos.y + y,
                    playerChunkPos.z + z
                );

                GetOrCreateChunk(chunkPos);
            }
        }

        /// <summary>
        /// Descarga chunks lejos de todos los jugadores.
        /// </summary>
        private void ManageChunkLoading()
        {
            // Recopilar posiciones de todos los jugadores
            var playerPositions = new List<Vector3>();
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null)
                    playerPositions.Add(client.PlayerObject.transform.position);
            }

            if (playerPositions.Count == 0) return;

            // Encontrar chunks a descargar (lejos de todos los jugadores)
            var chunksToUnload = new List<Vector3Int>();
            foreach (var kvp in _loadedChunks)
            {
                var chunkWorldPos = ChunkToWorldPos(kvp.Key);
                bool nearAnyPlayer = false;

                foreach (var playerPos in playerPositions)
                {
                    float dist = Vector3.Distance(chunkWorldPos, playerPos);
                    if (dist < (renderDistance + 2) * Chunk.Size)
                    {
                        nearAnyPlayer = true;
                        break;
                    }
                }

                if (!nearAnyPlayer)
                    chunksToUnload.Add(kvp.Key);
            }

            // Descargar (máximo 5 por frame para no causar lag)
            foreach (var pos in chunksToUnload.Take(5))
            {
                UnloadChunk(pos);
            }
        }

        /// <summary>
        /// Descarga un chunk.
        /// </summary>
        public void UnloadChunk(Vector3Int position)
        {
            if (_loadedChunks.TryGetValue(position, out var chunkNet))
            {
                chunkNet.GetComponent<NetworkObject>().Despawn(true);
                _loadedChunks.Remove(position);
                _dirtyChunks.Remove(position);
                OnChunkUnloaded?.Invoke(position);
                Debug.Log($"[ChunkManager] Chunk descargado: {position}");
            }
        }

        /// <summary>
        /// Convierte posición mundo a chunk.
        /// </summary>
        public static Vector3Int WorldToChunkPos(Vector3 worldPos)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x / Chunk.Size),
                Mathf.FloorToInt(worldPos.y / Chunk.Size),
                Mathf.FloorToInt(worldPos.z / Chunk.Size)
            );
        }

        /// <summary>
        /// Convierte posición chunk a mundo (centro del chunk).
        /// </summary>
        public static Vector3 ChunkToWorldPos(Vector3Int chunkPos)
        {
            return new Vector3(
                (chunkPos.x + 0.5f) * Chunk.Size,
                (chunkPos.y + 0.5f) * Chunk.Size,
                (chunkPos.z + 0.5f) * Chunk.Size
            );
        }

        /// <summary>
        /// Obtiene chunk en posición (sin crear).
        /// </summary>
        public ChunkNetworkBehaviour GetChunk(Vector3Int position)
        {
            return _loadedChunks.TryGetValue(position, out var chunk) ? chunk : null;
        }

        /// <summary>
        /// Fuerza guardado de todos los chunks cargados.
        /// </summary>
        public void SaveAllChunks()
        {
            foreach (var chunkNet in _loadedChunks.Values)
            {
                if (chunkNet.Chunk != null)
                {
                    // Usar SaveSystem existente
                    var path = System.IO.Path.Combine(Application.persistentDataPath, "world", $"chunk_{chunkNet.ChunkPosition.Value.x}_{chunkNet.ChunkPosition.Value.y}_{chunkNet.ChunkPosition.Value.z}.json");
                    VoxelSandbox.Core.SaveSystem.SaveChunk(chunkNet.Chunk, path);
                }
            }
            Debug.Log($"[ChunkManager] {_loadedChunks.Count} chunks guardados");
        }
    }
}