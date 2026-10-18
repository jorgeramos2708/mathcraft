using System.Collections.Generic;

namespace VoxelSandbox.Core
{
    public static class Crafting
    {
        private static readonly Dictionary<string, int> _recipes = new Dictionary<string, int>
        {
            { "1:2->2", 2 }
        };

        public static bool TryCraft(Dictionary<int,int> inputs, out int outputId, out int outputCount)
        {
            outputId = 0;
            outputCount = 0;
            string key = BuildKey(inputs);
            if (_recipes.TryGetValue(key, out int result))
            {
                outputId = result;
                outputCount = 1;
                return true;
            }
            return false;
        }

        private static string BuildKey(Dictionary<int,int> inputs)
        {
            var parts = new List<string>();
            foreach (var kv in inputs)
                parts.Add($"{kv.Key}:{kv.Value}");
            parts.Sort();
            return string.Join("+", parts);
        }

        public static void RegisterRecipe(string inputKey, int outputId)
        {
            _recipes[inputKey] = outputId;
        }
    }
}
