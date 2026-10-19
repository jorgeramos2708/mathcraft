using NUnit.Framework;
using UnityEngine;
using VoxelSandbox.World;

namespace VoxelSandbox.Multiplayer
{
    public class ChunkNetworkBehaviourTests
    {
        [Test]
        public void ChunkSerializer_FlattenUnflatten_RoundTrip()
        {
            var chunk = new Chunk(new Vector3Int(1, 2, 3));
            chunk.SetBlock(0, 0, 0, 10);
            chunk.SetBlock(15, 15, 15, 250);
            chunk.SetBlock(7, 3, 12, 42);

            byte[] flat = ChunkSerializer.Flatten(chunk.Blocks);
            byte[,,] restored = new byte[Chunk.Size, Chunk.Size, Chunk.Size];
            ChunkSerializer.Unflatten(flat, restored);

            Assert.AreEqual(10, restored[0, 0, 0]);
            Assert.AreEqual(250, restored[15, 15, 15]);
            Assert.AreEqual(42, restored[7, 3, 12]);
        }

        [Test]
        public void ChunkSerializer_Flatten_LengthMatchesChunkVolume()
        {
            var chunk = new Chunk(Vector3Int.zero);
            byte[] flat = ChunkSerializer.Flatten(chunk.Blocks);
            int expected = Chunk.Size * Chunk.Size * Chunk.Size;
            Assert.AreEqual(expected, flat.Length);
        }

        [Test]
        public void ChunkNetworkBehaviour_CanInstantiateChunkWithPosition()
        {
            var chunk = new Chunk(new Vector3Int(5, 6, 7));
            Assert.AreEqual(new Vector3Int(5, 6, 7), chunk.Position);
            Assert.AreEqual(Chunk.Size, chunk.Blocks.GetLength(0));
        }
    }
}
