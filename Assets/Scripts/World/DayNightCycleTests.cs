using NUnit.Framework;
using UnityEngine;

namespace VoxelSandbox.World
{
    public class DayNightCycleTests
    {
        [Test]
        public void IsDay_StartsDay()
        {
            var cycle = new GameObject().AddComponent<DayNightCycle>();
            cycle.DayLength = 100f;
            Assert.IsTrue(cycle.IsDay);
        }
    }
}
