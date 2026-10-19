using UnityEngine;

namespace VoxelSandbox.Education
{
    public class PuzzleUI : MonoBehaviour
    {
        private ProblemGenerator _gen = new ProblemGenerator();
        private RewardManager _rewards = new RewardManager();

        public void ShowPuzzle(int level)
        {
            var p = _gen.Generate(level);
            Debug.Log($"Solve: {p.a} {p.op} {p.b} = ?");
        }

        public void SubmitAnswer(int level, int answer)
        {
            var p = _gen.Generate(level);
            if (MathService.CheckAnswer(p.a, p.b, p.op, answer))
            {
                _rewards.Unlock($"map_level_{level}");
            }
        }
    }
}
