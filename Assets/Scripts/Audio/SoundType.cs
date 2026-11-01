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
        ReelStopReel1,
        ReelStopReel2,
        ReelStopReel3,
        WinSmall,
        WinBig,
        WinMega,
        JackpotFanfare,
        FreeSpinsTrigger,
        CoinsCollect,
        Anticipation,
        GambleCardFlip,
        GambleWin,
        GambleLose,
        ScreenShakeImpact
    }
}
