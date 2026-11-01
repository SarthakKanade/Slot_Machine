using System;
using System.Collections;
using SlotMachine.Audio;
using SlotMachine.Data;
using SlotMachine.Logic;
using SlotMachine.Reel;
using SlotMachine.UI;
using UnityEngine;

namespace SlotMachine.Core
{
    /// <summary>
    /// Master orchestrator for the slot machine game. Coordinates RNG outcomes,
    /// reel physics animation, win evaluation, economy credits, UI HUD, and bonus modes.
    /// </summary>
    public class SlotMachineController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PaytableConfig paytableConfig;

        [Header("Scene References")]
        [SerializeField] private SlotReelController reelController;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AudioManager audioManager;

        [Header("Automation & Pacing")]
        [SerializeField] private float autoSpinDelay = 1.0f;

        private RNGManager _rng;
        private EconomyManager _economy;
        private GameState _currentState = GameState.Idle;
        private bool _isAutoSpinActive = false;
        private Coroutine _autoSpinRoutine;
        private int _pendingGambleWinAmount = 0;
        private Coroutine _postWinRoutine;
        [NonSerialized] private bool _isInitialized = false;

        public GameState CurrentState => _currentState;
        public EconomyManager Economy => _economy;
        public PaytableConfig Config => paytableConfig;
        public bool CanSpin => (_currentState == GameState.Idle || _currentState == GameState.FreeSpins) && _economy != null && _economy.CanAffordSpin();

