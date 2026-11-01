using System;
using SlotMachine.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.UI
{
    /// <summary>
    /// Modal dialogue detailing the game's symbol hierarchy, payout multiplier schedule,
    /// and active payline geometry.
    /// </summary>
    public class PaytableUI : MonoBehaviour
    {
        [Header("Dialogue Frame")]
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private Button closeButton;

        [Header("Symbol Cards")]
        [SerializeField] private Transform symbolsContent;
        [SerializeField] private GameObject symbolCardPrefab;

        private PaytableConfig _config;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void Initialize(PaytableConfig config)
        {
            _config = config;
            PopulateCards();
        }

        public void Show()
        {
            if (windowRoot != null)
            {
                windowRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            if (windowRoot != null)
            {
                windowRoot.SetActive(false);
            }
        }

        private void PopulateCards()
        {
            if (_config == null || symbolsContent == null || symbolCardPrefab == null) return;

            // Clear any static placeholders
            for (int i = symbolsContent.childCount - 1; i >= 0; i--)
            {
                Destroy(symbolsContent.GetChild(i).gameObject);
            }

            for (int i = 0; i < _config.Symbols.Count; i++)
            {
                var sym = _config.Symbols[i];
                if (sym == null) continue;

                var card = Instantiate(symbolCardPrefab, symbolsContent);
                var iconImg = card.transform.Find("SymbolIcon")?.GetComponent<Image>();
                var nameText = card.transform.Find("SymbolNameText")?.GetComponent<TextMeshProUGUI>();
                var payoutText = card.transform.Find("SymbolPayoutText")?.GetComponent<TextMeshProUGUI>();

                if (iconImg != null && sym.Icon != null)
                {
                    iconImg.sprite = sym.Icon;
                }

                if (nameText != null)
                {
                    nameText.text = sym.SymbolName;
                }

                if (payoutText != null)
                {
                    if (sym.Type == SymbolType.Scatter)
                    {
                        payoutText.text = "3x = 10 Free Spins (2X)";
                    }
                    else if (sym.Type == SymbolType.Wild)
                    {
                        payoutText.text = $"Wild Joker (3x: {sym.Payout3Match}x)";
                    }
                    else
                    {
                        payoutText.text = $"3x: {sym.Payout3Match}x | 2x: {sym.Payout2Match}x";
                    }
                }
            }
        }
    }
}

