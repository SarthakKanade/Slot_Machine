using System;
using SlotMachine.Data;
using UnityEngine;

namespace SlotMachine.Core
{
    /// <summary>
    /// Manages player credits, betting levels, bankroll limits, and free spins accounting.
    /// Pure state tracker decoupled from direct visual controls via C# events.
    /// </summary>
    public class EconomyManager
    {
        private readonly PaytableConfig _config;

        private int _currentBalance;
        private int _currentBetIndex;
        private int _freeSpinsRemaining;
        private float _freeSpinsMultiplier = 1f;

        // Public Observers
        public event Action<int> OnBalanceChanged;
        public event Action<int> OnBetChanged;
        public event Action<int, float> OnFreeSpinsChanged;

        public int CurrentBalance => _currentBalance;
        public int CurrentBet => _config.BetAmounts[_currentBetIndex];
        public int CurrentBetIndex => _currentBetIndex;
        public int FreeSpinsRemaining => _freeSpinsRemaining;
        public float ActiveMultiplier => IsInFreeSpins ? _freeSpinsMultiplier : 1f;
        public bool IsInFreeSpins => _freeSpinsRemaining > 0;

        public EconomyManager(PaytableConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _currentBalance = _config.StartingBalance;
            _currentBetIndex = _config.DefaultBetIndex;
        }

        public void Initialize()
        {
            OnBalanceChanged?.Invoke(_currentBalance);
            OnBetChanged?.Invoke(CurrentBet);
            OnFreeSpinsChanged?.Invoke(_freeSpinsRemaining, _freeSpinsMultiplier);
        }

        public bool CanAffordSpin()
        {
            if (IsInFreeSpins) return true;
            return _currentBalance >= CurrentBet;
        }

        public bool DeductBet()
        {
            if (IsInFreeSpins)
            {
                return true;
            }

            if (_currentBalance >= CurrentBet)
            {
                _currentBalance -= CurrentBet;
                OnBalanceChanged?.Invoke(_currentBalance);
                return true;
            }

            return false;
        }

        public void AwardWin(int amount)
        {
            if (amount <= 0) return;
            _currentBalance += amount;
            OnBalanceChanged?.Invoke(_currentBalance);
        }

        public void DeductAmount(int amount)
        {
            if (amount <= 0) return;
            _currentBalance = Mathf.Max(0, _currentBalance - amount);
            OnBalanceChanged?.Invoke(_currentBalance);
        }

        public void IncreaseBet()
        {
            if (IsInFreeSpins) return;
            if (_currentBetIndex < _config.BetAmounts.Length - 1)
            {
                _currentBetIndex++;
                OnBetChanged?.Invoke(CurrentBet);
            }
        }

        public void DecreaseBet()
        {
            if (IsInFreeSpins) return;
            if (_currentBetIndex > 0)
            {
                _currentBetIndex--;
                OnBetChanged?.Invoke(CurrentBet);
            }
        }

        public void SetMaxBet()
        {
            if (IsInFreeSpins) return;
            _currentBetIndex = _config.BetAmounts.Length - 1;
            OnBetChanged?.Invoke(CurrentBet);
        }

        public void StartFreeSpins(int count, float multiplier)
        {
            _freeSpinsRemaining += count;
            _freeSpinsMultiplier = multiplier;
            OnFreeSpinsChanged?.Invoke(_freeSpinsRemaining, _freeSpinsMultiplier);
        }

        public void DecrementFreeSpin()
        {
            if (_freeSpinsRemaining > 0)
            {
                _freeSpinsRemaining--;
                OnFreeSpinsChanged?.Invoke(_freeSpinsRemaining, _freeSpinsMultiplier);
            }
        }

        public void ResetBalance(int newBalance = -1)
        {
            _currentBalance = newBalance >= 0 ? newBalance : _config.StartingBalance;
            OnBalanceChanged?.Invoke(_currentBalance);
        }
    }
}

