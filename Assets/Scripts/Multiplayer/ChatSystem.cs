using UnityEngine;
using System.Collections.Generic;

namespace VoxelSandbox.Multiplayer
{
    public class ChatSystem
    {
        private readonly List<string> _messages = new List<string>();

        public void SendMessage(string msg)
        {
            _messages.Add(msg);
            Debug.Log($"Chat: {msg}");
        }

        public IReadOnlyList<string> Messages => _messages;
    }
}
