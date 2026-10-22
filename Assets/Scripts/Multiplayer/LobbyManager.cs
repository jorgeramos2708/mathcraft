using System;
using System.Collections.Generic;
using UnityEngine;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Gestor de lobbies/salas para partidas multijugador.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private int maxLobbies = 100;
        [SerializeField] private float lobbyTimeoutMinutes = 30f;

        private Dictionary<string, Lobby> _lobbies = new Dictionary<string, Lobby>();

        public event Action<Lobby> OnLobbyCreated;
        public event Action<Lobby> OnLobbyDestroyed;
        public event Action<Lobby, PlayerSession> OnPlayerJoined;
        public event Action<Lobby, PlayerSession> OnPlayerLeft;
        public event Action<Lobby> OnLobbyStateChanged;

        public IReadOnlyDictionary<string, Lobby> Lobbies => _lobbies;

        /// <summary>
        /// Crea un nuevo lobby.
        /// </summary>
        public Lobby CreateLobby(PlayerSession owner, LobbySettings settings = null)
        {
            if (_lobbies.Count >= maxLobbies)
            {
                Debug.LogWarning("[LobbyManager] Límite de lobbies alcanzado");
                return null;
            }

            settings ??= new LobbySettings();

            var lobby = new Lobby
            {
                LobbyId = GenerateLobbyId(),
                Owner = owner,
                Players = new List<PlayerSession> { owner },
                Settings = settings,
                CreatedAt = DateTime.UtcNow,
                State = LobbyState.Waiting
            };

            _lobbies[lobby.LobbyId] = lobby;
            OnLobbyCreated?.Invoke(lobby);
            OnLobbyStateChanged?.Invoke(lobby);

            Debug.Log($"[LobbyManager] Lobby creado: {lobby.LobbyId} por {owner.ClientId}");
            return lobby;
        }

        /// <summary>
        /// Une a un jugador a un lobby existente.
        /// </summary>
        public bool JoinLobby(string lobbyId, PlayerSession player)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby))
            {
                Debug.LogWarning($"[LobbyManager] Lobby no encontrado: {lobbyId}");
                return false;
            }

            if (lobby.Players.Count >= lobby.Settings.MaxPlayers)
            {
                Debug.LogWarning($"[LobbyManager] Lobby lleno: {lobbyId}");
                return false;
            }

            if (lobby.State != LobbyState.Waiting)
            {
                Debug.LogWarning($"[LobbyManager] Lobby no está en estado Waiting: {lobby.State}");
                return false;
            }

            if (lobby.Settings.IsPrivate && !lobby.Settings.AllowedPlayers.Contains(player.ClientId))
            {
                Debug.LogWarning($"[LobbyManager] Lobby privado, jugador no autorizado: {player.ClientId}");
                return false;
            }

            lobby.Players.Add(player);
            OnPlayerJoined?.Invoke(lobby, player);
            OnLobbyStateChanged?.Invoke(lobby);

            Debug.Log($"[LobbyManager] Jugador {player.ClientId} unió a lobby {lobbyId} ({lobby.Players.Count}/{lobby.Settings.MaxPlayers})");
            return true;
        }

        /// <summary>
        /// Sacar a un jugador de un lobby.
        /// </summary>
        public bool LeaveLobby(string lobbyId, ulong clientId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return false;

            var player = lobby.Players.FirstOrDefault(p => p.ClientId == clientId);
            if (player == null) return false;

            lobby.Players.Remove(player);
            OnPlayerLeft?.Invoke(lobby, player);

            // Si el owner se va, transferir ownership
            if (lobby.Owner.ClientId == clientId)
            {
                if (lobby.Players.Count > 0)
                {
                    lobby.Owner = lobby.Players[0];
                    Debug.Log($"[LobbyManager] Nuevo owner para {lobbyId}: {lobby.Owner.ClientId}");
                }
                else
                {
                    // Lobby vacío, destruir
                    DestroyLobby(lobbyId);
                    return true;
                }
            }

            OnLobbyStateChanged?.Invoke(lobby);
            return true;
        }

        /// <summary>
        /// Cambia el estado del lobby (Waiting -> Starting -> InGame).
        /// </summary>
        public bool SetLobbyState(string lobbyId, LobbyState newState)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return false;

            lobby.State = newState;
            lobby.StateChangedAt = DateTime.UtcNow;
            OnLobbyStateChanged?.Invoke(lobby);

            Debug.Log($"[LobbyManager] Lobby {lobbyId} estado: {newState}");
            return true;
        }

        /// <summary>
        /// Destruye un lobby.
        /// </summary>
        public void DestroyLobby(string lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return;

            _lobbies.Remove(lobbyId);
            OnLobbyDestroyed?.Invoke(lobby);
            Debug.Log($"[LobbyManager] Lobby destruido: {lobbyId}");
        }

        /// <summary>
        /// Obtiene un lobby por ID.
        /// </summary>
        public Lobby GetLobby(string lobbyId)
        {
            return _lobbies.TryGetValue(lobbyId, out var lobby) ? lobby : null;
        }

        /// <summary>
        /// Obtiene lobbies disponibles para mostrar en lista.
        /// </summary>
        public List<Lobby> GetPublicLobbies()
        {
            return _lobbies.Values
                .Where(l => !l.Settings.IsPrivate && l.State == LobbyState.Waiting)
                .OrderByDescending(l => l.CreatedAt)
                .ToList();
        }

        private void Update()
        {
            // Limpiar lobbies abandonados
            var expired = _lobbies.Values
                .Where(l => (DateTime.UtcNow - l.CreatedAt).TotalMinutes > lobbyTimeoutMinutes 
                         && l.State == LobbyState.Waiting)
                .Select(l => l.LobbyId)
                .ToList();

            foreach (var id in expired)
            {
                Debug.Log($"[LobbyManager] Lobby expirado: {id}");
                DestroyLobby(id);
            }
        }

        private string GenerateLobbyId()
        {
            return $"lobby_{Guid.NewGuid().ToString("N")[..8]}";
        }

        [Serializable]
        public class Lobby
        {
            public string LobbyId;
            public PlayerSession Owner;
            public List<PlayerSession> Players;
            public LobbySettings Settings;
            public DateTime CreatedAt;
            public DateTime StateChangedAt;
            public LobbyState State;
            public int EducationLevel;
            public string GameMode;

            public int CurrentPlayers => Players.Count;
            public bool IsFull => CurrentPlayers >= Settings.MaxPlayers;
            public bool HasOwner => Owner != null;
        }

        [Serializable]
        public class LobbySettings
        {
            public int MaxPlayers = 8;
            public bool IsPrivate = false;
            public List<ulong> AllowedPlayers = new List<ulong>();
            public int EducationLevel = 1;
            public GameMode GameMode = GameMode.Survival;
            public bool AllowSpectators = false;
        }

        public enum LobbyState
        {
            Waiting,
            Starting,
            InGame,
            Ended
        }
    }
}