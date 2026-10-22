using Unity.Netcode;
using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    public class DedicatedServerManager : MonoBehaviour
    {
        [SerializeField] private int port = 7777;
        [SerializeField] private int maxConnections = 16;

        public void StartServer()
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log($"Dedicated server started on port {port}");
        }

        public void StopServer()
        {
            NetworkManager.Singleton.StopServer();
        }
    }
}
