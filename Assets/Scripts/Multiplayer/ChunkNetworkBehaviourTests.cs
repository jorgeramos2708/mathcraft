using NUnit.Framework;
using UnityEngine;
using VoxelSandbox.World;
using VoxelSandbox.Core;

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

        [Test]
        public void ChunkSerializer_ClientApply_UnflattenAppliesDataCorrectly()
        {
            var sourceChunk = new Chunk(new Vector3Int(1, 1, 1));
            sourceChunk.SetBlock(0, 0, 0, 33);
            sourceChunk.SetBlock(15, 15, 15, 77);
            sourceChunk.SetBlock(7, 3, 12, 99);

            byte[] flat = ChunkSerializer.Flatten(sourceChunk.Blocks);

            var clientChunk = new Chunk(new Vector3Int(1, 1, 1));
            ChunkSerializer.Unflatten(flat, clientChunk.Blocks);

            Assert.AreEqual(33, clientChunk.GetBlock(0, 0, 0));
            Assert.AreEqual(77, clientChunk.GetBlock(15, 15, 15));
            Assert.AreEqual(99, clientChunk.GetBlock(7, 3, 12));
        }

        [Test]
        public void ChunkSerializer_Unflatten_IgnoresInvalidLength()
        {
            var chunk = new Chunk(Vector3Int.zero);
            chunk.SetBlock(0, 0, 0, 55);
            byte[] badFlat = new byte[10];
            ChunkSerializer.Unflatten(badFlat, chunk.Blocks);
            Assert.AreEqual(55, chunk.GetBlock(0, 0, 0), "Unflatten should not modify blocks when length is invalid");
        }
    }
}
