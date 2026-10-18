using NUnit.Framework;

namespace VoxelSandbox.UI
{
    public class HotbarTests
    {
        [Test]
        public void AddItem_IncrementsSlot()
        {
            var hotbar = new Hotbar();
            hotbar.AddItem(0, 1, 5);
            Assert.AreEqual(5, hotbar.GetItem(0));
        }

        [Test]
        public void ClearSlot_Resets()
        {
            var hotbar = new Hotbar();
            hotbar.AddItem(2, 1, 3);
            hotbar.ClearSlot(2);
            Assert.AreEqual(0, hotbar.GetItem(2));
        }
    }
}
