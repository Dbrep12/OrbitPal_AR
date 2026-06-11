using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class BackToMenuButton : MonoBehaviour
{
    [Header("Referensi")]
    public Button          btnBack;
    public Image           btnBackground;
    public TextMeshProUGUI labelIcon;
    public TextMeshProUGUI labelText;

    [Header("Scene")]
    public string menuSceneName = "MainMenu";

    [Header("Warna")]
    public Color normalColor  = new Color(0f, 0f, 0f, 0.63f);
    public Color pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);

    private bool isLoading = false;

    void Start()
    {
        if (btnBack != null)
        {
            // Hilangkan default color transition
            var colors          = btnBack.colors;
            colors.normalColor  = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.fadeDuration = 0f;
            btnBack.colors      = colors;

            btnBack.onClick.AddListener(OnBackPressed);
        }

        // Animasi masuk — slide dari kiri
        var rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            Vector2 origPos = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(-200f, origPos.y);
            rect.DOAnchorPos(origPos, 0.5f)
                .SetEase(Ease.OutBack)
                .SetDelay(0.3f);
        }
    }

    void OnBackPressed()
    {
        if (isLoading) return;
        isLoading = true;

        // Visual feedback — scale punch
        transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 5, 0.5f);

        // Background flash
        if (btnBackground != null)
        {
            btnBackground.DOColor(pressedColor, 0.1f)
                .OnComplete(() => btnBackground.DOColor(normalColor, 0.15f));
        }

        // Fade layar lalu load MainMenu
        StartCoroutine(LoadMenuRoutine());
    }

    System.Collections.IEnumerator LoadMenuRoutine()
    {
        // Tunggu animasi selesai
        yield return new WaitForSeconds(0.25f);

        // Fade out seluruh layar
        var fadeObj = new GameObject("FadeOverlay");
        var canvas  = fadeObj.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var img   = fadeObj.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.rectTransform.offsetMin = Vector2.zero;
        img.rectTransform.offsetMax = Vector2.zero;

        img.DOFade(1f, 0.35f).OnComplete(() =>
        {
            SceneManager.LoadScene(menuSceneName);
        });
    }
}