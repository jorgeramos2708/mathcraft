using UnityEngine;

namespace VoxelSandbox.Education
{
    public static class MathService
    {
        public static int GenerateProblem(int level, out int a, out int b, out char op)
        {
            System.Random rng = new System.Random();
            a = rng.Next(1, level * 10);
            b = rng.Next(1, level * 10);
            op = level < 2 ? '+' : rng.Next(2) == 0 ? '+' : '*';
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
