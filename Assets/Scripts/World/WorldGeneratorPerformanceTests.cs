using NUnit.Framework;
using UnityEngine;
using VoxelSandbox.World;

namespace VoxelSandbox.World.Tests
{
    public class WorldGeneratorPerformanceTests
    {
        [SetUp]
        public void SetUp()
        {
            WorldGenerator.ClearCache();
        }

        [Test]
        public void GenerateChunk_100Chunks_CompletesQuickly()
        {
            var start = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                var pos = new Vector3Int(i % 10, 0, i / 10);
                WorldGenerator.GenerateChunk(pos, seed: 42);
            }
            start.Stop();
            // Expect under 200ms on typical CI; adjust as needed
            Assert.Less(start.ElapsedMilliseconds, 200, $"100 chunk generation took {start.ElapsedMilliseconds}ms");
        }

        [Test]
        public void GenerateChunk_Cache_ReturnsSameInstance()
        {
            var pos = new Vector3Int(1, 0, 1);
            var a = WorldGenerator.GenerateChunk(pos, seed: 123);
            var b = WorldGenerator.GenerateChunk(pos, seed: 123);
            Assert.AreSame(a, b);
        }

        [Test]
        public void GenerateChunk_DifferentSeed_DifferentInstances()
        {
            var pos = new Vector3Int(1, 0, 1);
            var a = WorldGenerator.GenerateChunk(pos, seed: 1);
            var b = WorldGenerator.GenerateChunk(pos, seed: 2);
            Assert.AreNotSame(a, b);
            // Ensure at least one block differs
            bool differs = false;
            for (int x = 0; x < Chunk.Size && !differs; x++)
                for (int y = 0; y < Chunk.Size && !differs; y++)
                    for (int z = 0; z < Chunk.Size && !differs; z++)
                        if (a.GetBlock(x, y, z) != b.GetBlock(x, y, z))
                            differs = true;
            Assert.IsTrue(differs);
        }

        [Test]
        public void GenerateChunks_Batch_ReturnsCorrectCount()
        {
            var chunks = WorldGenerator.GenerateChunks(Vector3Int.zero, 10, 10, seed: 0);
            Assert.AreEqual(100, chunks.Length);
        }
    }
}
