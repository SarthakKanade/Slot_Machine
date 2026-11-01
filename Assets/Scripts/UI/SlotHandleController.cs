using System;
using System.Collections;
using SlotMachine.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SlotMachine.UI
{
    /// <summary>
    /// Interactive physical slot machine handle arm.
    /// Supports click and pointer drag gestures to pull down the lever,
    /// triggering authentic animation state changes and mechanical audio cues.
    /// </summary>
    public class SlotHandleController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Animator & Cues")]
        [SerializeField] private Animator handleAnimator;
        [SerializeField] private AudioManager audioManager;

        [Header("Tuning")]
        [SerializeField] private float pullThresholdY = -40f;
        [SerializeField] private float autoReleaseDelay = 0.25f;

        private static readonly int PullDownTrigger = Animator.StringToHash("PullDown");
        private static readonly int ReleaseUpTrigger = Animator.StringToHash("ReleaseUp");

        public event Action OnHandlePulled;

        private bool _isInteractable = true;
        private bool _isPressed = false;
        private Vector2 _pointerDownPos;
        private Coroutine _autoReleaseRoutine;

        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInteractable || _isPressed) return;
            _isPressed = true;
            _pointerDownPos = eventData.position;

            if (handleAnimator != null)
            {
                handleAnimator.SetTrigger(PullDownTrigger);
            }

            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.ButtonClick, 0.7f);
            }

            if (_autoReleaseRoutine != null) StopCoroutine(_autoReleaseRoutine);
            _autoReleaseRoutine = StartCoroutine(AutoReleaseRoutine());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            TriggerRelease();
        }

        private IEnumerator AutoReleaseRoutine()
        {
            yield return new WaitForSeconds(autoReleaseDelay);
            if (_isPressed)
            {
                TriggerRelease();
            }
        }

        private void TriggerRelease()
        {
            if (!_isPressed) return;
            _isPressed = false;

            if (handleAnimator != null)
            {
                handleAnimator.SetTrigger(ReleaseUpTrigger);
            }

            if (audioManager != null)
            {
                audioManager.PlaySound(SoundType.SpinStart, 0.8f);
            }

            OnHandlePulled?.Invoke();
        }
    }
}
