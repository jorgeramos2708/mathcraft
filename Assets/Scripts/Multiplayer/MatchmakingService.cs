using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Servicio de matchmaking que empareja jugadores por nivel educativo y preferencias.
    /// </summary>
    public class MatchmakingService : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private float matchCheckInterval = 2f;
        [SerializeField] private int minPlayersPerMatch = 2;
        [SerializeField] private int maxPlayersPerMatch = 8;

        private List<MatchmakingRequest> _queue = new List<MatchmakingRequest>();
        private Dictionary<string, LobbyManager.Lobby> _activeLobbies = new Dictionary<string, LobbyManager.Lobby>();
        private float _lastCheckTime;

        public event Action<MatchmakingRequest> OnPlayerQueued;
        public event Action<MatchmakingRequest> OnPlayerDequeued;
        public event Action<string, List<PlayerSession>> OnMatchFound;

        private void Update()
        {
            if (Time.time - _lastCheckTime >= matchCheckInterval)
            {
                _lastCheckTime = Time.time;
                ProcessQueue();
            }
        }

        /// <summary>
        /// Añade un jugador a la cola de matchmaking.
        /// </summary>
        public void JoinQueue(PlayerSession player, MatchmakingPreferences preferences)
        {
            if (_queue.Any(r => r.Player.ClientId == player.ClientId))
            {
                Debug.LogWarning($"[Matchmaking] Jugador {player.ClientId} ya está en cola");
                return;
            }

            var request = new MatchmakingRequest
            {
                Player = player,
                Preferences = preferences,
                QueuedAt = DateTime.UtcNow
            };

            _queue.Add(request);
            OnPlayerQueued?.Invoke(request);
            Debug.Log($"[Matchmaking] Jugador {player.ClientId} en cola (Total: {_queue.Count})");
        }

        /// <summary>
        /// Retira un jugador de la cola.
        /// </summary>
        public void LeaveQueue(ulong clientId)
        {
            var request = _queue.FirstOrDefault(r => r.Player.ClientId == clientId);
            if (request != null)
            {
                _queue.Remove(request);
                OnPlayerDequeued?.Invoke(request);
                Debug.Log($"[Matchmaking] Jugador {clientId} salió de la cola (Restantes: {_queue.Count})");
            }
        }

        /// <summary>
        /// Procesa la cola buscando coincidencias compatibles.
        /// </summary>
        private void ProcessQueue()
        {
            if (_queue.Count < minPlayersPerMatch) return;

            // Agrupar por nivel educativo preferido
            var grouped = _queue
                .GroupBy(r => r.Preferences.PreferredEducationLevel)
                .Where(g => g.Count() >= minPlayersPerMatch)
                .OrderByDescending(g => g.Count());

            foreach (var group in grouped)
            {
                var candidates = group.ToList();
                
                while (candidates.Count >= minPlayersPerMatch)
                {
                    var matchPlayers = candidates.Take(maxPlayersPerMatch).ToList();
                    candidates.RemoveRange(0, matchPlayers.Count);

                    // Retirar de la cola principal
                    foreach (var matched in matchPlayers)
                        _queue.Remove(matched);

                    // Crear lobby para este match
                    var lobbyId = CreateLobbyForMatch(matchPlayers);
                    
                    var sessions = matchPlayers.Select(m => m.Player).ToList();
                    OnMatchFound?.Invoke(lobbyId, sessions);
                    
                    Debug.Log($"[Matchmaking] Match creado: {lobbyId} con {sessions.Count} jugadores (Nivel: {group.Key})");
                }
            }
        }

        private string CreateLobbyForMatch(List<MatchmakingRequest> players)
        {
            var lobbyId = $"lobby_{Guid.NewGuid().ToString("N")[..8]}";
            var lobby = new LobbyManager.Lobby
            {
                LobbyId = lobbyId,
                Owner = players[0].Player,
                Players = players.Select(p => p.Player).ToList(),
                CreatedAt = DateTime.UtcNow,
                EducationLevel = players[0].Preferences.PreferredEducationLevel,
                IsPrivate = false
            };

            _activeLobbies[lobbyId] = lobby;
            return lobbyId;
        }

        public IReadOnlyList<MatchmakingRequest> GetQueueSnapshot() => _queue.AsReadOnly();
        public IReadOnlyDictionary<string, LobbyManager.Lobby> GetActiveLobbies() => _activeLobbies;

        public void RemoveLobby(string lobbyId)
        {
            _activeLobbies.Remove(lobbyId);
        }
    }

    [Serializable]
    public class MatchmakingPreferences
    {
        public int PreferredEducationLevel = 1; // 1=Secundaria, 2=Bachillerato, etc.
        public GameMode PreferredMode = GameMode.Survival;
        public string Region = "auto";
        public bool AllowCrossLevel = true;
    }

    public enum GameMode
    {
        Survival,
        Creative,
        Adventure,
        Education
    }

    [Serializable]
    public class MatchmakingRequest
    {
        public PlayerSession Player;
        public MatchmakingPreferences Preferences;
        public DateTime QueuedAt;
        public int WaitTimeSeconds => (int)(DateTime.UtcNow - QueuedAt).TotalSeconds;
    }
}