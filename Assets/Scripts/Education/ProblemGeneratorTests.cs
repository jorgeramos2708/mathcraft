using NUnit.Framework;

namespace VoxelSandbox.Education
{
    public class ProblemGeneratorTests
    {
        [Test]
        public void Generate_ReturnsValidProblem()
        {
            var gen = new ProblemGenerator();
            var p = gen.Generate(1);
            Assert.Greater(p.a, 0);
            Assert.Greater(p.b, 0);
        }
    }
}
