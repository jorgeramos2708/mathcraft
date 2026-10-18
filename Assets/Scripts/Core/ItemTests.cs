using NUnit.Framework;

namespace VoxelSandbox.Core
{
    public class ItemTests
    {
        [Test]
        public void Item_CreatesWithDefaults()
        {
            var item = new Item(1, "Wood");
            Assert.AreEqual(1, item.Id);
            Assert.AreEqual("Wood", item.Name);
            Assert.AreEqual(64, item.StackSize);
        }
    }
}
