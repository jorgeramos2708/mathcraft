using System;

namespace VoxelSandbox.Core
{
    public static class ErrorHandler
    {
        public static T SafeExecute<T>(Func<T> action, T fallback, string context)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                Logger.Error($"{context} failed: {ex.Message}");
                return fallback;
            }
        }

        public static void SafeExecute(Action action, string context)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Error($"{context} failed: {ex.Message}");
            }
        }
    }
}
