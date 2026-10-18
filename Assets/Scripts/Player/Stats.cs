namespace VoxelSandbox.Player
{
    public class Stats
    {
        public int Health { get; private set; } = 100;
        public int Hunger { get; private set; } = 100;

        public void TakeDamage(int amount)
        {
            Health = Mathf.Max(0, Health - amount);
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(100, Health + amount);
        }

        public void ConsumeFood(int amount)
        {
            Hunger = Mathf.Min(100, Hunger + amount);
            Heal(amount / 2);
        }

        public void TickHunger()
        {
            Hunger = Mathf.Max(0, Hunger - 1);
            if (Hunger == 0) TakeDamage(1);
        }
    }
}
