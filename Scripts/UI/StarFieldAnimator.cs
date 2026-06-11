using UnityEngine;
using UnityEngine.UI;

public class StarFieldAnimator : MonoBehaviour
{
    [Header("Settings")]
    public int   starCount     = 80;
    public float minSize       = 2f;
    public float maxSize       = 5f;
    public float minSpeed      = 0.02f;
    public float maxSpeed      = 0.08f;
    public float minAlpha      = 0.3f;
    public float maxAlpha      = 1f;

    private RectTransform[] stars;
    private float[]         speeds;
    private float[]         alphas;
    private float[]         twinkleSpeed;
    private float           screenW;
    private float           screenH;

    void Start()
    {
        screenW = Screen.width;
        screenH = Screen.height;

        stars        = new RectTransform[starCount];
        speeds       = new float[starCount];
        alphas       = new float[starCount];
        twinkleSpeed = new float[starCount];

        for (int i = 0; i < starCount; i++)
        {
            // Buat GameObject bintang
            var go  = new GameObject("Star_" + i);
            go.transform.SetParent(transform, false);

            var img        = go.AddComponent<Image>();
            img.color      = Color.white;
            img.raycastTarget = false;

            var rect       = go.GetComponent<RectTransform>();
            float size     = Random.Range(minSize, maxSize);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.anchoredPosition = new Vector2(
                Random.Range(0f, screenW),
                Random.Range(0f, screenH));

            speeds[i]       = Random.Range(minSpeed, maxSpeed);
            alphas[i]       = Random.Range(minAlpha, maxAlpha);
            twinkleSpeed[i] = Random.Range(0.5f, 2f);
            stars[i]        = rect;

            // Set alpha awal
            img.color = new Color(1f, 1f, 1f, alphas[i]);
        }
    }

    void Update()
    {
        for (int i = 0; i < starCount; i++)
        {
            if (stars[i] == null) continue;

            // Gerak ke bawah perlahan
            var pos    = stars[i].anchoredPosition;
            pos.y     -= speeds[i] * screenH * Time.deltaTime;

            // Reset ke atas kalau sudah keluar bawah
            if (pos.y < -10f)
            {
                pos.y = screenH + 10f;
                pos.x = Random.Range(0f, screenW);
            }

            stars[i].anchoredPosition = pos;

            // Twinkle effect
            var img   = stars[i].GetComponent<Image>();
            if (img != null)
            {
                float alpha = alphas[i] * (0.6f + 
                    0.4f * Mathf.Sin(Time.time * twinkleSpeed[i]));
                img.color   = new Color(1f, 1f, 1f, alpha);
            }
        }
    }
}