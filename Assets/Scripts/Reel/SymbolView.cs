using System.Collections;
using SlotMachine.Data;
using SlotMachine.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.Reel
{
    /// <summary>
    /// Visual component representing an individual symbol slot on a reel.
    /// Handles sprite rendering, win celebration pulsing, and visual effects.
    /// </summary>
    public class SymbolView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image symbolImage;
        [SerializeField] private Image glowOutline;
        [SerializeField] private RectTransform rectTransform;

        [Header("Animation Settings")]
        [SerializeField] private float pulseScale = 1.18f;
        [SerializeField] private float pulseSpeed = 4f;

        private SymbolData _currentSymbol;
        private Coroutine _winAnimationRoutine;
        private Vector3 _originalScale;
        private Color _originalImageColor = Color.white;
        private Color _baseGlowColor = Color.yellow;
        private bool _isHighlighted;

        public SymbolData CurrentSymbol => _currentSymbol;
        public RectTransform Rect => rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());
        public bool IsHighlighted => _isHighlighted;

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            _originalScale = rectTransform != null ? rectTransform.localScale : Vector3.one;

            if (symbolImage != null)
            {
                _originalImageColor = symbolImage.color;
            }

            if (glowOutline != null)
            {
                glowOutline.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Binds and displays the given SymbolData.
        /// </summary>
        public void SetSymbol(SymbolData symbol)
        {
            _currentSymbol = symbol;
            if (symbolImage != null)
            {
                if (symbol != null && symbol.Icon != null)
                {
                    symbolImage.sprite = symbol.Icon;
                    symbolImage.enabled = true;
                }
                else
                {
                    symbolImage.enabled = false;
                }
            }

            ResetHighlight();
        }

        /// <summary>
        /// Triggers win highlight effect (glow outline + scale-pulse & color-flash tween).
        /// </summary>
        public void PlayWinHighlight(Color customGlowColor)
        {
            if (_isHighlighted) return;
            _isHighlighted = true;
            _baseGlowColor = customGlowColor;

            if (glowOutline != null)
            {
                glowOutline.color = _baseGlowColor;
                glowOutline.gameObject.SetActive(true);
            }

            if (_winAnimationRoutine != null) StopCoroutine(_winAnimationRoutine);
            _winAnimationRoutine = StartCoroutine(AnimateWinPulseRoutine(pulseScale, pulseSpeed));
        }

        /// <summary>
        /// Triggers rapid anticipation pulse for landed scatters when bonus is within reach.
        /// </summary>
        public void PlayAnticipationPulse(Color? customColor = null)
        {
            if (_isHighlighted) return;
            _isHighlighted = true;
            _baseGlowColor = customColor ?? new Color(1f, 0.85f, 0.1f, 1f);

            if (glowOutline != null)
            {
                glowOutline.color = _baseGlowColor;
                glowOutline.gameObject.SetActive(true);
            }

            if (_winAnimationRoutine != null) StopCoroutine(_winAnimationRoutine);
            _winAnimationRoutine = StartCoroutine(AnimateWinPulseRoutine(1.12f, 6.5f));
        }

        /// <summary>
        /// Restores symbol to baseline neutral state.
        /// </summary>
        public void ResetHighlight()
        {
            _isHighlighted = false;
            if (_winAnimationRoutine != null)
            {
                StopCoroutine(_winAnimationRoutine);
                _winAnimationRoutine = null;
            }

            if (glowOutline != null)
            {
                glowOutline.gameObject.SetActive(false);
            }

            if (rectTransform != null)
            {
                rectTransform.localScale = _originalScale;
            }

            if (symbolImage != null)
            {
                symbolImage.color = _originalImageColor;
            }
        }

        private IEnumerator AnimateWinPulseRoutine(float targetScale, float speed)
        {
            float elapsed = 0f;
            while (true)
            {
                elapsed += Time.deltaTime * speed;
                float wave = (Mathf.Sin(elapsed) + 1f) * 0.5f;

                if (rectTransform != null)
                {
                    float currentScale = Mathf.Lerp(1f, targetScale, wave);
                    rectTransform.localScale = _originalScale * currentScale;
                }

                if (glowOutline != null)
                {
                    float alpha = Mathf.Lerp(0.4f, 1f, wave);
                    glowOutline.color = new Color(_baseGlowColor.r, _baseGlowColor.g, _baseGlowColor.b, alpha);
                }

                yield return null;
            }
        }
    }
}

