using NUnit.Framework;
using UnityEngine;

namespace VoxelSandbox.World
{
    public class WorldGeneratorTests
    {
        [Test]
        public void GenerateChunk_HasSolidBase()
        {
            var chunk = WorldGenerator.GenerateChunk(Vector3Int.zero, seed: 42);
            // Check that at least some blocks are placed
            int filled = 0;
            for (int x = 0; x < Chunk.Size; x++)
            for (int y = 0; y < Chunk.Size; y++)
            for (int z = 0; z < Chunk.Size; z++)
                if (chunk.GetBlock(x, y, z) != 0) filled++;
            Assert.Greater(filled, 0);
        }

        [Test]
        public void GenerateChunk_DeterministicWithSeed()
        {
            var a = WorldGenerator.GenerateChunk(Vector3Int.zero, seed: 123);
            var b = WorldGenerator.GenerateChunk(Vector3Int.zero, seed: 123);
            for (int x = 0; x < Chunk.Size; x++)
            for (int y = 0; y < Chunk.Size; y++)
            for (int z = 0; z < Chunk.Size; z++)
                Assert.AreEqual(a.GetBlock(x, y, z), b.GetBlock(x, y, z));
        }
    }
}
