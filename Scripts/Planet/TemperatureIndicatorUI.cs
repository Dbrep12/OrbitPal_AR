using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// TemperatureIndicatorUI.cs
/// Thermometer animasi yang menunjukkan suhu permukaan planet.
/// Pasang ke: GameObject "TemperatureIndicator" di dalam InfoPanel
/// </summary>
public class TemperatureIndicatorUI : MonoBehaviour
{
    [Header("UI References")]
    public Image         thermometerFill;    // Image yang di-fill dari bawah ke atas (Fill Method: Vertical)
    public Image         thermometerBulb;    // Lingkaran di bawah thermometer
    public TextMeshProUGUI tempLabel;        // "-180°C ~ 430°C"
    public TextMeshProUGUI tempDescLabel;    // "Ekstrem Panas" / "Sangat Dingin" dll
    public CanvasGroup   canvasGroup;

    [Header("Animasi")]
    public float fillDuration = 1.0f;       // durasi animasi fill thermometer

    // ── Data Suhu per Body ─────────────────────────────────
    // index: 0=Sun, 1=Mercury, ..., 8=Neptune
    // Format: (suhu min °C, suhu maks °C)
    private Vector2[] tempRange = new Vector2[]
    {
        new Vector2(5500f,  5500f),   // Sun  — permukaan
        new Vector2(-180f,   430f),   // Mercury
        new Vector2( 465f,   465f),   // Venus
        new Vector2( -88f,    58f),   // Earth
        new Vector2(-125f,    20f),   // Mars
        new Vector2(-108f,  -108f),   // Jupiter — rata-rata cloud top
        new Vector2(-139f,  -139f),   // Saturn
        new Vector2(-197f,  -197f),   // Uranus
        new Vector2(-200f,  -200f),   // Neptune
    };

    private string[] tempText = new string[]
    {
        "~5.500°C",            // Sun
        "-180°C ~ 430°C",      // Mercury
        "~465°C",              // Venus
        "-88°C ~ 58°C",        // Earth
        "-125°C ~ 20°C",       // Mars
        "~-108°C",             // Jupiter
        "~-139°C",             // Saturn
        "~-197°C",             // Uranus
        "~-200°C",             // Neptune
    };

    private string[] tempDesc = new string[]
    {
        "☀️ Luar Biasa Panas",
        "🌡️ Sangat Ekstrem",
        "🔥 Terpanas di Tata Surya",
        "✅ Layak Huni",
        "❄️ Sangat Dingin",
        "🧊 Sangat Dingin",
        "🧊 Sangat Dingin",
        "🥶 Terdingin di Tata Surya",
        "🥶 Sangat Dingin",
    };

    // Fill ratio 0-1 (untuk thermometer visual)
    // 0 = paling dingin (Neptune), 1 = paling panas (Sun)
    private float[] fillRatio = new float[]
    {
        1.00f,   // Sun
        0.65f,   // Mercury (sangat ekstrem, tapi variabel)
        0.88f,   // Venus
        0.38f,   // Earth
        0.20f,   // Mars
        0.15f,   // Jupiter
        0.12f,   // Saturn
        0.05f,   // Uranus
        0.03f,   // Neptune
    };

    // Warna thermometer berdasarkan suhu
    private Color[] fillColors = new Color[]
    {
        new Color(1.0f, 0.3f, 0.0f),   // Sun — oranye merah
        new Color(1.0f, 0.5f, 0.0f),   // Mercury — oranye
        new Color(1.0f, 0.1f, 0.0f),   // Venus — merah
        new Color(0.2f, 0.7f, 1.0f),   // Earth — biru sejuk
        new Color(0.3f, 0.5f, 0.9f),   // Mars — biru dingin
        new Color(0.2f, 0.4f, 0.9f),   // Jupiter — biru
        new Color(0.1f, 0.3f, 0.8f),   // Saturn — biru tua
        new Color(0.4f, 0.8f, 1.0f),   // Uranus — cyan dingin
        new Color(0.2f, 0.3f, 0.8f),   // Neptune — biru gelap
    };

    void Awake()
    {
        HideImmediate();
    }

    public void ShowTemperature(int bodyIndex)
    {
        if (bodyIndex < 0 || bodyIndex >= tempRange.Length) return;

        gameObject.SetActive(true);
        DOTween.Kill(thermometerFill);
        DOTween.Kill(canvasGroup);

        // Update teks
        if (tempLabel    != null) tempLabel.text    = tempText[bodyIndex];
        if (tempDescLabel != null) tempDescLabel.text = tempDesc[bodyIndex];

        // Update warna bulb & fill sesuai suhu
        Color targetColor = fillColors[bodyIndex];
        if (thermometerBulb != null)
            thermometerBulb.color = targetColor;
        if (thermometerFill != null)
            thermometerFill.color = targetColor;

        // Animasi fill dari nilai saat ini ke target
        float targetFill = fillRatio[bodyIndex];
        if (thermometerFill != null)
        {
            // Reset ke 0 dulu agar animasi selalu dari bawah
            thermometerFill.fillAmount = 0f;
            DOTween.To(
                () => thermometerFill.fillAmount,
                x  => thermometerFill.fillAmount = x,
                targetFill,
                fillDuration
            ).SetEase(Ease.OutCubic);
        }

        // Fade in container
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.35f);
    }

    public void HideTemperature()
    {
        DOTween.Kill(canvasGroup);
        canvasGroup.DOFade(0f, 0.25f)
            .OnComplete(() => gameObject.SetActive(false));
    }

    void HideImmediate()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}