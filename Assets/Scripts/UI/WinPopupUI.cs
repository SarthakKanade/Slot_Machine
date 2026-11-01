using System;
using System.Collections;
using SlotMachine.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.UI
{
    /// <summary>
    /// Celebratory victory modal featuring dynamic banner tiers (WIN, BIG WIN, MEGA WIN)
    /// and roll-up credit interpolation.
    /// </summary>
    public class WinPopupUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI subDetailText;
        [SerializeField] private Button collectButton;

        [Header("Animation")]
        [SerializeField] private float rollUpDuration = 1.0f;

        private Action _onDismissed;
        private Coroutine _rollUpRoutine;

        private void Awake()
        {
            if (collectButton != null)
            {
                collectButton.onClick.AddListener(Dismiss);
            }

            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        public void Show(WinResult result, Action onDismissed)
        {
            _onDismissed = onDismissed;

            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
            }

            string tierTitle = "NICE WIN!";
            Color titleColor = Color.white;

            if (result.TotalMultiplier >= 25f)
            {
                tierTitle = "MEGA WIN!";
                titleColor = new Color(1f, 0.2f, 0.8f);
            }
            else if (result.TotalMultiplier >= 10f)
            {
                tierTitle = "BIG WIN!";
                titleColor = new Color(1f, 0.85f, 0f);
            }

            if (titleText != null)
            {
                titleText.text = tierTitle;
                titleText.color = titleColor;
            }

            if (subDetailText != null)
            {
                subDetailText.text = $"{result.WinningPaylines.Count} Winning Line(s) Hit!";
            }

            if (_rollUpRoutine != null) StopCoroutine(_rollUpRoutine);
            _rollUpRoutine = StartCoroutine(RollUpCreditsRoutine(result.TotalWinAmount));
        }

        private IEnumerator RollUpCreditsRoutine(int targetAmount)
        {
            float elapsed = 0f;
            while (elapsed < rollUpDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / rollUpDuration);
                int current = Mathf.RoundToInt(Mathf.Lerp(0, targetAmount, progress));

                if (amountText != null)
                {
                    amountText.text = $"+{current:N0}";
                }

                yield return null;
            }

            if (amountText != null)
            {
                amountText.text = $"+{targetAmount:N0}";
            }
        }

        public void Dismiss()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }

            _onDismissed?.Invoke();
            _onDismissed = null;
        }
    }
}

