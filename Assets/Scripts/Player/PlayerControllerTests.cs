using NUnit.Framework;

namespace VoxelSandbox.Player
{
    public class PlayerControllerTests
    {
        [Test]
        public void PlayerController_HasDefaultSpeed()
        {
            Assert.AreEqual(5f, new PlayerController().speed);
        }
    }
}
