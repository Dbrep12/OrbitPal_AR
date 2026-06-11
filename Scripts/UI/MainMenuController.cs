using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// MainMenuController.cs
/// Letakkan di: Assets/Scripts/UI/
/// Pasang ke: GameObject "MainMenuManager" di scene MainMenu
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup mainMenuCanvas;   // drag Canvas di scene ini
    public Button playButton;            // drag tombol Play
    public Button exitButton;            // drag tombol Exit

    [Header("Animasi Logo (opsional)")]
    public RectTransform logoTransform;  // drag Image logo/judul jika ada
    public float logoBounceDuration = 1.2f;

    [Header("Scene Target")]
    [Tooltip("Nama scene AR utama — harus sama persis dengan nama di Build Settings")]
    public string mainSceneName = "Main"; // <-- sesuaikan jika nama scene-mu beda

    [Header("Audio (opsional)")]
    public AudioSource menuMusic;        // drag AudioSource di scene ini
    public AudioClip   clickSound;

    private bool isLoading = false;

    // ──────────────────────────────────────────────
    void Start()
    {
        // Pastikan canvas terlihat saat scene dibuka
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.alpha = 0f;
            mainMenuCanvas.DOFade(1f, 0.8f).SetEase(Ease.OutCubic);
        }

        // Animasi logo bounce saat masuk (jika ada)
        if (logoTransform != null)
        {
            logoTransform.localScale = Vector3.zero;
            logoTransform.DOScale(Vector3.one, logoBounceDuration)
                         .SetEase(Ease.OutBack)
                         .SetDelay(0.3f);
        }

        // Pasang event tombol
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayPressed);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitPressed);

        // Mulai musik menu jika ada
        if (menuMusic != null && !menuMusic.isPlaying)
            menuMusic.Play();
    }

    // ──────────────────────────────────────────────
    public void OnPlayPressed()
    {
        if (isLoading) return;
        isLoading = true;

        PlayClickSound();

        // Animasi tombol ditekan
        if (playButton != null)
            playButton.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);

        // Fade out lalu load scene AR
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.DOFade(0f, 0.5f)
                          .SetDelay(0.15f)
                          .OnComplete(() => LoadMainScene());
        }
        else
        {
            LoadMainScene();
        }
    }

    // ──────────────────────────────────────────────
    public void OnExitPressed()
    {
        PlayClickSound();

        if (exitButton != null)
            exitButton.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f);

#if UNITY_EDITOR
        // Di dalam Editor: stop play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Di device Android: keluar aplikasi
        Application.Quit();
#endif
    }

    // ──────────────────────────────────────────────
    void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    void PlayClickSound()
    {
        if (menuMusic != null && clickSound != null)
            menuMusic.PlayOneShot(clickSound, 0.8f);
    }
}