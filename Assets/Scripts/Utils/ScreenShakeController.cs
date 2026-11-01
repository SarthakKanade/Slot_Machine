using System.Collections;
using UnityEngine;

namespace SlotMachine.Utils
{
    /// <summary>
    /// Screen and cabinet impulse vibration manager.
    /// Delivers punchy physical impact on reel stops, payline triggers, and jackpot celebrations.
    /// </summary>
    public class ScreenShakeController : MonoBehaviour
    {
        public static ScreenShakeController Instance { get; private set; }

        [Header("Target Target")]
        [Tooltip("Transform to shake. If unassigned, defaults to main camera or self.")]
        [SerializeField] private Transform targetTransform;

        private Vector3 _initialLocalPos;
        private Coroutine _shakeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (targetTransform == null)
            {
                targetTransform = Camera.main != null ? Camera.main.transform : transform;
            }

            if (targetTransform != null)
            {
                _initialLocalPos = targetTransform.localPosition;
            }
        }

        public void Shake(float intensity = 8f, float duration = 0.2f)
        {
            if (targetTransform == null) return;

            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
                targetTransform.localPosition = _initialLocalPos;
            }

            _shakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration));
        }

        public void Punch(Vector2 direction, float intensity = 12f, float duration = 0.25f)
        {
            if (targetTransform == null) return;

            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
                targetTransform.localPosition = _initialLocalPos;
            }

            _shakeRoutine = StartCoroutine(PunchRoutine(direction.normalized * intensity, duration));
        }

        private IEnumerator ShakeRoutine(float intensity, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float damper = 1f - Mathf.Clamp01(elapsed / duration);
                float x = (Random.value * 2f - 1f) * intensity * damper;
                float y = (Random.value * 2f - 1f) * intensity * damper;

                targetTransform.localPosition = _initialLocalPos + new Vector3(x, y, 0f);
                yield return null;
            }

            targetTransform.localPosition = _initialLocalPos;
            _shakeRoutine = null;
        }

        private IEnumerator PunchRoutine(Vector3 offset, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Elastic bounce back
                float factor = Mathf.Sin(t * Mathf.PI);
                targetTransform.localPosition = _initialLocalPos + offset * factor * (1f - t);
                yield return null;
            }

            targetTransform.localPosition = _initialLocalPos;
            _shakeRoutine = null;
        }
    }
}

