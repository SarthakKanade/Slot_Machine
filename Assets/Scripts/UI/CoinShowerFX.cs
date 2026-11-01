using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine.UI
{
    /// <summary>
    /// Canvas-based golden coin particle fountain.
    /// Sprays spinning gold coins during Big Win, Mega Win, and Jackpot celebrations.
    /// Zero GC allocations via pre-warmed pooling.
    /// </summary>
    public class CoinShowerFX : MonoBehaviour
    {
        public static CoinShowerFX Instance { get; private set; }

        [Header("Coin Setup")]
        [SerializeField] private Sprite coinSprite;
        [SerializeField] private Color coinTint = new Color(1f, 0.84f, 0.1f);
        [SerializeField] private float coinSize = 36f;
        [SerializeField] private RectTransform container;

        private readonly List<CoinParticle> _activeCoins = new List<CoinParticle>();
        private readonly Queue<CoinParticle> _coinPool = new Queue<CoinParticle>();
        private Coroutine _showerRoutine;

        private class CoinParticle
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public float RotSpeed;
            public float Gravity;
            public float Lifetime;
            public float Elapsed;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (container == null)
            {
                container = GetComponent<RectTransform>();
            }

            if (coinSprite == null)
            {
                Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                Color goldBorder = new Color(0.9f, 0.7f, 0.1f, 1f);
                Color goldCenter = new Color(1f, 0.9f, 0.3f, 1f);
                Vector2 center = new Vector2(16f, 16f);
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), center);
                        if (dist <= 14f)
                        {
                            tex.SetPixel(x, y, dist > 11f ? goldBorder : goldCenter);
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                tex.Apply();
                coinSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            }

            PrewarmPool(40);
        }

        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject($"CoinParticle_{i}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(container, false);
                go.SetActive(false);

                Image img = go.GetComponent<Image>();
                img.sprite = coinSprite;
                img.color = coinTint;
                img.raycastTarget = false;

                RectTransform rect = go.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(coinSize, coinSize);

                _coinPool.Enqueue(new CoinParticle
                {
                    Rect = rect,
                    Image = img
                });
            }
        }

        public void PlayCoinShower(float duration = 2.5f, int count = 35)
        {
            if (_showerRoutine != null) StopCoroutine(_showerRoutine);
            _showerRoutine = StartCoroutine(ShowerRoutine(duration, count));
        }

        private IEnumerator ShowerRoutine(float duration, int count)
        {
            float spawnInterval = duration * 0.4f / count;
            float spawnTimer = 0f;
            int spawned = 0;
            float totalElapsed = 0f;

            while (totalElapsed < duration || _activeCoins.Count > 0)
            {
                float dt = Time.deltaTime;
                totalElapsed += dt;

                // Spawn coins
                if (spawned < count && totalElapsed < duration * 0.5f)
                {
                    spawnTimer += dt;
                    while (spawnTimer >= spawnInterval && spawned < count)
                    {
                        spawnTimer -= spawnInterval;
                        SpawnCoin();
                        spawned++;
                    }
                }

                // Update active coins
                for (int i = _activeCoins.Count - 1; i >= 0; i--)
                {
                    var coin = _activeCoins[i];
                    coin.Elapsed += dt;

                    if (coin.Elapsed >= coin.Lifetime)
                    {
                        RecycleCoin(coin);
                        _activeCoins.RemoveAt(i);
                        continue;
                    }

                    // Physics
                    coin.Velocity.y -= coin.Gravity * dt;
                    coin.Rect.anchoredPosition += coin.Velocity * dt;
                    coin.Rect.Rotate(0f, 0f, coin.RotSpeed * dt);

                    // Fade out near end of life
                    float alpha = Mathf.Clamp01(1f - (coin.Elapsed / coin.Lifetime));
                    coin.Image.color = new Color(coinTint.r, coinTint.g, coinTint.b, alpha);
                }

                yield return null;
            }

            _showerRoutine = null;
        }

        private void SpawnCoin()
        {
            CoinParticle coin = _coinPool.Count > 0 ? _coinPool.Dequeue() : CreateNewCoin();

            coin.Rect.gameObject.SetActive(true);
            coin.Rect.anchoredPosition = new Vector2(Random.Range(-120f, 120f), -280f);
            coin.Velocity = new Vector2(Random.Range(-280f, 280f), Random.Range(650f, 950f));
            coin.Gravity = 1200f;
            coin.RotSpeed = Random.Range(-400f, 400f);
            coin.Lifetime = Random.Range(1.8f, 2.6f);
            coin.Elapsed = 0f;
            coin.Image.color = coinTint;

            _activeCoins.Add(coin);
        }

        private void RecycleCoin(CoinParticle coin)
        {
            coin.Rect.gameObject.SetActive(false);
            _coinPool.Enqueue(coin);
        }

        private CoinParticle CreateNewCoin()
        {
            GameObject go = new GameObject("CoinParticle", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(container, false);

            Image img = go.GetComponent<Image>();
            img.sprite = coinSprite;
            img.color = coinTint;
            img.raycastTarget = false;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(coinSize, coinSize);

            return new CoinParticle { Rect = rect, Image = img };
        }
    }
}
