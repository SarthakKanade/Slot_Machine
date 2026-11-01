using System;
using System.Collections.Generic;
using SlotMachine.Audio;
using SlotMachine.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.UI
{
    /// <summary>
    /// Modal dialog displaying all symbol payouts, Wild / Scatter bonus rules, and payline configurations.
    /// Compatible with both serialized inspector schemas.
    /// </summary>
    public class PaytableUI : MonoBehaviour
    {
        [Header("UI Containers")]
        [SerializeField] private GameObject paytablePanel;
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform symbolsContentParent;
        [SerializeField] private Transform symbolsContent;

        [Header("Prefabs & Templates")]
        [SerializeField] private GameObject symbolEntryTemplate;
        [SerializeField] private GameObject symbolCardPrefab;

        private GameObject ActivePanel => paytablePanel != null ? paytablePanel : (windowRoot != null ? windowRoot : gameObject);
        private Transform ActiveContainer => symbolsContentParent != null ? symbolsContentParent : symbolsContent;
        private GameObject ActiveTemplate => symbolEntryTemplate != null ? symbolEntryTemplate : symbolCardPrefab;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (symbolEntryTemplate != null)
            {
                symbolEntryTemplate.SetActive(false);
            }

            HideImmediate();
        }

        private void OnEnable()
        {
            transform.SetAsLastSibling();
            if (ActivePanel != null && !ActivePanel.activeSelf)
            {
                ActivePanel.SetActive(true);
            }
        }

        public void Initialize(PaytableConfig config)
        {
            if (config == null || ActiveContainer == null || ActiveTemplate == null) return;

            // Clear old dynamically generated cards (keep template if it lives inside container)
            for (int i = ActiveContainer.childCount - 1; i >= 0; i--)
            {
                var child = ActiveContainer.GetChild(i).gameObject;
                if (child != symbolEntryTemplate && child != symbolCardPrefab)
                {
                    Destroy(child);
                }
            }

            // Populate symbol cards
            foreach (var sym in config.Symbols)
            {
                if (sym == null) continue;

                GameObject entry = Instantiate(ActiveTemplate, ActiveContainer);
                entry.SetActive(true);

                var iconImg = entry.transform.Find("SymbolIcon")?.GetComponent<Image>();
                if (iconImg == null)
                {
                    var images = entry.GetComponentsInChildren<Image>(true);
                    foreach (var im in images)
                    {
                        if (im.gameObject.name.IndexOf("Icon", StringComparison.OrdinalIgnoreCase) >= 0 || im.gameObject != entry)
                        {
                            iconImg = im;
                            break;
                        }
                    }
                    if (iconImg == null && images.Length > 0) iconImg = images[0];
                }

                if (iconImg != null && sym.Icon != null)
                {
                    iconImg.sprite = sym.Icon;
                    iconImg.preserveAspect = true;
                }

                var nameTxt = entry.transform.Find("SymbolNameText")?.GetComponent<TextMeshProUGUI>();
                var payoutTxt = entry.transform.Find("SymbolPayoutText")?.GetComponent<TextMeshProUGUI>();

                if (nameTxt == null || payoutTxt == null)
                {
                    var texts = entry.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var t in texts)
                    {
                        if (nameTxt == null && t.gameObject.name.IndexOf("Name", StringComparison.OrdinalIgnoreCase) >= 0) nameTxt = t;
                        else if (payoutTxt == null && (t.gameObject.name.IndexOf("Payout", StringComparison.OrdinalIgnoreCase) >= 0 || t.gameObject.name.IndexOf("Info", StringComparison.OrdinalIgnoreCase) >= 0)) payoutTxt = t;
                    }
                    if (nameTxt == null && texts.Length >= 1) nameTxt = texts[0];
                    if (payoutTxt == null && texts.Length >= 2) payoutTxt = texts[1];
                }

                if (nameTxt != null) nameTxt.text = $"<b>{sym.SymbolName}</b>";

                if (payoutTxt != null)
                {
                    if (sym.Type == SymbolType.Wild)
                    {
                        payoutTxt.text = "<color=#FFE082><b>WILD JOKER</b></color>\nSubstitutes all symbols\n<color=#FFF8E1>3-Match: <b>100x</b></color>";
                    }
                    else if (sym.Type == SymbolType.Scatter)
                    {
                        payoutTxt.text = $"<color=#FF80DF><b>STAR SCATTER</b></color>\n3+ triggers {config.FreeSpinsAwarded} FS\n<color=#FFF8E1>Multiplier: <b>2x</b></color>";
                    }
                    else
                    {
                        payoutTxt.text = $"3-Match: <b>{sym.Payout3Match}x</b>\n2-Match: <b>{sym.Payout2Match}x</b>";
                    }
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (ActivePanel != null)
            {
                ActivePanel.SetActive(true);
            }
            transform.SetAsLastSibling();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
        }

        public void Hide()
        {
            if (ActivePanel != null)
            {
                ActivePanel.SetActive(false);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
        }

        public void Toggle()
        {
            if (ActivePanel != null)
            {
                if (ActivePanel.activeSelf) Hide();
                else Show();
            }
        }

        private void HideImmediate()
        {
            if (ActivePanel != null)
            {
                ActivePanel.SetActive(false);
            }
        }
    }
}
