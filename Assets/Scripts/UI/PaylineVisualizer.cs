using System.Collections;
using System.Collections.Generic;
using SlotMachine.Data;
using SlotMachine.Logic;
using SlotMachine.Reel;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.UI
{
    /// <summary>
    /// Renders animated neon laser vectors and side indicator pegs connecting
    /// winning symbol coordinates directly across the reels canvas.
    /// Standard commercial video-slot visual polish.
    /// </summary>
    public class PaylineVisualizer : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private SlotReelController reelController;
        [SerializeField] private RectTransform linesContainer;
        [SerializeField] private float lineThickness = 6f;

        [Header("Side Payline Peg Indicators")]
        [Tooltip("Side pill/peg lights for paylines 1 to 5 on left flank.")]
        [SerializeField] private Image[] leftLinePegs;

        [Tooltip("Side pill/peg lights for paylines 1 to 5 on right flank.")]
        [SerializeField] private Image[] rightLinePegs;

        private readonly List<Image> _activeLineSegments = new List<Image>();
        private readonly Queue<Image> _linePool = new Queue<Image>();
        private Coroutine _cycleRoutine;

        private void Awake()
        {
            if (reelController == null)
            {
                reelController = FindFirstObjectByType<SlotReelController>();
            }

            if (linesContainer == null)
            {
                linesContainer = GetComponent<RectTransform>();
            }

            ClearAll();
        }

        public void DisplayWinningPaylines(IReadOnlyList<PaylineWin> wins)
        {
            ClearAll();
            if (wins == null || wins.Count == 0 || reelController == null) return;

            if (_cycleRoutine != null) StopCoroutine(_cycleRoutine);
            _cycleRoutine = StartCoroutine(CyclePaylinesRoutine(wins));
        }

        public void ClearAll()
        {
            if (_cycleRoutine != null)
            {
                StopCoroutine(_cycleRoutine);
                _cycleRoutine = null;
            }

            for (int i = 0; i < _activeLineSegments.Count; i++)
            {
                _activeLineSegments[i].gameObject.SetActive(false);
                _linePool.Enqueue(_activeLineSegments[i]);
            }
            _activeLineSegments.Clear();

            ResetSidePegs();
        }

        private IEnumerator CyclePaylinesRoutine(IReadOnlyList<PaylineWin> wins)
        {
            // 1. Draw all lines initially for 1.2 seconds
            DrawAllLines(wins);
            yield return new WaitForSeconds(1.2f);

            // 2. If multiple wins, cycle through each individually
            if (wins.Count > 1)
            {
                int index = 0;
                while (true)
                {
                    ClearCurrentLinesOnly();
                    DrawSinglePayline(wins[index % wins.Count]);
                    yield return new WaitForSeconds(1.4f);
                    index++;
                }
            }
        }

        private void DrawAllLines(IReadOnlyList<PaylineWin> wins)
        {
            ClearCurrentLinesOnly();
            for (int i = 0; i < wins.Count; i++)
            {
                DrawSinglePayline(wins[i], false);
            }
        }

        private void ClearCurrentLinesOnly()
        {
            for (int i = 0; i < _activeLineSegments.Count; i++)
            {
                _activeLineSegments[i].gameObject.SetActive(false);
                _linePool.Enqueue(_activeLineSegments[i]);
            }
            _activeLineSegments.Clear();
            ResetSidePegs();
        }

        private void DrawSinglePayline(PaylineWin win, bool resetPegs = true)
        {
            if (win == null || win.Payline == null || win.WinningPositions == null || win.WinningPositions.Count < 2) return;

            if (resetPegs) ResetSidePegs();

            Color lineColor = win.Payline.LineColor;

            // Highlight side peg indicator
            int lineIdx = win.Payline.LineId - 1;
            if (lineIdx >= 0)
            {
                if (leftLinePegs != null && lineIdx < leftLinePegs.Length && leftLinePegs[lineIdx] != null)
                {
                    leftLinePegs[lineIdx].color = lineColor;
                    leftLinePegs[lineIdx].gameObject.SetActive(true);
                }
                if (rightLinePegs != null && lineIdx < rightLinePegs.Length && rightLinePegs[lineIdx] != null)
                {
                    rightLinePegs[lineIdx].color = lineColor;
                    rightLinePegs[lineIdx].gameObject.SetActive(true);
                }
            }

            // Draw line segments between consecutive symbol centers
            for (int i = 0; i < win.WinningPositions.Count - 1; i++)
            {
                var pA = GetSymbolCanvasPosition(win.WinningPositions[i]);
                var pB = GetSymbolCanvasPosition(win.WinningPositions[i + 1]);

                if (pA.HasValue && pB.HasValue)
                {
                    CreateLineSegment(pA.Value, pB.Value, lineColor);
                }
            }
        }

        private Vector2? GetSymbolCanvasPosition(SlotCoordinate coord)
        {
            if (reelController == null || linesContainer == null) return null;

            var reels = reelController.GetComponentsInChildren<ReelStrip>();
            if (coord.reelIndex < 0 || coord.reelIndex >= reels.Length) return null;

            var view = reels[coord.reelIndex].GetVisibleSymbolView(coord.rowIndex);
            if (view == null || view.Rect == null) return null;

            Vector3 worldCenter = view.Rect.TransformPoint(view.Rect.rect.center);
            Vector3 localInContainer = linesContainer.InverseTransformPoint(worldCenter);
            return new Vector2(localInContainer.x, localInContainer.y);
        }

        private void CreateLineSegment(Vector2 startPos, Vector2 endPos, Color color)
        {
            Image segment = GetPooledLine();
            segment.gameObject.SetActive(true);
            segment.color = color;

            RectTransform rect = segment.rectTransform;
            Vector2 dir = endPos - startPos;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            rect.anchoredPosition = startPos + (dir * 0.5f);
            rect.sizeDelta = new Vector2(distance, lineThickness);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);

            _activeLineSegments.Add(segment);
        }

        private Image GetPooledLine()
        {
            if (_linePool.Count > 0)
            {
                return _linePool.Dequeue();
            }

            GameObject go = new GameObject("LaserLineSegment", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(linesContainer, false);
            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            return img;
        }

        private void ResetSidePegs()
        {
            if (leftLinePegs != null)
            {
                for (int i = 0; i < leftLinePegs.Length; i++)
                {
                    if (leftLinePegs[i] != null) leftLinePegs[i].color = new Color(0.3f, 0.3f, 0.35f, 0.5f);
                }
            }
            if (rightLinePegs != null)
            {
                for (int i = 0; i < rightLinePegs.Length; i++)
                {
                    if (rightLinePegs[i] != null) rightLinePegs[i].color = new Color(0.3f, 0.3f, 0.35f, 0.5f);
                }
            }
        }
    }
}
