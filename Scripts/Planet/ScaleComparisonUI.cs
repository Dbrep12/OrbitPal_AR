using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// ScaleComparisonUI.cs
/// Tampilkan siluet Bumi di samping planet sebagai perbandingan ukuran.
/// Pasang ke: GameObject "ScaleComparison" di dalam InfoPanel
/// </summary>
public class ScaleComparisonUI : MonoBehaviour
{
    [Header("UI References")]
    public Image         earthSilhouette;    // Image siluet Bumi (sprite lingkaran biru)
    public Image         planetSilhouette;   // Image siluet planet (sprite lingkaran)
    public TextMeshProUGUI comparisonLabel;  // "Bumi vs Jupiter"
    public TextMeshProUGUI ratioLabel;       // "Jupiter 11x lebih besar dari Bumi"
    public CanvasGroup   canvasGroup;        // untuk fade in/out
    public RectTransform container;          // parent container

    [Header("Ukuran Referensi")]
    [Tooltip("Ukuran siluet Bumi dalam pixel (referensi tetap)")]
    public float earthBaseSize = 32f;
    [Tooltip("Ukuran maksimum siluet planet dalam pixel")]
    public float planetMaxSize = 96f;

    [Header("Warna Siluet Planet per Body")]
    // index 0=Sun, 1=Mercury, ..., 8=Neptune
    private Color[] planetColors = new Color[]
    {
        new Color(1.0f, 0.7f, 0.2f),   // Sun — oranye
        new Color(0.7f, 0.6f, 0.5f),   // Mercury — abu
        new Color(0.9f, 0.7f, 0.4f),   // Venus — krem
        new Color(0.2f, 0.5f, 1.0f),   // Earth — biru (referensi)
        new Color(0.8f, 0.3f, 0.2f),   // Mars — merah
        new Color(0.9f, 0.7f, 0.5f),   // Jupiter — coklat muda
        new Color(0.9f, 0.8f, 0.5f),   // Saturn — kuning
        new Color(0.5f, 0.8f, 0.9f),   // Uranus — cyan
        new Color(0.2f, 0.4f, 0.9f),   // Neptune — biru tua
    };

    // Diameter relatif terhadap Bumi (Bumi = 1.0)
    // Data nyata dibulatkan proporsional
    private float[] diameterRatio = new float[]
    {
        109.0f,  // Sun
        0.38f,   // Mercury
        0.95f,   // Venus
        1.00f,   // Earth (referensi)
        0.53f,   // Mars
        11.2f,   // Jupiter
        9.45f,   // Saturn
        4.01f,   // Uranus
        3.88f,   // Neptune
    };

    private string[] bodyNames = new string[]
    {
        "Matahari", "Merkurius", "Venus", "Bumi",
        "Mars", "Jupiter", "Saturnus", "Uranus", "Neptunus"
    };

    private string[] ratioTexts = new string[]
    {
        "Matahari 109× lebih besar dari Bumi",
        "Bumi 2,6× lebih besar dari Merkurius",
        "Venus ≈ sama besar dengan Bumi",
        "Ini adalah Bumi — referensi ukuran",
        "Bumi 1,9× lebih besar dari Mars",
        "Jupiter 11,2× lebih besar dari Bumi",
        "Saturnus 9,5× lebih besar dari Bumi",
        "Uranus 4× lebih besar dari Bumi",
        "Neptunus 3,9× lebih besar dari Bumi",
    };

    void Awake()
    {
        HideImmediate();
    }

    /// <summary>
    /// Dipanggil oleh ZoomModeController saat index berubah
    /// </summary>
    public void ShowComparison(int bodyIndex)
    {
        if (bodyIndex < 0 || bodyIndex >= diameterRatio.Length) return;

        // Jangan tampilkan untuk Bumi (index 3) — tidak perlu dibandingkan dengan dirinya
        bool isEarth = bodyIndex == 3;

        gameObject.SetActive(true);
        DOTween.Kill(canvasGroup);

        // Hitung ukuran planet silhouette
        float ratio       = diameterRatio[bodyIndex];
        float clampedSize = Mathf.Clamp(ratio * earthBaseSize, 12f, planetMaxSize);

        // Update warna & ukuran planet silhouette
        if (planetSilhouette != null)
        {
            planetSilhouette.color = planetColors[bodyIndex];
            planetSilhouette.rectTransform.sizeDelta = Vector2.one * clampedSize;
        }

        // Bumi selalu 32px
        if (earthSilhouette != null)
        {
            earthSilhouette.gameObject.SetActive(!isEarth);
            earthSilhouette.rectTransform.sizeDelta = Vector2.one * earthBaseSize;
        }

        // Label
        if (comparisonLabel != null)
            comparisonLabel.text = isEarth ? "Bumi" : $"Bumi  vs  {bodyNames[bodyIndex]}";

        if (ratioLabel != null)
            ratioLabel.text = ratioTexts[bodyIndex];

        // Fade in
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.35f);
    }

    public void HideComparison()
    {
        DOTween.Kill(canvasGroup);
        canvasGroup.DOFade(0f, 0.25f)
            .OnComplete(() => gameObject.SetActive(false));
    }

    void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha          = 0f;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }
}