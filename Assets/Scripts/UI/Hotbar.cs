using UnityEngine;

namespace VoxelSandbox.UI
{
    public class Hotbar : MonoBehaviour
    {
        public int Slots = 9;
        private int[] _items;

        void Awake()
        {
            _items = new int[Slots];
        }

        public void AddItem(int slot, int id, int count = 1)
        {
            if (slot < 0 || slot >= Slots) return;
            _items[slot] += count;
        }

        public int GetItem(int slot)
        {
            if (slot < 0 || slot >= Slots) return 0;
            return _items[slot];
        }

        public void ClearSlot(int slot)
        {
            if (slot < 0 || slot >= Slots) return;
            _items[slot] = 0;
        }
    }
}
