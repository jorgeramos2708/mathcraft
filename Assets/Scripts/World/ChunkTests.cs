using NUnit.Framework;
using UnityEngine;
using VoxelSandbox.World;

namespace VoxelSandbox.World.Tests
{
    public class ChunkTests
    {
        [Test]
        public void GetSetBlock_WithinBounds_Works()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(5, 5, 5, 7);
            Assert.AreEqual(7, chunk.GetBlock(5, 5, 5));
        }

        [Test]
        public void GetBlock_OutOfBounds_ReturnsZero()
        {
            var chunk = new Chunk(Vector3Int.zero);
            Assert.AreEqual(0, chunk.GetBlock(-1, 0, 0));
            Assert.AreEqual(0, chunk.GetBlock(Chunk.Size, 0, 0));
            Assert.AreEqual(0, chunk.GetBlock(0, -1, 0));
        }

        [Test]
        public void SetBlock_OutOfBounds_DoesNotThrow()
        {
            var chunk = new Chunk(Vector3Int.zero);
            Assert.DoesNotThrow(() => chunk.SetBlock(-1, 0, 0, 1));
            Assert.DoesNotThrow(() => chunk.SetBlock(0, Chunk.Size, 0, 1));
        }

        [Test]
        public void Clear_ResetsAllBlocks()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(0, 0, 0, 1);
            chunk.SetBlock(15, 15, 15, 2);
            chunk.Clear();
            for (int x = 0; x < Chunk.Size; x++)
                for (int y = 0; y < Chunk.Size; y++)
                    for (int z = 0; z < Chunk.Size; z++)
                        Assert.AreEqual(0, chunk.GetBlock(x, y, z));
        }

        [Test]
        public void SetPosition_UpdatesPosition()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetPosition(new Vector3Int(1, 2, 3));
            Assert.AreEqual(new Vector3Int(1, 2, 3), chunk.Position);
        }
    }
}
