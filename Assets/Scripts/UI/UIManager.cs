using UnityEngine;

namespace VoxelSandbox.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Hotbar hotbar;
        [SerializeField] private VoxelSandbox.Education.PuzzleUI puzzleUI;
        [SerializeField] private VoxelSandbox.Player.Stats playerStats;

        /// <summary>
        /// Centralizes hotbar updates. Delegates to Hotbar component.
        /// </summary>
        public void UpdateHotbarSlot(int slot, int itemId, int count)
        {
            hotbar?.AddItem(slot, itemId, count);
        }

        public void ClearHotbarSlot(int slot)
        {
            hotbar?.ClearSlot(slot);
        }

        public int GetHotbarSlotCount(int slot)
        {
            return hotbar != null ? hotbar.GetItem(slot) : 0;
        }

        /// <summary>
        /// Centralizes puzzle UI interactions.
        /// </summary>
        public void ShowPuzzle(int level)
        {
            puzzleUI?.ShowPuzzle(level);
        }

        public bool SubmitPuzzleAnswer(int level, int a, int b, char op, int answer)
        {
            if (puzzleUI == null) return false;
            puzzleUI.SubmitAnswer(level, a, b, op, answer);
            return true;
        }

        /// <summary>
        /// Centralizes stats UI refresh. Exposes read-only snapshots.
        /// </summary>
        public int Health => playerStats?.Health ?? 0;
        public int Hunger => playerStats?.Hunger ?? 0;

        public void RefreshStatsUI()
        {
            // Hook for UI binding. In Unity this would update Text/UIElements.
            // Kept empty for testability; real implementation would push values to UI components.
        }

        public void ApplyDamage(int amount)
        {
            if (playerStats != null)
            {
                playerStats.TakeDamage(amount);
                RefreshStatsUI();
            }
        }

        public void Heal(int amount)
        {
            if (playerStats != null)
            {
                playerStats.Heal(amount);
                RefreshStatsUI();
            }
        }

        public void ConsumeFood(int amount)
        {
            if (playerStats != null)
            {
                playerStats.ConsumeFood(amount);
                RefreshStatsUI();
            }
        }
    }
}
