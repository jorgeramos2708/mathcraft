using NUnit.Framework;
using UnityEngine;

namespace VoxelSandbox.World
{
    public class BlockInteractionTests
    {
        [Test]
        public void BreakBlock_RemovesBlock()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(5,5,5,1);
            bool ok = BlockInteraction.TryBreakBlock(chunk, new Vector3Int(5,5,5));
            Assert.IsTrue(ok);
            Assert.AreEqual(0, chunk.GetBlock(5,5,5));
        }

        [Test]
        public void PlaceBlock_AddsBlock()
        {
            var chunk = new Chunk(Vector3Int.zero);
            bool ok = BlockInteraction.TryPlaceBlock(chunk, new Vector3Int(3,3,3));
            Assert.IsTrue(ok);
            Assert.AreEqual(1, chunk.GetBlock(3,3,3));
        }

        [Test]
        public void PlaceBlock_FailsIfOccupied()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(1,1,1,1);
            bool ok = BlockInteraction.TryPlaceBlock(chunk, new Vector3Int(1,1,1));
            Assert.IsFalse(ok);
        }
    }
}
