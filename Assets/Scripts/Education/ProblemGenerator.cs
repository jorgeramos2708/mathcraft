using UnityEngine;

namespace VoxelSandbox.Education
{
    public class ProblemGenerator
    {
        public (int a, int b, char op, int answer) Generate(int level)
        {
            int a, b;
            char op;
            int ans = MathService.GenerateProblem(level, out a, out b, out op);
            return (a, b, op, ans);
        }
    }
}
