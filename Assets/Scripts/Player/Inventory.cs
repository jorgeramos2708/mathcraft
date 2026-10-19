using System.Collections.Generic;

namespace VoxelSandbox.Player
{
    public class Inventory
    {
        private readonly Dictionary<int, int> _items = new Dictionary<int, int>();
        public int Capacity { get; }

        public Inventory(int capacity = 36)
        {
            Capacity = capacity;
        }

        public IReadOnlyDictionary<int, int> Items => _items;

        public bool AddItem(int id, int count)
        {
            if (_items.ContainsKey(id))
                _items[id] += count;
            else
                _items[id] = count;
            return true;
        }

        public bool RemoveItem(int id, int count)
        {
            if (!_items.ContainsKey(id) || _items[id] < count) return false;
            _items[id] -= count;
            if (_items[id] <= 0) _items.Remove(id);
            return true;
        }

        public int GetCount(int id) => _items.TryGetValue(id, out var c) ? c : 0;
    }
}
