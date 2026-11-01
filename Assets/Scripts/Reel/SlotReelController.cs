using System;
using System.Collections;
using System.Collections.Generic;
using SlotMachine.Data;
using SlotMachine.Logic;
using UnityEngine;

namespace SlotMachine.Reel
{
    /// <summary>
    /// Master coordinator for all reels on the slot machine grid.
    /// Manages staggered launch, staggered stop sequencing, and symbol highlight effects.
    /// </summary>
    public class SlotReelController : MonoBehaviour
    {
        [Header("Reels Setup")]
        [Tooltip("Ordered array of ReelStrip components from left to right.")]
        [SerializeField] private ReelStrip[] reels;

        [Header("Timing Configuration")]
        [Tooltip("Delay in seconds between starting consecutive reels.")]
        [SerializeField] private float spinStartStagger = 0.08f;

        [Tooltip("Minimum duration in seconds before the first reel begins stopping.")]
        [SerializeField] private float minSpinDuration = 1.0f;

        [Tooltip("Delay in seconds between stopping consecutive reels.")]
        [SerializeField] private float reelStopStagger = 0.32f;

        [Tooltip("Extra spin tension duration in seconds when 2+ scatters land before the final reel stops.")]
        [SerializeField] private float anticipationSpinDuration = 1.25f;

        // Events
        public event Action OnAllReelsSpinStarted;
        public event Action OnAllReelsSpinCompleted;
        public event Action<int> OnReelClickTick;
        public event Action<int> OnAnticipationStarted;

        private bool _isSpinning;
        private int _reelsStoppedCount;

        public bool IsSpinning => _isSpinning;
        public int ReelCount => reels != null ? reels.Length : 0;

        public void Initialize(PaytableConfig config, IRandomNumberGenerator rng)
        {
            if (reels == null || reels.Length == 0)
            {
                reels = GetComponentsInChildren<ReelStrip>();
            }

            for (int i = 0; i < reels.Length; i++)
            {
                reels[i].Initialize(i, config.Symbols, rng);
                reels[i].OnSymbolTick += HandleSymbolTick;
            }
        }

        private void OnDestroy()
        {
            if (reels != null)
            {
                for (int i = 0; i < reels.Length; i++)
                {
                    if (reels[i] != null)
                    {
                        reels[i].OnSymbolTick -= HandleSymbolTick;
                    }
                }
            }
        }

        private void HandleSymbolTick(int reelIdx)
        {
            OnReelClickTick?.Invoke(reelIdx);
        }

        public void SpinReels(SymbolData[,] targetGrid, Action onComplete)
        {
            if (_isSpinning) return;
            _isSpinning = true;
            _reelsStoppedCount = 0;

            ResetAllHighlights();
            StartCoroutine(SpinSequenceRoutine(targetGrid, onComplete));
        }

        private IEnumerator SpinSequenceRoutine(SymbolData[,] targetGrid, Action onComplete)
        {
            OnAllReelsSpinStarted?.Invoke();

            // 1. Staggered Start
            for (int i = 0; i < reels.Length; i++)
            {
                reels[i].StartSpin();
                if (spinStartStagger > 0f)
                {
                    yield return new WaitForSeconds(spinStartStagger);
                }
            }

            // 2. Minimum spin spin duration
            yield return new WaitForSeconds(minSpinDuration);

            // 3. Staggered Stopping with Scatter Anticipation
            int scatterCountLanded = 0;

            for (int i = 0; i < reels.Length; i++)
            {
                int reelIdx = i;

                // Extract target symbols for this reel column
                int rowCount = targetGrid.GetLength(1);
                SymbolData[] reelTargets = new SymbolData[rowCount];
                for (int r = 0; r < rowCount; r++)
                {
                    reelTargets[r] = targetGrid[reelIdx, r];
                    if (reelTargets[r] != null && reelTargets[r].Type == SymbolType.Scatter)
                    {
                        scatterCountLanded++;
                    }
                }

                // Check if final reel should enter anticipation mode
                bool isAnticipation = (i == reels.Length - 1) && (scatterCountLanded >= 2);
                if (isAnticipation)
                {
                    OnAnticipationStarted?.Invoke(reelIdx);
                    HighlightAnticipationScatters(targetGrid, i);
                    yield return new WaitForSeconds(anticipationSpinDuration);
                }

                bool reelDone = false;
                Action<int> onReelStop = null;
                onReelStop = (stoppedIdx) =>
                {
                    if (stoppedIdx == reelIdx)
                    {
                        reels[reelIdx].OnReelSpinStopped -= onReelStop;
                        reelDone = true;
                    }
                };

                reels[reelIdx].OnReelSpinStopped += onReelStop;
                reels[reelIdx].StopSpin(reelTargets);

                while (!reelDone)
                {
                    yield return null;
                }

                if (reelStopStagger > 0f && i < reels.Length - 1)
                {
                    yield return new WaitForSeconds(reelStopStagger);
                }
            }

            _isSpinning = false;
            OnAllReelsSpinCompleted?.Invoke();
            onComplete?.Invoke();
        }

        private void HighlightAnticipationScatters(SymbolData[,] grid, int currentReelIndex)
        {
            for (int r = 0; r < currentReelIndex; r++)
            {
                for (int row = 0; row < grid.GetLength(1); row++)
                {
                    if (grid[r, row] != null && grid[r, row].Type == SymbolType.Scatter)
                    {
                        var view = reels[r].GetVisibleSymbolView(row);
                        if (view != null)
                        {
                            view.PlayAnticipationPulse();
                        }
                    }
                }
            }
        }

        public void HighlightPayline(PaylineWin win)
        {
            if (win == null || win.WinningPositions == null) return;

            Color glowColor = win.WinningSymbol != null ? win.WinningSymbol.HighlightColor : Color.yellow;
            for (int i = 0; i < win.WinningPositions.Count; i++)
            {
                var pos = win.WinningPositions[i];
                if (pos.reelIndex >= 0 && pos.reelIndex < reels.Length)
                {
                    var view = reels[pos.reelIndex].GetVisibleSymbolView(pos.rowIndex);
                    if (view != null)
                    {
                        view.PlayWinHighlight(glowColor);
                    }
                }
            }
        }

        public void HighlightScatterPositions(IReadOnlyList<SlotCoordinate> scatterPositions)
        {
            if (scatterPositions == null) return;
            for (int i = 0; i < scatterPositions.Count; i++)
            {
                var pos = scatterPositions[i];
                if (pos.reelIndex >= 0 && pos.reelIndex < reels.Length)
                {
                    var view = reels[pos.reelIndex].GetVisibleSymbolView(pos.rowIndex);
                    if (view != null)
                    {
                        view.PlayWinHighlight(new Color(1f, 0.5f, 0f, 1f));
                    }
                }
            }
        }

        public void ResetAllHighlights()
        {
            if (reels == null) return;
            for (int r = 0; r < reels.Length; r++)
            {
                if (reels[r] == null) continue;
                for (int row = 0; row < 3; row++)
                {
                    var view = reels[r].GetVisibleSymbolView(row);
                    if (view != null)
                    {
                        view.ResetHighlight();
                    }
                }
            }
        }
    }
}
