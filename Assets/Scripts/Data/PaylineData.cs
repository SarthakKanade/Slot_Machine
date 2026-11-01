using System;
using UnityEngine;

namespace SlotMachine.Data
{
    /// <summary>
    /// Represents a discrete (reelIndex, rowIndex) coordinate on the slot machine display grid.
    /// Dimension 0 = Reel Index (0 to 2, left to right).
    /// Dimension 1 = Row Index (0 to 2, top to bottom).
    /// </summary>
    [Serializable]
    public struct SlotCoordinate : IEquatable<SlotCoordinate>
    {
        public int reelIndex;
        public int rowIndex;

        public SlotCoordinate(int reel, int row)
        {
            reelIndex = reel;
            rowIndex = row;
        }

        public bool Equals(SlotCoordinate other) => reelIndex == other.reelIndex && rowIndex == other.rowIndex;
        public override bool Equals(object obj) => obj is SlotCoordinate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(reelIndex, rowIndex);
        public override string ToString() => $"({reelIndex}, {rowIndex})";
    }

    /// <summary>
    /// Defines a payline trajectory across the reels and its visual configuration.
    /// </summary>
    [Serializable]
    public class PaylineData
    {
        [Tooltip("Human-readable label (e.g., 'Center Line', 'Diagonal Down').")]
        [SerializeField] private string lineName;

        [Tooltip("Numeric index or identifier for sorting and logging.")]
        [SerializeField] private int lineId;

        [Tooltip("Ordered series of grid coordinates from leftmost reel to rightmost reel.")]
        [SerializeField] private SlotCoordinate[] coordinates;

        [Tooltip("Distinct color used to trace the line path during payline celebrations.")]
        [SerializeField] private Color lineColor = Color.yellow;

        public string LineName => lineName;
        public int LineId => lineId;
        public SlotCoordinate[] Coordinates => coordinates;
        public Color LineColor => lineColor;

        public PaylineData(string name, int id, SlotCoordinate[] coords, Color color)
        {
            lineName = name;
            lineId = id;
            coordinates = coords;
            lineColor = color;
        }
    }
}

