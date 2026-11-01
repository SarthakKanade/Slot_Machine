namespace SlotMachine.Data
{
    /// <summary>
    /// Categorizes the role of a slot machine symbol in game logic and payline evaluation.
    /// </summary>
    public enum SymbolType
    {
        /// <summary>
        /// Regular paying symbol evaluated across paylines from left to right.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Wild symbol that substitutes for any standard symbol to form winning combinations.
        /// </summary>
        Wild = 1,

        /// <summary>
        /// Scatter symbol that pays and triggers bonus modes regardless of payline positions.
        /// </summary>
        Scatter = 2
    }
}
