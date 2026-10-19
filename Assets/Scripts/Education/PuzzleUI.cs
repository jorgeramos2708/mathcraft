using UnityEngine;

namespace VoxelSandbox.Education
{
    public class PuzzleUI : MonoBehaviour
    {
        private MathController _controller = new MathController();

        public void ShowPuzzle(int level)
        {
            var p = _controller.GetProblem(level);
            Debug.Log($"Solve: {p.a} {p.op} {p.b} = ?");
        }

        public void SubmitAnswer(int level, int a, int b, char op, int answer)
        {
            _controller.SubmitAnswer(level, a, b, op, answer);
        }
    }
}
