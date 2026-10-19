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

        [Test]
        public void AddProgress_IncrementsProgress()
        {
            var rm = new RewardManager();
            rm.AddProgress("quiz_master", 3);
            Assert.AreEqual(3, rm.GetProgress("quiz_master"));
            Assert.AreEqual(0, rm.GetLevel("quiz_master"));
        }

        [Test]
        public void AddProgress_LevelUpWhenThresholdReached()
        {
            var rm = new RewardManager();
            rm.AddProgress("quiz_master", 10);
            Assert.AreEqual(0, rm.GetProgress("quiz_master"));
            Assert.AreEqual(1, rm.GetLevel("quiz_master"));
            Assert.IsTrue(rm.IsLevelUnlocked("quiz_master", 1));
        }

        [Test]
        public void AddProgress_MultipleLevels()
        {
            var rm = new RewardManager();
            rm.AddProgress("quiz_master", 25);
            // 25 points = 2 levels (20) + 5 progress
            Assert.AreEqual(5, rm.GetProgress("quiz_master"));
            Assert.AreEqual(2, rm.GetLevel("quiz_master"));
            Assert.IsTrue(rm.IsLevelUnlocked("quiz_master", 2));
            Assert.IsFalse(rm.IsLevelUnlocked("quiz_master", 3));
        }

        [Test]
        public void GetLevel_UnknownAchievement_ReturnsZero()
        {
            var rm = new RewardManager();
            Assert.AreEqual(0, rm.GetLevel("nonexistent"));
            Assert.AreEqual(0, rm.GetProgress("nonexistent"));
        }

        [Test]
        public void AddProgress_ZeroOrNegative_Ignores()
        {
            var rm = new RewardManager();
            rm.AddProgress("quiz_master", 0);
            rm.AddProgress("quiz_master", -5);
            Assert.AreEqual(0, rm.GetProgress("quiz_master"));
            Assert.AreEqual(0, rm.GetLevel("quiz_master"));
        }
    }
}
