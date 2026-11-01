using System;
using System.Collections;
using SlotMachine.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.UI
{
    /// <summary>
    /// Interactive classic "Double or Nothing" card gamble dialogue.
    /// Lets player guess Red vs Black to double current win up to max rounds.
    /// </summary>
    public class GambleUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private TextMeshProUGUI currentWinText;
        [SerializeField] private TextMeshProUGUI possibleWinText;
        [SerializeField] private TextMeshProUGUI cardDisplay;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button redButton;
        [SerializeField] private Button blackButton;
        [SerializeField] private Button collectButton;

        private int _currentAmount;
        private int _currentRound;
        private int _maxRounds;
        private IRandomNumberGenerator _rng;
        private Action<int> _onWinDouble;
        private Action _onLose;
        private Action _onCollect;
        private bool _isEvaluating = false;

        private void Awake()
        {
            if (redButton != null) redButton.onClick.AddListener(() => MakeChoice(CardColor.Red));
            if (blackButton != null) blackButton.onClick.AddListener(() => MakeChoice(CardColor.Black));
            if (collectButton != null) collectButton.onClick.AddListener(Collect);

            if (windowRoot != null) windowRoot.SetActive(false);
        }

        public void OpenGamble(int startAmount, IRandomNumberGenerator rng, int maxRounds,
            Action<int> onWinDouble, Action onLose, Action onCollect)
        {
            _currentAmount = startAmount;
            _rng = rng;
            _maxRounds = maxRounds;
            _currentRound = 1;
            _onWinDouble = onWinDouble;
            _onLose = onLose;
            _onCollect = onCollect;
            _isEvaluating = false;

            if (windowRoot != null) windowRoot.SetActive(true);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (currentWinText != null) currentWinText.text = $"CURRENT: {_currentAmount:N0}";
            if (possibleWinText != null) possibleWinText.text = $"WIN: {_currentAmount * 2:N0}";
            if (cardDisplay != null) cardDisplay.text = "?";
            if (statusText != null) statusText.text = $"Round {_currentRound}/{_maxRounds}: Pick RED or BLACK";

            SetButtonsInteractable(true);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (redButton != null) redButton.interactable = interactable;
            if (blackButton != null) blackButton.interactable = interactable;
            if (collectButton != null) collectButton.interactable = interactable;
        }

        private void MakeChoice(CardColor playerChoice)
        {
            if (_isEvaluating) return;
            _isEvaluating = true;
            SetButtonsInteractable(false);

            StartCoroutine(EvaluateChoiceRoutine(playerChoice));
        }

        private IEnumerator EvaluateChoiceRoutine(CardColor playerChoice)
        {
            // Simulate card shuffle
            string[] symbols = { "♠", "♥", "♦", "♣" };
            for (int i = 0; i < 6; i++)
            {
                if (cardDisplay != null) cardDisplay.text = symbols[i % 4];
                yield return new WaitForSeconds(0.06f);
            }

            // Outcome
            CardColor drawnColor = _rng.NextFloat() < 0.5f ? CardColor.Red : CardColor.Black;
            string icon = drawnColor == CardColor.Red ? "<color=#FF3344>♦</color>" : "<color=#222222>♠</color>";
            if (cardDisplay != null) cardDisplay.text = icon;

            bool isWin = drawnColor == playerChoice;

            if (isWin)
            {
                _currentAmount *= 2;
                if (statusText != null) statusText.text = $"<color=#00FF66>CORRECT! YOU DOUBLED TO {_currentAmount:N0}!</color>";
                _onWinDouble?.Invoke(_currentAmount);

                yield return new WaitForSeconds(1.0f);

                _currentRound++;
                if (_currentRound > _maxRounds)
                {
                    if (statusText != null) statusText.text = "<color=#FFD700>MAX ROUNDS REACHED! COLLECTING...</color>";
                    yield return new WaitForSeconds(1.0f);
                    Collect();
                }
                else
                {
                    _isEvaluating = false;
                    UpdateDisplay();
                }
            }
            else
            {
                if (statusText != null) statusText.text = "<color=#FF4444>WRONG PICK! GAMBLE LOST.</color>";
                _onLose?.Invoke();

                yield return new WaitForSeconds(1.2f);
                Close();
            }
        }

        private void Collect()
        {
            _onCollect?.Invoke();
            Close();
        }

        private void Close()
        {
            if (windowRoot != null) windowRoot.SetActive(false);
            _isEvaluating = false;
        }
    }
}
