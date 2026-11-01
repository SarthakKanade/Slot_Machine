namespace SlotMachine.Audio
{
    /// <summary>
    /// Audio event sound types triggered across the slot machine life cycle.
    /// </summary>
    public enum SoundType
    {
        ButtonClick,
        BetChange,
        SpinStart,
        ReelTick,
        ReelStop,
        WinSmall,
        WinBig,
        WinMega,
        FreeSpinsTrigger,
        CoinsCollect,
        Anticipation,
        GambleCardFlip,
        GambleWin,
        GambleLose
    }
}

