using System.Collections.Generic;
using SlotMachine.Data;
using UnityEngine;

namespace SlotMachine.Logic
{
    /// <summary>
    /// Pure C# evaluation engine for matching symbols across paylines, computing payouts,
    /// evaluating Wild substitutions, and detecting Scatter bonus triggers.
    /// Completely decoupled from UnityEngine.Object for fast, isolated unit test execution.
    /// </summary>
    public static class WinEvaluator
    {
        /// <summary>
        /// Evaluates a 2D symbol matrix against configured paylines and bonus rules.
        /// </summary>
        /// <param name="grid">2D array where dim 0 = reelIndex [0..2] and dim 1 = rowIndex [0..2].</param>
        /// <param name="config">Paytable configuration containing paylines and payout definitions.</param>
        /// <param name="currentBet">Base bet placed for the current spin.</param>
        /// <param name="activeMultiplier">Global multiplier (e.g., 2.0x during Free Spins).</param>
        /// <returns>Populated WinResult containing winning paylines, payout amount, and bonus flags.</returns>
        public static WinResult EvaluateGrid(SymbolData[,] grid, PaytableConfig config, int currentBet, float activeMultiplier = 1f)
        {
            var result = new WinResult();
            if (grid == null || config == null || currentBet <= 0) return result;

            int reelCount = grid.GetLength(0);
            int rowCount = grid.GetLength(1);

            float totalMultiplier = 0f;
            int totalWin = 0;

            // 1. Evaluate All Configured Paylines
            for (int p = 0; p < config.Paylines.Count; p++)
            {
                var payline = config.Paylines[p];
                if (payline.Coordinates == null || payline.Coordinates.Length < 2) continue;

                var lineCoords = payline.Coordinates;
                int coordsLength = lineCoords.Length;

                SymbolData[] lineSymbols = new SymbolData[coordsLength];
                for (int i = 0; i < coordsLength; i++)
                {
                    var coord = lineCoords[i];
                    if (coord.reelIndex < reelCount && coord.rowIndex < rowCount)
                    {
                        lineSymbols[i] = grid[coord.reelIndex, coord.rowIndex];
                    }
                }

                if (EvaluateLineMatch(lineSymbols, out SymbolData winningSymbol, out int matchCount))
                {
                    if (winningSymbol != null && matchCount >= 2)
                    {
                        float lineMultiplier = winningSymbol.GetPayoutMultiplier(matchCount) * activeMultiplier;
                        if (lineMultiplier > 0f)
                        {
                            int lineWinAmount = Mathf.RoundToInt(lineMultiplier * currentBet);
                            totalWin += lineWinAmount;
                            totalMultiplier += lineMultiplier;

                            var winningPositions = new List<SlotCoordinate>();
                            for (int i = 0; i < matchCount; i++)
                            {
                                winningPositions.Add(lineCoords[i]);
                            }

                            result.WinningPaylines.Add(new PaylineWin(
                                payline,
                                winningSymbol,
                                matchCount,
                                lineMultiplier,
                                lineWinAmount,
                                winningPositions
                            ));
                        }
                    }
                }
            }

            // 2. Evaluate Scatter Symbols Anywhere on the Grid
            int scatterCount = 0;
            var scatterCoords = new List<SlotCoordinate>();
            for (int r = 0; r < reelCount; r++)
            {
                for (int row = 0; row < rowCount; row++)
                {
                    var sym = grid[r, row];
                    if (sym != null && sym.Type == SymbolType.Scatter)
                    {
                        scatterCount++;
                        scatterCoords.Add(new SlotCoordinate(r, row));
                    }
                }
            }

            result.ScatterCount = scatterCount;
            result.ScatterPositions = scatterCoords;
            if (scatterCount >= config.ScatterTriggerCount)
            {
                result.IsFreeSpinsTriggered = true;
                result.FreeSpinsAwarded = config.FreeSpinsAwarded;
            }

            result.TotalWinAmount = totalWin;
            result.TotalMultiplier = totalMultiplier;

            return result;
        }

        /// <summary>
        /// Evaluates a sequence of symbols from left to right along a payline, resolving Wild substitutions.
        /// </summary>
        private static bool EvaluateLineMatch(SymbolData[] line, out SymbolData targetSymbol, out int count)
        {
            targetSymbol = null;
            count = 0;

            if (line == null || line.Length == 0 || line[0] == null) return false;

            // Find the first non-Wild standard symbol in the line
            SymbolData firstNonWild = null;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == null) break;
                if (line[i].Type == SymbolType.Standard)
                {
                    firstNonWild = line[i];
                    break;
                }
            }

            // If the line consists entirely of Wilds, target is the Wild symbol itself
            targetSymbol = firstNonWild ?? line[0];

            // Ignore Scatter on regular paylines (Scatters pay independently)
            if (targetSymbol.Type == SymbolType.Scatter) return false;

            // Count consecutive matching symbols from reel 0
            for (int i = 0; i < line.Length; i++)
            {
                var current = line[i];
                if (current == null) break;

                bool isMatch = current.SymbolId == targetSymbol.SymbolId ||
                               current.Type == SymbolType.Wild;

                if (isMatch)
                {
                    count++;
                }
                else
                {
                    break; // Non-matching symbol terminates line sequence
                }
            }

            return count >= 2;
        }
    }
}

