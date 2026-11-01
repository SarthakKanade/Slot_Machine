using System;
using System.Collections;
using SlotMachine.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.UI
{
    /// <summary>
    /// Master UI coordinator binding all HUD labels, buttons, popups, and inputs
    /// to the central SlotMachineController orchestrator.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Primary Spin Controls")]
        [SerializeField] private Button spinButton;
        [SerializeField] private Button autoSpinButton;
        [SerializeField] private Image autoSpinButtonImage;
        [SerializeField] private Sprite autoSpinReadySprite;
        [SerializeField] private Sprite autoSpinActiveSprite;
        [SerializeField] private Button turboButton;
        [SerializeField] private TextMeshProUGUI turboButtonText;
        [SerializeField] private SlotHandleController handleController;

        [Header("Bet Controls")]
        [SerializeField] private Button betIncreaseButton;
        [SerializeField] private Button betDecreaseButton;
        [SerializeField] private Button maxBetButton;

        [Header("Bonus & Paytable Triggers")]
        [SerializeField] private Button gambleButton;
        [SerializeField] private Button paytableButton;

        [Header("HUD Readout Texts")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI betText;
        [SerializeField] private TextMeshProUGUI lastWinText;
        [SerializeField] private TextMeshProUGUI statusMessageText;
        [SerializeField] private TextMeshProUGUI jackpotText;

        [Header("Free Spins Banner")]
        [SerializeField] private GameObject freeSpinsBanner;
        [SerializeField] private TextMeshProUGUI freeSpinsCountText;

        [Header("Popups")]
        [SerializeField] private PaytableUI paytablePopup;
        [SerializeField] private WinPopupUI winPopup;
        [SerializeField] private GambleUI gamblePopup;
        [SerializeField] private PaylineVisualizer paylineVisualizer;

        // User Intent Events
        public event Action OnSpinRequested;
        public event Action OnAutoSpinToggled;
        public event Action<bool> OnTurboToggled;
        public event Action OnIncreaseBetRequested;
        public event Action OnDecreaseBetRequested;
        public event Action OnMaxBetRequested;
        public event Action OnGambleRequested;

        public PaytableUI PaytablePopup => paytablePopup;
        public WinPopupUI WinPopup => winPopup;
        public GambleUI GamblePopup => gamblePopup;
        public bool IsTurboActive => _isTurboActive;

        private bool _isTurboActive = false;
        private Coroutine _breakdownRoutine;

        private void Awake()
        {
            if (spinButton != null) spinButton.onClick.AddListener(() => OnSpinRequested?.Invoke());
            if (autoSpinButton != null) autoSpinButton.onClick.AddListener(() => OnAutoSpinToggled?.Invoke());
            if (turboButton != null) turboButton.onClick.AddListener(ToggleTurbo);
            if (betIncreaseButton != null) betIncreaseButton.onClick.AddListener(() => OnIncreaseBetRequested?.Invoke());
            if (betDecreaseButton != null) betDecreaseButton.onClick.AddListener(() => OnDecreaseBetRequested?.Invoke());
            if (maxBetButton != null) maxBetButton.onClick.AddListener(() => OnMaxBetRequested?.Invoke());
            if (gambleButton != null) gambleButton.onClick.AddListener(() => OnGambleRequested?.Invoke());

            if (paytableButton != null)
            {
                paytableButton.onClick.AddListener(() =>
                {
                    if (paytablePopup == null)
                    {
                        paytablePopup = FindFirstObjectByType<PaytableUI>(FindObjectsInactive.Include);
                    }

                    if (paytablePopup != null)
                    {
                        paytablePopup.gameObject.SetActive(true);
                        paytablePopup.Show();
                    }
                    else
                    {
                        var allRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                        foreach (var root in allRoots)
                        {
                            var matches = root.GetComponentsInChildren<Transform>(true);
                            foreach (var tr in matches)
                            {
                                if (tr.gameObject.name == "Popup_Paytable")
                                {
                                    tr.gameObject.SetActive(true);
                                    tr.SetAsLastSibling();
                                    return;
                                }
                            }
                        }
                    }
                });
            }

            if (handleController != null)
            {
                handleController.OnHandlePulled += () => OnSpinRequested?.Invoke();
            }

            if (gambleButton != null)
            {
                gambleButton.gameObject.SetActive(false);
            }

            if (freeSpinsBanner != null)
            {
                freeSpinsBanner.SetActive(false);
            }

            if (paylineVisualizer == null)
            {
                paylineVisualizer = GetComponent<PaylineVisualizer>() ?? gameObject.AddComponent<PaylineVisualizer>();
            }

            if (CoinShowerFX.Instance == null)
            {
                var coinGo = new GameObject("[Auto] CoinShowerFX", typeof(RectTransform), typeof(CoinShowerFX));
                coinGo.transform.SetParent(transform, false);
            }

            UpdateTurboUI();
        }

        private void Update()
        {
            // Keyboard shortcut support: Spacebar spins
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (spinButton != null && spinButton.interactable)
                {
                    OnSpinRequested?.Invoke();
                }
            }

            // 'T' toggles Turbo Mode
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleTurbo();
            }
        }

        public void ToggleTurbo()
        {
            _isTurboActive = !_isTurboActive;
            UpdateTurboUI();
            OnTurboToggled?.Invoke(_isTurboActive);
        }

        private void UpdateTurboUI()
        {
            if (turboButtonText != null)
            {
                turboButtonText.text = _isTurboActive ? "⚡ TURBO: ON" : "TURBO: OFF";
                turboButtonText.color = _isTurboActive ? new Color(1f, 0.85f, 0.1f) : Color.white;
            }
        }

        public void UpdateJackpot(int newJackpot)
        {
            if (jackpotText != null)
            {
                jackpotText.text = $"★ JACKPOT: {newJackpot:N0} ★";
            }
        }

        public void UpdateBalance(int newBalance)
        {
            if (balanceText != null)
            {
                balanceText.text = $"{newBalance:N0}";
            }
        }

        public void UpdateBet(int newBet)
        {
            if (betText != null)
            {
                betText.text = $"{newBet:N0}";
            }
        }

        public void UpdateLastWin(int winAmount)
        {
            if (lastWinText != null)
            {
                lastWinText.text = $"{winAmount:N0}";
            }
        }

        public void UpdateFreeSpinsDisplay(int remaining, float multiplier)
        {
            if (freeSpinsBanner != null)
            {
                freeSpinsBanner.SetActive(remaining > 0);
            }

            if (freeSpinsCountText != null)
            {
                freeSpinsCountText.text = $"FREE SPINS: {remaining} ({multiplier:0.#}X)";
            }
        }

        public void SetSpinInteractable(bool interactable)
        {
            if (spinButton != null)
            {
                spinButton.interactable = interactable;
            }

            if (handleController != null)
            {
                handleController.SetInteractable(interactable);
            }
        }

        public void SetBetControlsInteractable(bool interactable)
        {
            if (betIncreaseButton != null) betIncreaseButton.interactable = interactable;
            if (betDecreaseButton != null) betDecreaseButton.interactable = interactable;
            if (maxBetButton != null) maxBetButton.interactable = interactable;
        }

        public void SetStatusMessage(string message)
        {
            if (statusMessageText != null)
            {
                statusMessageText.text = message;
            }
        }

        public void SetGambleButtonVisible(bool visible)
        {
            if (gambleButton != null)
            {
                gambleButton.gameObject.SetActive(visible);
            }
        }

        public void UpdateAutoSpinButton(bool isActive)
        {
            if (autoSpinButtonImage != null)
            {
                autoSpinButtonImage.sprite = isActive ? autoSpinActiveSprite : autoSpinReadySprite;
            }
        }

        public void DisplayWinBreakdown(WinResult result)
        {
            if (_breakdownRoutine != null)
            {
                StopCoroutine(_breakdownRoutine);
                _breakdownRoutine = null;
            }

            if (result == null || result.WinningPaylines == null || result.WinningPaylines.Count == 0) return;

            paylineVisualizer?.DisplayWinningPaylines(result.WinningPaylines);
            _breakdownRoutine = StartCoroutine(CycleWinBreakdownRoutine(result));
        }

        public void StopWinBreakdown()
        {
            if (_breakdownRoutine != null)
            {
                StopCoroutine(_breakdownRoutine);
                _breakdownRoutine = null;
            }

            paylineVisualizer?.ClearAll();
        }

        private IEnumerator CycleWinBreakdownRoutine(WinResult result)
        {
            var lines = result.WinningPaylines;
            int idx = 0;
            while (true)
            {
                var win = lines[idx % lines.Count];
                string symName = win.WinningSymbol != null ? win.WinningSymbol.SymbolName : "Match";
                SetStatusMessage($"<color=#{ColorUtility.ToHtmlStringRGB(win.Payline.LineColor)}>{win.Payline.LineName.ToUpper()}</color>: {win.MatchCount}x {symName} -> <color=#FFD700>+{win.WinAmount:N0}</color> ({win.Multiplier:0.#}x)");

                yield return new WaitForSeconds(1.6f);
                idx++;
            }
        }

        public void ShowWinCelebration(WinResult result, Action onDone)
        {
            if (winPopup != null)
            {
                winPopup.Show(result, onDone);
            }
            else
            {
                onDone?.Invoke();
            }
        }
    }
}

