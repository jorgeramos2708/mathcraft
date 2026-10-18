using NUnit.Framework;
using System.Collections.Generic;

namespace VoxelSandbox.Core
{
    public class CraftingTests
    {
        [Test]
        public void TryCraft_UnknownRecipe_Fails()
        {
            var inputs = new Dictionary<int,int> { {1,1} };
            bool ok = Crafting.TryCraft(inputs, out int id, out int count);
            Assert.IsFalse(ok);
        }

        [Test]
        public void RegisterAndCraft_Works()
        {
            Crafting.RegisterRecipe("1:2", 2);
            var inputs = new Dictionary<int,int> { {1,2} };
            bool ok = Crafting.TryCraft(inputs, out int id, out int count);
            Assert.IsTrue(ok);
            Assert.AreEqual(2, id);
        }
    }
}
