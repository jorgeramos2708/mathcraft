using NUnit.Framework;

namespace VoxelSandbox.Education
{
    public class RewardManagerTests
    {
        [Test]
        public void Unlock_MarksAsUnlocked()
        {
            var rm = new RewardManager();
            rm.Unlock("map_1");
            Assert.IsTrue(rm.IsUnlocked("map_1"));
        }
    }
}
