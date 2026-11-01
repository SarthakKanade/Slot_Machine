using System.Collections.Generic;
using UnityEngine;

namespace SlotMachine.Data
{
    /// <summary>
    /// Master ScriptableObject centralizing all paytable configurations, active paylines,
    /// economy betting bounds, and bonus rules.
    /// </summary>
    [CreateAssetMenu(fileName = "PaytableConfig", menuName = "Slot Machine/Paytable Config", order = 0)]
    public class PaytableConfig : ScriptableObject
    {
        [Header("Symbols Catalogue")]
        [Tooltip("List of all active symbol ScriptableObjects available to the reels.")]
        [SerializeField] private List<SymbolData> symbols = new List<SymbolData>();

        [Header("Active Paylines")]
        [Tooltip("Configured win lines evaluated during every spin.")]
        [SerializeField] private List<PaylineData> paylines = new List<PaylineData>();

        [Header("Betting & Economy Settings")]
        [Tooltip("Initial player starting credits balance upon boot.")]
        [SerializeField] private int startingBalance = 1000;

        [Tooltip("Selectable bet tiers available to the player.")]
        [SerializeField] private int[] betAmounts = new int[] { 10, 20, 50, 100, 200, 500 };

        [Tooltip("Index into betAmounts array to select on startup.")]
        [SerializeField] private int defaultBetIndex = 1; // 20 credits

        [Header("Bonus & Free Spins")]
        [Tooltip("Number of Scatter symbols required anywhere on grid to trigger Free Spins.")]
        [SerializeField] private int scatterTriggerCount = 3;

        [Tooltip("Number of Free Spins awarded upon Scatter trigger.")]
        [SerializeField] private int freeSpinsAwarded = 10;

        [Tooltip("Global payout multiplier active during Free Spins.")]
        [SerializeField] private float freeSpinsWinMultiplier = 2.0f;

        [Header("Celebration Win Multipliers")]
        [Tooltip("Minimum multiplier (Win / Bet) to classify outcome as a Big Win.")]
        [SerializeField] private float bigWinMultiplierThreshold = 10f;

        [Tooltip("Minimum multiplier (Win / Bet) to classify outcome as a Mega Win.")]
        [SerializeField] private float megaWinMultiplierThreshold = 25f;

        [Header("Gamble Mini-Game")]
        [Tooltip("Allows player to gamble win amounts in a double-or-nothing card game.")]
        [SerializeField] private bool isGambleEnabled = true;

        [Tooltip("Maximum consecutive gamble rounds permitted per winning spin.")]
        [SerializeField] private int maxGambleRounds = 5;

        // Public Accessors
        public IReadOnlyList<SymbolData> Symbols => symbols;
        public IReadOnlyList<PaylineData> Paylines => paylines;
        public int StartingBalance => startingBalance;
        public int[] BetAmounts => betAmounts;
        public int DefaultBetIndex => Mathf.Clamp(defaultBetIndex, 0, betAmounts.Length - 1);
        public int ScatterTriggerCount => scatterTriggerCount;
        public int FreeSpinsAwarded => freeSpinsAwarded;
        public float FreeSpinsWinMultiplier => freeSpinsWinMultiplier;
        public float BigWinMultiplierThreshold => bigWinMultiplierThreshold;
        public float MegaWinMultiplierThreshold => megaWinMultiplierThreshold;
        public bool IsGambleEnabled => isGambleEnabled;
        public int MaxGambleRounds => maxGambleRounds;

        /// <summary>
        /// Populates standard classic 5-line configuration if none are defined.
        /// </summary>
        public void InitializeDefaultPaylines()
        {
            if (paylines != null && paylines.Count > 0) return;

            paylines = new List<PaylineData>
            {
                new PaylineData("Top Line", 1, new SlotCoordinate[] {
                    new SlotCoordinate(0, 0), new SlotCoordinate(1, 0), new SlotCoordinate(2, 0)
                }, new Color(0.2f, 0.8f, 1f, 1f)),

                new PaylineData("Center Line", 2, new SlotCoordinate[] {
                    new SlotCoordinate(0, 1), new SlotCoordinate(1, 1), new SlotCoordinate(2, 1)
                }, new Color(1f, 0.8f, 0.2f, 1f)),

                new PaylineData("Bottom Line", 3, new SlotCoordinate[] {
                    new SlotCoordinate(0, 2), new SlotCoordinate(1, 2), new SlotCoordinate(2, 2)
                }, new Color(0.4f, 1f, 0.4f, 1f)),

                new PaylineData("Diagonal Down", 4, new SlotCoordinate[] {
                    new SlotCoordinate(0, 0), new SlotCoordinate(1, 1), new SlotCoordinate(2, 2)
                }, new Color(1f, 0.4f, 0.8f, 1f)),

                new PaylineData("Diagonal Up", 5, new SlotCoordinate[] {
                    new SlotCoordinate(0, 2), new SlotCoordinate(1, 1), new SlotCoordinate(2, 0)
                }, new Color(1f, 0.5f, 0.2f, 1f))
            };
        }
    }
}
