using NUnit.Framework;

namespace VoxelSandbox.Player
{
    public class StatsTests
    {
        [Test]
        public void ConsumeFood_HealsAndRestoresHunger()
        {
            var s = new Stats();
            s.TakeDamage(30);
            s.ConsumeFood(20);
            Assert.AreEqual(70 + 10, s.Health);
            Assert.AreEqual(100, s.Hunger);
        }
    }
}
