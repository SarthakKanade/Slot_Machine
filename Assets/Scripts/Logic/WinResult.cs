using System;
using System.Collections.Generic;
using SlotMachine.Data;

namespace SlotMachine.Logic
{
    /// <summary>
    /// Represents a successful match along an individual payline.
    /// </summary>
    [Serializable]
    public class PaylineWin
    {
        public PaylineData Payline { get; }
        public SymbolData WinningSymbol { get; }
        public int MatchCount { get; }
        public float Multiplier { get; }
        public int WinAmount { get; }
        public IReadOnlyList<SlotCoordinate> WinningPositions { get; }

        public PaylineWin(
            PaylineData payline,
            SymbolData winningSymbol,
            int matchCount,
            float multiplier,
            int winAmount,
            IReadOnlyList<SlotCoordinate> winningPositions)
        {
            Payline = payline;
            WinningSymbol = winningSymbol;
            MatchCount = matchCount;
            Multiplier = multiplier;
            WinAmount = winAmount;
            WinningPositions = winningPositions;
        }
    }

    /// <summary>
    /// Encapsulates the complete evaluation of a spin outcome across all paylines and bonus triggers.
    /// </summary>
    public class WinResult
    {
        public List<PaylineWin> WinningPaylines { get; set; } = new List<PaylineWin>();
        public int TotalWinAmount { get; set; }
        public float TotalMultiplier { get; set; }
        public int ScatterCount { get; set; }
        public List<SlotCoordinate> ScatterPositions { get; set; } = new List<SlotCoordinate>();
        public bool IsFreeSpinsTriggered { get; set; }
        public int FreeSpinsAwarded { get; set; }

        public bool HasAnyWin => TotalWinAmount > 0 || IsFreeSpinsTriggered;
        public bool IsBigWin(float threshold) => TotalMultiplier >= threshold;
        public bool IsMegaWin(float threshold) => TotalMultiplier >= threshold;
    }
}

