using NUnit.Framework;

namespace VoxelSandbox.Player
{
    public class InventoryTests
    {
        [Test]
        public void AddAndRemoveItem_Works()
        {
            var inv = new Inventory();
            inv.AddItem(1, 10);
            Assert.AreEqual(10, inv.GetCount(1));
            inv.RemoveItem(1, 4);
            Assert.AreEqual(6, inv.GetCount(1));
        }

        [Test]
        public void RemoveItem_FailsIfNotEnough()
        {
            var inv = new Inventory();
            inv.AddItem(2, 3);
            bool ok = inv.RemoveItem(2, 5);
            Assert.IsFalse(ok);
        }
    }
}
