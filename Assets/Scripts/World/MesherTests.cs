using NUnit.Framework;
using UnityEngine;

namespace VoxelSandbox.World
{
    public class MesherTests
    {
        [Test]
        public void SingleBlock_HasSixFaces()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(8, 8, 8, 1);
            int faces = Mesher.CountVisibleFaces(chunk);
            Assert.AreEqual(6, faces);
        }

        [Test]
        public void TwoAdjacentBlocks_HasTenFaces()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(8, 8, 8, 1);
            chunk.SetBlock(9, 8, 8, 1);
            int faces = Mesher.CountVisibleFaces(chunk);
            Assert.AreEqual(10, faces);
        }
    }
}
