namespace SlotMachine.Core
{
    /// <summary>
    /// Explicit high-level lifecycle states governing the slot machine gameplay and input routing.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Machine is idle waiting for player input (Spin, Bet adjust, Paytable, or Handle pull).
        /// </summary>
        Idle,

        /// <summary>
        /// Reels are actively spinning in continuous scroll loop.
        /// </summary>
        Spinning,

        /// <summary>
        /// Reels have landed; outcomes are being evaluated against active paylines.
        /// </summary>
        Evaluating,

        /// <summary>
        /// Payline celebration active: highlighting winning symbols, tallying payout credits.
        /// </summary>
        WinCelebration,

        /// <summary>
        /// Free Spins bonus sequence actively in progress with 2X win multiplier.
        /// </summary>
        FreeSpins,

        /// <summary>
        /// Double-or-Nothing card flip gamble mini-game is open.
        /// </summary>
        Gamble,

        /// <summary>
        /// Player credits insufficient for current bet; prompt bet reduction or credit reset.
        /// </summary>
        OutOfFunds
    }
}
