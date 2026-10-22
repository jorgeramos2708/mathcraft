using UnityEngine;

namespace VoxelSandbox.Multiplayer
{
    [CreateAssetMenu(menuName = "VoxelSandbox/Server Config")]
    public class ServerConfig : ScriptableObject
    {
        public int Port = 7777;
        public int MaxPlayers = 16;
        public string Motd = "Voxel Sandbox Dedicated Server";
    }
}
