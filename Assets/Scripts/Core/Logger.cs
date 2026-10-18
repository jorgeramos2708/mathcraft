using UnityEngine;

namespace VoxelSandbox.Core
{
    public static class Logger
    {
        public static void Info(string message) => Debug.Log($"[INFO] {message}");
        public static void Warn(string message) => Debug.LogWarning($"[WARN] {message}");
        public static void Error(string message) => Debug.LogError($"[ERROR] {message}");
    }
}
