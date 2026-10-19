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

        [Test]
        public void SingleBlock_GreedyQuads_IsSix()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(8, 8, 8, 1);
            int quads = Mesher.CountGreedyQuads(chunk);
            Assert.AreEqual(6, quads);
        }

        [Test]
        public void TwoAdjacentBlocks_GreedyQuads_ReducesCount()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(8, 8, 8, 1);
            chunk.SetBlock(9, 8, 8, 1);
            int visibleFaces = Mesher.CountVisibleFaces(chunk);
            int greedyQuads = Mesher.CountGreedyQuads(chunk);
            Assert.AreEqual(10, visibleFaces);
            Assert.Less(greedyQuads, visibleFaces);
            Assert.AreEqual(6, greedyQuads);
        }

        [Test]
        public void Cache_ReturnsConsistentResult()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(4, 4, 4, 2);
            int first = Mesher.CountVisibleFaces(chunk);
            int second = Mesher.CountVisibleFaces(chunk);
            Assert.AreEqual(first, second);

            int firstGreedy = Mesher.CountGreedyQuads(chunk);
            int secondGreedy = Mesher.CountGreedyQuads(chunk);
            Assert.AreEqual(firstGreedy, secondGreedy);
        }

        [Test]
        public void CacheInvalidation_ReflectsChanges()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(5, 5, 5, 1);
            int facesBefore = Mesher.CountVisibleFaces(chunk);
            Mesher.InvalidateCache(chunk);
            chunk.SetBlock(5, 5, 6, 1);
            int facesAfter = Mesher.CountVisibleFaces(chunk);
            Assert.Greater(facesAfter, facesBefore);
        }
    }
}
