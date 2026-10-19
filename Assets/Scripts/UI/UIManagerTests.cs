using NUnit.Framework;
using VoxelSandbox.Education;
using VoxelSandbox.Player;

namespace VoxelSandbox.UI
{
    public class UIManagerTests
    {
        [Test]
        public void UpdateHotbarSlot_ForwardsToHotbar()
        {
            var ui = new UIManager();
            var hotbar = new Hotbar();
            // Use reflection to set private serialized field for test
            var hotbarField = typeof(UIManager).GetField("hotbar", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            hotbarField.SetValue(ui, hotbar);

            ui.UpdateHotbarSlot(0, 42, 3);
            Assert.AreEqual(3, ui.GetHotbarSlotCount(0));
        }

        [Test]
        public void ClearHotbarSlot_ForwardsToHotbar()
        {
            var ui = new UIManager();
            var hotbar = new Hotbar();
            var hotbarField = typeof(UIManager).GetField("hotbar", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            hotbarField.SetValue(ui, hotbar);

            hotbar.AddItem(1, 5, 2);
            ui.ClearHotbarSlot(1);
            Assert.AreEqual(0, ui.GetHotbarSlotCount(1));
        }

        [Test]
        public void Health_ExposesStats()
        {
            var ui = new UIManager();
            var stats = new Stats();
            var statsField = typeof(UIManager).GetField("playerStats", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            statsField.SetValue(ui, stats);

            stats.TakeDamage(20);
            Assert.AreEqual(80, ui.Health);
        }

        [Test]
        public void ApplyDamage_UpdatesStatsAndRefreshes()
        {
            var ui = new UIManager();
            var stats = new Stats();
            var statsField = typeof(UIManager).GetField("playerStats", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            statsField.SetValue(ui, stats);

            ui.ApplyDamage(30);
            Assert.AreEqual(70, ui.Health);
        }

        [Test]
        public void ShowPuzzle_DoesNotThrowWhenNull()
        {
            var ui = new UIManager();
            Assert.DoesNotThrow(() => ui.ShowPuzzle(1));
        }

        [Test]
        public void SubmitPuzzleAnswer_ReturnsFalseWhenNull()
        {
            var ui = new UIManager();
            Assert.IsFalse(ui.SubmitPuzzleAnswer(1, 2, 3, '+', 5));
        }
    }
}