        // Spin Lifecycle Events
        public event Action OnSpinInitiated;
        public event Action<WinResult> OnSpinResultsReady;
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            Application.runInBackground = true;
            InitializeGame();
        }

        private void Start()
        {
            if (!_isInitialized || _economy == null)
            {
                InitializeGame();
            }
        }

        public void InitializeGame()
        {
            if (_isInitialized && _economy != null && _rng != null) return;

            if (paytableConfig == null)
            {
                Debug.LogError("[SlotMachineController] PaytableConfig is missing!");
                return;
            }

            paytableConfig.InitializeDefaultPaylines();

            if (Utils.ScreenShakeController.Instance == null)
            {
                new GameObject("[Auto] ScreenShakeController", typeof(Utils.ScreenShakeController));
            }

            _rng = new RNGManager();
            _economy = new EconomyManager(paytableConfig);
            _isInitialized = true;

            if (uiManager != null)
            {
                uiManager.OnSpinRequested += HandleSpinRequested;
                uiManager.OnAutoSpinToggled += HandleAutoSpinToggled;
                uiManager.OnTurboToggled += (isTurbo) =>
                {
                    if (reelController != null) reelController.IsTurboMode = isTurbo;
                };
                uiManager.OnIncreaseBetRequested += () => _economy.IncreaseBet();
                uiManager.OnDecreaseBetRequested += () => _economy.DecreaseBet();
                uiManager.OnMaxBetRequested += () => _economy.SetMaxBet();
                uiManager.OnGambleRequested += StartGamble;

                _economy.OnBalanceChanged += uiManager.UpdateBalance;
                _economy.OnBetChanged += uiManager.UpdateBet;
                _economy.OnFreeSpinsChanged += uiManager.UpdateFreeSpinsDisplay;
                _economy.OnJackpotChanged += uiManager.UpdateJackpot;

                if (uiManager.PaytablePopup != null)
                {
                    uiManager.PaytablePopup.Initialize(paytableConfig);
                }
            }

            if (reelController != null)
            {
                reelController.Initialize(paytableConfig, _rng);
                reelController.OnReelClickTick += HandleReelClickTick;
                reelController.OnAnticipationStarted += HandleAnticipationStarted;
                reelController.OnIndividualReelStopped += HandleIndividualReelStopped;
            }

            _economy.Initialize();
            SetState(GameState.Idle);
        }

        private void OnDestroy()
        {
            if (reelController != null)
            {
                reelController.OnReelClickTick -= HandleReelClickTick;
                reelController.OnAnticipationStarted -= HandleAnticipationStarted;
                reelController.OnIndividualReelStopped -= HandleIndividualReelStopped;
            }
        }

        private void HandleIndividualReelStopped(int reelIdx)
        {
            if (audioManager == null) return;
            switch (reelIdx)
            {
                case 0:
                    audioManager.PlaySound(SoundType.ReelStopReel1, 0.75f);
                    break;
                case 1:
                    audioManager.PlaySound(SoundType.ReelStopReel2, 0.85f);
                    break;
                case 2:
                    audioManager.PlaySound(SoundType.ReelStopReel3, 1.0f);
                    break;
                default:
                    audioManager.PlaySound(SoundType.ReelStop, 0.8f);
                    break;
            }
        }

        private void HandleReelClickTick(int reelIdx)
        {
            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.ReelTick, 0.25f);
            }
        }

        private void HandleAnticipationStarted(int reelIdx)
        {
            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.Anticipation, 0.9f);
            }

            if (uiManager != null)
            {
                uiManager.SetStatusMessage("<color=#FFD700>BONUS CHANCE! SCATTER ANTICIPATION...</color>");
            }
        }

        private void SetState(GameState newState)
        {
            _currentState = newState;
            UpdateControlsForState();
            OnStateChanged?.Invoke(newState);
        }

        private void UpdateControlsForState()
        {
            if (uiManager == null) return;

            switch (_currentState)
            {
                case GameState.Idle:
                    uiManager.SetSpinInteractable(_economy.CanAffordSpin());
                    uiManager.SetBetControlsInteractable(!_economy.IsInFreeSpins);
                    uiManager.SetStatusMessage("Click HANDLE or SPIN to play!");
                    break;

                case GameState.Spinning:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetStatusMessage("Spinning...");
                    break;

                case GameState.Evaluating:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetStatusMessage("Evaluating...");
                    break;

                case GameState.WinCelebration:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetGambleButtonVisible(paytableConfig.IsGambleEnabled && !_economy.IsInFreeSpins && !_isAutoSpinActive && _pendingGambleWinAmount > 0);
                    uiManager.SetStatusMessage("WINNER! Press GAMBLE (2X) or SPIN/COLLECT!");
                    break;

                case GameState.Gamble:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetGambleButtonVisible(false);
                    uiManager.SetStatusMessage("Gamble Active: Double or Nothing!");
                    break;

                case GameState.FreeSpins:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(false);
                    uiManager.SetGambleButtonVisible(false);
                    uiManager.SetStatusMessage("FREE SPINS BONUS ACTIVE (2X)!");
                    break;

                case GameState.OutOfFunds:
                    uiManager.SetSpinInteractable(false);
                    uiManager.SetBetControlsInteractable(true);
                    uiManager.SetGambleButtonVisible(false);
                    uiManager.SetStatusMessage("Out of credits! Adjust bet or reset balance.");
                    StopAutoSpin();
                    break;
            }
        }

        public void HandleSpinRequested()
        {
            if (!CanSpin)
            {
                if (!_economy.CanAffordSpin() && !_economy.IsInFreeSpins)
                {
                    SetState(GameState.OutOfFunds);
                }
                return;
            }

            if (_postWinRoutine != null)
            {
                StopCoroutine(_postWinRoutine);
                _postWinRoutine = null;
            }

            CollectPendingGamble();
            ExecuteSpin();
        }

        private void ExecuteSpin()
        {
            if (!_economy.DeductBet())
            {
                SetState(GameState.OutOfFunds);
                return;
            }

            SetState(_economy.IsInFreeSpins ? GameState.FreeSpins : GameState.Spinning);

            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.SpinStart, 0.7f);
            }

            if (uiManager != null)
            {
                uiManager.SetGambleButtonVisible(false);
                uiManager.StopWinBreakdown();
            }

            OnSpinInitiated?.Invoke();

            // 1. Generate RNG outcomes
            int reelCount = reelController != null ? reelController.ReelCount : 3;
            int rowCount = 3;
            SymbolData[,] outcomeGrid = _rng.GenerateGrid(reelCount, rowCount, paytableConfig.Symbols);

            // 2. Evaluate outcomes
            float multiplier = _economy.ActiveMultiplier;
            WinResult winResult = WinEvaluator.EvaluateGrid(outcomeGrid, paytableConfig, _economy.CurrentBet, multiplier);

            // 3. Command Reel presentation to spin and land on grid
            if (reelController != null)
            {
                reelController.SpinReels(outcomeGrid, () =>
                {
                    StartCoroutine(ProcessSpinResolutionRoutine(winResult));
                });
            }
        }

        private IEnumerator ProcessSpinResolutionRoutine(WinResult result)
        {
            SetState(GameState.Evaluating);

            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.ReelStop, 0.8f);
            }

            yield return new WaitForSeconds(0.2f);

            OnSpinResultsReady?.Invoke(result);

            if (_economy.IsInFreeSpins)
            {
                _economy.DecrementFreeSpin();
            }

            if (result.IsJackpotWon)
            {
                int jackpotWon = _economy.AwardJackpot();
                if (audioManager != null)
                {
                    audioManager.PlaySound(SoundType.JackpotFanfare, 1.0f);
                }
                Utils.ScreenShakeController.Instance?.Shake(16f, 0.65f);
                if (uiManager != null)
                {
                    uiManager.SetStatusMessage($"<color=#FFD700>★★★ PROGRESSIVE JACKPOT! +{jackpotWon:N0} CREDITS! ★★★</color>");
                }
                yield return new WaitForSeconds(1.5f);
            }

            if (result.IsFreeSpinsTriggered)
            {
                if (audioManager != null)
                {
                    audioManager.PlaySound(SoundType.FreeSpinsTrigger, 1.0f);
                }

                if (reelController != null)
                {
                    reelController.HighlightScatterPositions(result.ScatterPositions);
                }

                _economy.StartFreeSpins(result.FreeSpinsAwarded, paytableConfig.FreeSpinsWinMultiplier);

                if (uiManager != null)
                {
                    uiManager.SetStatusMessage($"<color=#FFD700>SCATTERS HIT! +{result.FreeSpinsAwarded} FREE SPINS (2X)!</color>");
                }

                yield return new WaitForSeconds(1.5f);
            }

            if (result.HasAnyWin && result.TotalWinAmount > 0)
            {
                _pendingGambleWinAmount = result.TotalWinAmount;
                _economy.AwardWin(result.TotalWinAmount);

                if (uiManager != null)
                {
                    uiManager.UpdateLastWin(result.TotalWinAmount);
                    uiManager.DisplayWinBreakdown(result);
                }

                PlayWinCelebrationSound(result);

                if (reelController != null && result.WinningPaylines.Count > 0)
                {
                    for (int i = 0; i < result.WinningPaylines.Count; i++)
                    {
                        reelController.HighlightPayline(result.WinningPaylines[i]);
                    }
                }

                if (uiManager != null)
                {
                    SetState(GameState.WinCelebration);
                    uiManager.ShowWinCelebration(result, () =>
                    {
                        OnWinCelebrationDismissed();
                    });
                }
                else
                {
                    OnWinCelebrationDismissed();
                }
            }
            else
            {
                _pendingGambleWinAmount = 0;
                FinishSpinCycle();
            }
        }

        private void OnWinCelebrationDismissed()
        {
            if (_isAutoSpinActive)
            {
                CollectPendingGamble();
                FinishSpinCycle();
                return;
            }

            if (paytableConfig.IsGambleEnabled && _pendingGambleWinAmount > 0 && !_economy.IsInFreeSpins)
            {
                SetState(GameState.WinCelebration);
                _postWinRoutine = StartCoroutine(PostWinAutoCollectTimer());
            }
            else
            {
                FinishSpinCycle();
            }
        }

        private IEnumerator PostWinAutoCollectTimer()
        {
            yield return new WaitForSeconds(4f);
            CollectPendingGamble();
            FinishSpinCycle();
        }

        private void PlayWinCelebrationSound(WinResult result)
        {
            if (audioManager == null) return;

            if (result.IsMegaWin(paytableConfig.MegaWinMultiplierThreshold))
            {
                audioManager.PlaySound(SoundType.WinMega, 1.0f);
                Utils.ScreenShakeController.Instance?.Shake(13f, 0.45f);
            }
            else if (result.IsBigWin(paytableConfig.BigWinMultiplierThreshold))
            {
                audioManager.PlaySound(SoundType.WinBig, 0.9f);
                Utils.ScreenShakeController.Instance?.Shake(8f, 0.3f);
            }
            else
            {
                audioManager.PlaySound(SoundType.WinSmall, 0.7f);
            }
        }

        private void FinishSpinCycle()
        {
            if (_economy.IsInFreeSpins)
            {
                SetState(GameState.FreeSpins);
                StartCoroutine(AutoRunFreeSpinsRoutine());
            }
            else if (_isAutoSpinActive)
            {
                SetState(GameState.Idle);
                _autoSpinRoutine = StartCoroutine(AutoSpinCountdownRoutine());
            }
            else
            {
                SetState(_economy.CanAffordSpin() ? GameState.Idle : GameState.OutOfFunds);
            }
        }

        private IEnumerator AutoRunFreeSpinsRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            if (_economy.IsInFreeSpins)
            {
                ExecuteSpin();
            }
            else
            {
                FinishSpinCycle();
            }
        }

        private void HandleAutoSpinToggled()
        {
            _isAutoSpinActive = !_isAutoSpinActive;

            if (uiManager != null)
            {
                uiManager.UpdateAutoSpinButton(_isAutoSpinActive);
            }

            if (_isAutoSpinActive && _currentState == GameState.Idle)
            {
                HandleSpinRequested();
            }
            else if (!_isAutoSpinActive && _autoSpinRoutine != null)
            {
                StopCoroutine(_autoSpinRoutine);
                _autoSpinRoutine = null;
            }
        }

        private void StopAutoSpin()
        {
            _isAutoSpinActive = false;
            if (_autoSpinRoutine != null)
            {
                StopCoroutine(_autoSpinRoutine);
                _autoSpinRoutine = null;
            }
            if (uiManager != null)
            {
                uiManager.UpdateAutoSpinButton(false);
            }
        }

        private IEnumerator AutoSpinCountdownRoutine()
        {
            yield return new WaitForSeconds(autoSpinDelay);
            if (_isAutoSpinActive && CanSpin)
            {
                HandleSpinRequested();
            }
            else
            {
                StopAutoSpin();
            }
        }

        public void StartGamble()
        {
            if (!paytableConfig.IsGambleEnabled || _pendingGambleWinAmount <= 0) return;

            if (_postWinRoutine != null)
            {
                StopCoroutine(_postWinRoutine);
                _postWinRoutine = null;
            }

            SetState(GameState.Gamble);
            if (uiManager != null && uiManager.GamblePopup != null)
            {
                uiManager.GamblePopup.OpenGamble(_pendingGambleWinAmount, _rng, paytableConfig.MaxGambleRounds,
                    onWinDouble: (doubled) =>
                    {
                        int added = doubled - _pendingGambleWinAmount;
                        _pendingGambleWinAmount = doubled;
                        _economy.AwardWin(added);
                        if (audioManager != null) audioManager.PlaySound(SoundType.GambleWin, 0.9f);
                    },
                    onLose: () =>
                    {
                        _economy.DeductAmount(_pendingGambleWinAmount);
                        _pendingGambleWinAmount = 0;
                        if (audioManager != null) audioManager.PlaySound(SoundType.GambleLose, 0.9f);
                    },
                    onCollect: () =>
                    {
                        CollectPendingGamble();
                        FinishSpinCycle();
                    }
                );
            }
        }

        public void CollectPendingGamble()
        {
            if (_pendingGambleWinAmount > 0)
            {
                if (audioManager != null) audioManager.PlaySound(SoundType.CoinsCollect, 0.6f);
                _pendingGambleWinAmount = 0;
            }
            if (uiManager != null)
            {
                uiManager.SetGambleButtonVisible(false);
                uiManager.StopWinBreakdown();
            }
        }
    }
}

