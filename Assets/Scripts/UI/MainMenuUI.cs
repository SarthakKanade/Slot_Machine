using SlotMachine.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.UI
{
    /// <summary>
    /// Auxiliary header bar for sound muting, full screen, paytable shortcut, and info dialogs.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private Image soundIconImage;
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite soundOffSprite;
        [SerializeField] private Button paytableNavButton;
        [SerializeField] private PaytableUI paytableUI;

        private void Awake()
        {
            if (soundToggleButton != null)
            {
                soundToggleButton.onClick.AddListener(HandleSoundToggle);
            }

            if (paytableNavButton != null && paytableUI != null)
            {
                paytableNavButton.onClick.AddListener(paytableUI.Show);
            }

            UpdateSoundIcon();
        }

        private void HandleSoundToggle()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMute();
                UpdateSoundIcon();
            }
        }

        private void UpdateSoundIcon()
        {
            if (soundIconImage == null || AudioManager.Instance == null) return;
            soundIconImage.sprite = AudioManager.Instance.IsMuted ? soundOffSprite : soundOnSprite;
        }
    }
}

