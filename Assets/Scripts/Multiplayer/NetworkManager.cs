using Unity.Netcode;
using UnityEngine;
using VoxelSandbox.Multiplayer;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// NetworkManager wrapper que integra Unity Netcode for GameObjects
    /// con nuestra configuración personalizada para servidor dedicado y cliente.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private ServerConfig serverConfig;
        [SerializeField] private bool isDedicatedServer = false;
        
        [Header("Prefabs de Red")]
        [SerializeField] private NetworkObject playerPrefab;
        
        private Unity.Netcode.NetworkManager _netcodeManager;
        
        public bool IsServer => _netcodeManager.IsServer;
        public bool IsClient => _netcodeManager.IsClient;
        public bool IsHost => _netcodeManager.IsHost;
        public ulong LocalClientId => _netcodeManager.LocalClientId;

        private void Awake()
        {
            _netcodeManager = Unity.Netcode.NetworkManager.Singleton;
            if (_netcodeManager == null)
            {
                var go = new GameObject("NetcodeManager");
                _netcodeManager = go.AddComponent<Unity.Netcode.NetworkManager>();
                DontDestroyOnLoad(go);
            }
            
            ConfigureNetworkManager();
        }

        private void ConfigureNetworkManager()
        {
            if (serverConfig != null)
            {
                var transport = _netcodeManager.GetComponent<UnityTransport>();
                if (transport != null)
                {
                    transport.ConnectionData.Port = (ushort)serverConfig.Port;
                    transport.ConnectionData.Address = "0.0.0.0";
                }
            }

            if (playerPrefab != null)
            {
                _netcodeManager.NetworkConfig.PlayerPrefab = playerPrefab;
            }
        }

        public void StartDedicatedServer()
        {
            isDedicatedServer = true;
            Debug.Log($"[NetworkManager] Iniciando servidor dedicado en puerto {serverConfig?.Port ?? 7777}");
            _netcodeManager.StartServer();
        }

        public void StartHost()
        {
            Debug.Log("[NetworkManager] Iniciando host (servidor + cliente local)");
            _netcodeManager.StartHost();
        }

        public void JoinServer(string address = "127.0.0.1", ushort port = 7777)
        {
            var transport = _netcodeManager.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = address;
                transport.ConnectionData.Port = port;
            }
            Debug.Log($"[NetworkManager] Conectando a {address}:{port}");
            _netcodeManager.StartClient();
        }

        public void StopNetwork()
        {
            if (_netcodeManager.IsListening)
            {
                _netcodeManager.Shutdown();
                Debug.Log("[NetworkManager] Red detenida");
            }
        }

        private void OnDestroy()
        {
            StopNetwork();
        }
    }
}