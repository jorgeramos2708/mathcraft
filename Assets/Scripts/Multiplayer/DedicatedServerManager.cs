using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Gestor completo para servidor dedicado (headless).
    /// Maneja autenticación, ciclo de vida y autoridad de servidor.
    /// </summary>
    public class DedicatedServerManager : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private ServerConfig serverConfig;
        [SerializeField] private NetworkManager networkManager;
        
        [Header("Seguridad")]
        [SerializeField] private bool requireAuthentication = true;
        [SerializeField] private List<string> validTokens = new List<string>();

        public bool RequireAuthentication => requireAuthentication;
        public int ConnectedPlayers => connectedPlayers;
        public ServerConfig ServerConfig => serverConfig;

        [Header("Estado")]
        [SerializeField] private bool isRunning = false;
        [SerializeField] private int connectedPlayers = 0;

        public event Action<ulong> OnPlayerConnected;
        public event Action<ulong> OnPlayerDisconnected;
        public event Action<string> OnServerLog;

        public bool IsRunning => isRunning;

        private Dictionary<ulong, PlayerSession> _sessions = new Dictionary<ulong, PlayerSession>();

        private void Awake()
        {
            if (networkManager == null)
                networkManager = FindObjectOfType<NetworkManager>();
        }

        public void StartServer()
        {
            if (isRunning)
            {
                Log("El servidor ya está en ejecución");
                return;
            }

            if (serverConfig == null)
            {
                Log("ERROR: No hay ServerConfig asignado");
                return;
            }

            SubscribeToNetworkEvents();
            
            networkManager.StartDedicatedServer();
            isRunning = true;
            
            Log($"Servidor dedicado iniciado - Puerto: {serverConfig.Port}, Max Jugadores: {serverConfig.MaxPlayers}");
        }

        public void StopServer()
        {
            if (!isRunning) return;

            UnsubscribeFromNetworkEvents();
            networkManager.StopNetwork();
            _sessions.Clear();
            connectedPlayers = 0;
            isRunning = false;
            
            Log("Servidor detenido");
        }

        public void RestartServer()
        {
            StopServer();
            StartServer();
        }

        public bool ValidateToken(string token)
        {
            if (!requireAuthentication) return true;
            return validTokens.Contains(token);
        }

        public void AddValidToken(string token)
        {
            if (!validTokens.Contains(token))
                validTokens.Add(token);
        }

        public void RemoveValidToken(string token)
        {
            validTokens.Remove(token);
        }

        public PlayerSession GetSession(ulong clientId)
        {
            return _sessions.TryGetValue(clientId, out var session) ? session : null;
        }

        public IReadOnlyDictionary<ulong, PlayerSession> GetAllSessions()
        {
            return _sessions;
        }

        private void SubscribeToNetworkEvents()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void UnsubscribeFromNetworkEvents()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            connectedPlayers++;
            var session = new PlayerSession { ClientId = clientId, ConnectedAt = DateTime.UtcNow };
            _sessions[clientId] = session;
            
            OnPlayerConnected?.Invoke(clientId);
            Log($"Cliente conectado: {clientId} (Total: {connectedPlayers})");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            connectedPlayers--;
            _sessions.Remove(clientId);
            
            OnPlayerDisconnected?.Invoke(clientId);
            Log($"Cliente desconectado: {clientId} (Total: {connectedPlayers})");
        }

        private void Log(string message)
        {
            var logEntry = $"[{DateTime.UtcNow:HH:mm:ss}] {message}";
            Debug.Log(logEntry);
            OnServerLog?.Invoke(logEntry);
        }

        private void OnValidate()
        {
            if (serverConfig != null)
            {
                serverConfig.Port = Mathf.Clamp(serverConfig.Port, 1024, 65535);
                serverConfig.MaxPlayers = Mathf.Max(1, serverConfig.MaxPlayers);
            }
        }
    }

    [Serializable]
    public class PlayerSession
    {
        public ulong ClientId;
        public DateTime ConnectedAt;
        public string AuthToken;
        public bool IsAuthenticated;
    }
}