using UnityEngine;

namespace VoxelSandbox.Education
{
    public class MathController
    {
        private ProblemGenerator _gen = new ProblemGenerator();
        private RewardManager _rewards = new RewardManager();

        public (int a,int b,char op,int answer) GetProblem(int level) => _gen.Generate(level);

        public bool SubmitAnswer(int level, int a, int b, char op, int answer)
        {
            if (MathService.CheckAnswer(a,b,op,answer))
            {
                _rewards.Unlock($"map_level_{level}");
                return true;
            }
            return false;
        }
    }
}
