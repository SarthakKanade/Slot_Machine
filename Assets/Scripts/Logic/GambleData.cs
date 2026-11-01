using System;

namespace SlotMachine.Logic
{
    /// <summary>
    /// Available suit colors in the double-or-nothing card gamble mini-game.
    /// </summary>
    public enum CardColor
    {
        Red,
        Black
    }

    /// <summary>
    /// Standard playing card suits for graphical presentation.
    /// </summary>
    public enum CardSuit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    /// <summary>
    /// Data payload describing an individual gamble round outcome.
    /// </summary>
    [Serializable]
    public struct GambleOutcome
    {
        public bool IsWin;
        public CardColor DrawnColor;
        public CardSuit DrawnSuit;
        public int ResultingAmount;
        public int RoundNumber;

        public GambleOutcome(bool isWin, CardColor color, CardSuit suit, int amount, int round)
        {
            IsWin = isWin;
            DrawnColor = color;
            DrawnSuit = suit;
            ResultingAmount = amount;
            RoundNumber = round;
        }
    }
}
