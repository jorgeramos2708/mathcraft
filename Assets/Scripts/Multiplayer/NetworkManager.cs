using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    public class NetworkManager : MonoBehaviour
    {
        public void StartHost()
        {
            Debug.Log("Hosting server");
        }

        public void JoinServer()
        {
            Debug.Log("Joining server");
        }
    }
}
