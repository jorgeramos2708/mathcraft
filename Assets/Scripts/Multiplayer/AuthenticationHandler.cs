using Unity.Netcode;
using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    /// <summary>
    /// Maneja la autenticación de clientes que se conectan al servidor.
    /// Se conecta al pipeline de conexión de Netcode.
    /// </summary>
    public class AuthenticationHandler : MonoBehaviour
    {
        [SerializeField] private DedicatedServerManager serverManager;
        
        private void Awake()
        {
            if (serverManager == null)
                serverManager = FindObjectOfType<DedicatedServerManager>();
        }

        private void Start()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // Interceptar conexiones antes de que se completen
                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            }
        }

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            // Obtener token del payload de conexión
            string token = "";
            if (request.Payload != null && request.Payload.Length > 0)
            {
                token = System.Text.Encoding.UTF8.GetString(request.Payload);
            }

            bool approved = true;
            string reason = "";

            if (serverManager != null && serverManager.RequireAuthentication)
            {
                if (string.IsNullOrEmpty(token) || !serverManager.ValidateToken(token))
                {
                    approved = false;
                    reason = "Token de autenticación inválido";
                }
            }

            // Verificar límite de jugadores
            if (approved && serverManager != null)
            {
                if (serverManager.ConnectedPlayers >= serverManager.ServerConfig.MaxPlayers)
                {
                    approved = false;
                    reason = "Servidor lleno";
                }
            }

            response.Approved = approved;
            response.Reason = reason;
            response.CreatePlayerObject = true;

            if (approved)
            {
                // Asignar posición de spawn inicial
                response.Position = new UnityEngine.Vector3(0, 10, 0);
                response.Rotation = UnityEngine.Quaternion.identity;
            }

            Debug.Log($"[Auth] Cliente {request.ClientNetworkId}: {(approved ? "APROBADO" : "RECHAZADO")} - {reason}");
        }
    }
}