using UnityEngine;

namespace SlotMachine.Data
{
    /// <summary>
    /// ScriptableObject defining static configuration, visual identity, drop weighting,
    /// and payout rules for an individual reel symbol.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSymbolData", menuName = "Slot Machine/Symbol Data", order = 1)]
    public class SymbolData : ScriptableObject
    {
        [Header("Symbol Identity")]
        [Tooltip("Unique programmatic identifier for the symbol (e.g., 'lucky_seven').")]
        [SerializeField] private string symbolId;

        [Tooltip("Player-facing display name shown in paytables and popups.")]
        [SerializeField] private string symbolName;

        [Tooltip("Special gameplay classification (Standard, Wild, or Scatter).")]
        [SerializeField] private SymbolType symbolType = SymbolType.Standard;

        [Header("Visuals")]
        [Tooltip("High-resolution 2D sprite rendered in reel slots and UI paytables.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Accent glow color applied during win celebrations and payline highlights.")]
        [SerializeField] private Color highlightColor = Color.yellow;

        [Header("RNG & Drop Rate")]
        [Tooltip("Relative drop probability weight. Higher values appear more frequently on the reels.")]
        [Range(1, 200)]
        [SerializeField] private int dropWeight = 50;

        [Header("Payout Multipliers (x Base Bet)")]
        [Tooltip("Multiplier applied to base bet when 3 identical symbols match along an active payline.")]
        [SerializeField] private float payout3Match = 10f;

        [Tooltip("Multiplier applied to base bet when 2 identical symbols match from leftmost reel.")]
        [SerializeField] private float payout2Match = 2f;

        // Public Accessors
        public string SymbolId => symbolId;
        public string SymbolName => symbolName;
        public SymbolType Type => symbolType;
        public Sprite Icon => icon;
        public Color HighlightColor => highlightColor;
        public int DropWeight => dropWeight;
        public float Payout3Match => payout3Match;
        public float Payout2Match => payout2Match;

        /// <summary>
        /// Retrieves the payout multiplier for the given count of consecutive matches.
        /// </summary>
        /// <param name="matchCount">Number of matched symbols along a payline (2 or 3).</param>
        /// <returns>Multiplier of the base bet.</returns>
        public float GetPayoutMultiplier(int matchCount)
        {
            return matchCount switch
            {
                >= 3 => payout3Match,
                2 => payout2Match,
                _ => 0f
            };
        }
    }
}

