using System;
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

        [Header("Free Spins Banner")]
        [SerializeField] private GameObject freeSpinsBanner;
        [SerializeField] private TextMeshProUGUI freeSpinsCountText;

        [Header("Popups")]
        [SerializeField] private PaytableUI paytablePopup;
        [SerializeField] private WinPopupUI winPopup;
        [SerializeField] private GambleUI gamblePopup;

        // User Intent Events
        public event Action OnSpinRequested;
        public event Action OnAutoSpinToggled;
        public event Action OnIncreaseBetRequested;
        public event Action OnDecreaseBetRequested;
        public event Action OnMaxBetRequested;
        public event Action OnGambleRequested;

        public PaytableUI PaytablePopup => paytablePopup;
        public WinPopupUI WinPopup => winPopup;
        public GambleUI GamblePopup => gamblePopup;

        private void Awake()
        {
            if (spinButton != null) spinButton.onClick.AddListener(() => OnSpinRequested?.Invoke());
            if (autoSpinButton != null) autoSpinButton.onClick.AddListener(() => OnAutoSpinToggled?.Invoke());
            if (betIncreaseButton != null) betIncreaseButton.onClick.AddListener(() => OnIncreaseBetRequested?.Invoke());
            if (betDecreaseButton != null) betDecreaseButton.onClick.AddListener(() => OnDecreaseBetRequested?.Invoke());
            if (maxBetButton != null) maxBetButton.onClick.AddListener(() => OnMaxBetRequested?.Invoke());
            if (gambleButton != null) gambleButton.onClick.AddListener(() => OnGambleRequested?.Invoke());

            if (paytableButton != null && paytablePopup != null)
            {
                paytableButton.onClick.AddListener(paytablePopup.Show);
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

