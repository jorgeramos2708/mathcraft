using NUnit.Framework;
using UnityEngine;
using System.IO;

namespace VoxelSandbox.Core
{
    public class SaveSystemTests
    {
        [Test]
        public void SaveAndLoadChunk_RoundTrip()
        {
            var chunk = new Chunk(new Vector3Int(1,2,3));
            chunk.SetBlock(5,5,5, 7);
            chunk.SetBlock(0,0,0, 9);

            string path = Path.Combine(Application.temporaryCachePath, "test_chunk.json");
            SaveSystem.SaveChunk(chunk, path);

            var loaded = SaveSystem.LoadChunk(path);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(chunk.Position, loaded.Position);
            Assert.AreEqual(7, loaded.GetBlock(5,5,5));
            Assert.AreEqual(9, loaded.GetBlock(0,0,0));

            File.Delete(path);
        }
    }
}
