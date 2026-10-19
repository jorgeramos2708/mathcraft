using NUnit.Framework;

namespace VoxelSandbox.Education
{
    public class MathServiceTests
    {
        [Test]
        public void CheckAnswer_Correct()
        {
            bool ok = MathService.CheckAnswer(2,3,'+',5);
            Assert.IsTrue(ok);
        }

        [Test]
        public void CheckAnswer_Incorrect()
        {
            bool ok = MathService.CheckAnswer(2,3,'+',6);
            Assert.IsFalse(ok);
        }
    }
}
