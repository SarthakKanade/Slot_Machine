using System.Collections.Generic;
using SlotMachine.Data;

namespace SlotMachine.Logic
{
    /// <summary>
    /// Contract for random number generation and outcome determination in the slot engine.
    /// Allows mocking and deterministic seed injection during unit testing.
    /// </summary>
    public interface IRandomNumberGenerator
    {
        /// <summary>
        /// Generates a pseudo-random integer within [minInclusive, maxExclusive).
        /// </summary>
        int NextRange(int minInclusive, int maxExclusive);

        /// <summary>
        /// Generates a normalized floating point number in [0.0, 1.0).
        /// </summary>
        float NextFloat();

        /// <summary>
        /// Selects a symbol from the available pool based on weighted probability.
        /// </summary>
        SymbolData GetWeightedRandomSymbol(IReadOnlyList<SymbolData> availableSymbols);

        /// <summary>
        /// Seeds the internal generator for auditability and deterministic testing.
        /// </summary>
        void SetSeed(int seed);
    }
}
