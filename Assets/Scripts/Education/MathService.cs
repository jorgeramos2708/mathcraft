using UnityEngine;

namespace VoxelSandbox.Education
{
    public static class MathService
    {
        public static int GenerateProblem(int level, out int a, out int b, out char op)
        {
            a = Random.Range(1, level * 10 + 1);
            b = Random.Range(1, level * 10 + 1);
            op = level < 2 ? '+' : Random.value < 0.5f ? '+' : '*';
            int result = op == '+' ? a + b : a * b;
            return result;
        }

        public static bool CheckAnswer(int a, int b, char op, int answer)
        {
            int correct = op == '+' ? a + b : a * b;
            return answer == correct;
        }
    }
}
