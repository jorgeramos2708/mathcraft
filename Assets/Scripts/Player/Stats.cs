namespace VoxelSandbox.Player
{
    public class Stats
    {
        public int Health { get; private set; } = 100;
        public int Hunger { get; private set; } = 100;

        public Stats() { }

        public Stats(int health, int hunger)
        {
            Health = health;
            Hunger = hunger;
        }

        public void SetHealth(int value) => Health = Mathf.Clamp(value, 0, 100);
        public void SetHunger(int value) => Hunger = Mathf.Clamp(value, 0, 100);

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
