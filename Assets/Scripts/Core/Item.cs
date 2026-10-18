namespace VoxelSandbox.Core
{
    public class Item
    {
        public int Id { get; }
        public string Name { get; }
        public int StackSize { get; }

        public Item(int id, string name, int stackSize = 64)
        {
            Id = id;
            Name = name;
            StackSize = stackSize;
        }
    }
}
