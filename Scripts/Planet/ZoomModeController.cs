using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ZoomModeController : MonoBehaviour
{
    [Header("Referensi")]
    public SolarSystemManager solarSystemManager;
    public PinchController pinchController;
    public Camera             arCamera;

    [Header("Zoom Settings")]
    public float zoomDistance = 0.5f;
    public float zoomDuration = 0.8f;
    public float zoomHeight   = 0.1f;

    [Header("UI")]
    public GameObject      zoomModeUI;
    public Button          btnNext;
    public Button          btnPrev;
    public Button          btnExit;
    public CanvasGroup     infoPanel;
    public RectTransform   infoPanelRect;
    public TextMeshProUGUI labelBodyName;
    public TextMeshProUGUI labelDistance;
    public TextMeshProUGUI labelFact;
    public TextMeshProUGUI labelNextPreview;
    public TextMeshProUGUI labelPrevPreview;

    [Header("Fitur Tambahan")]
    public ScaleComparisonUI      scaleComparison;
    public TemperatureIndicatorUI temperatureIndicator;

    [Header("Canvas (untuk disable raycaster saat AR mode)")]
    [Tooltip("Drag Canvas utama yang berisi ZoomModeUI")]
    public GraphicRaycaster canvasRaycaster;

    [Header("Animasi Panel")]
    public float panelDuration = 0.4f;
    public float shownPosY    = 20f;
    public float panelSlideY  = 340f;

    private struct Info { public string name, distance, fact; }
    private Info[] data = new Info[]
    {
        new Info { name="Matahari",  distance="Pusat Tata Surya",             fact="Bintang di pusat tata surya. Suhu permukaan ~5.500 derajat C." },
        new Info { name="Merkurius", distance="57,9 juta km dari Matahari",   fact="Planet terkecil. Suhu 430 derajat C siang, -180 derajat C malam." },
        new Info { name="Venus",     distance="108,2 juta km dari Matahari",  fact="Planet terpanas (465 derajat C). Matahari terbit dari barat di Venus." },
        new Info { name="Bumi",      distance="149,6 juta km dari Matahari",  fact="Satu-satunya planet berpenghuni. 71% permukaan adalah air." },
        new Info { name="Mars",      distance="227,9 juta km dari Matahari",  fact="Gunung tertinggi tata surya: Olympus Mons (21,9 km)." },
        new Info { name="Jupiter",   distance="778,5 juta km dari Matahari",  fact="Planet terbesar. Badai Besar Merah berlangsung 350+ tahun." },
        new Info { name="Saturnus",  distance="1,43 miliar km dari Matahari", fact="Cincin dari es dan batuan. Punya 146 bulan." },
        new Info { name="Uranus",    distance="2,87 miliar km dari Matahari", fact="Berotasi miring 98 derajat. Planet terdingin: -197 derajat C." },
        new Info { name="Neptunus",  distance="4,50 miliar km dari Matahari", fact="Angin tercepat: 2.100 km/jam. 165 tahun Bumi per orbit." },
    };

    private bool       isZoomMode   = false;
    public  bool       IsZoomMode   => isZoomMode;
    private int        currentIndex = 0;
    private const int  TOTAL        = 9;
    private Vector3    origPos;
    private Quaternion origRot;
    private bool       origSaved    = false;
    private bool       panelVisible = false;
    private bool       tapBegan     = false;
    private bool       tapMoved     = false;

    void OnEnable()  { EnhancedTouchSupport.Enable(); }
    void OnDisable() { EnhancedTouchSupport.Disable(); }

    void Awake()  { }
    void Start()
    {
        if (arCamera == null) arCamera = Camera.main;
        HardReset();

        if (btnNext != null) btnNext.onClick.AddListener(GoNext);
        if (btnPrev != null) btnPrev.onClick.AddListener(GoPrev);
        if (btnExit != null) btnExit.onClick.AddListener(ExitZoomMode);

        SetupBtn(btnNext, new Color(1f,1f,1f,0.85f));
        SetupBtn(btnPrev, new Color(1f,1f,1f,0.85f));
        SetupBtn(btnExit, new Color(0.86f,0.2f,0.2f,0.9f));
    }

    void HardReset()
    {
        isZoomMode = false;
        if (canvasRaycaster != null)
            canvasRaycaster.enabled = false;
        if (zoomModeUI != null)
            zoomModeUI.SetActive(false);
        if (infoPanel != null)
        {
            infoPanel.alpha          = 0f;
            infoPanel.interactable   = false;
            infoPanel.blocksRaycasts = false;
        }
        if (infoPanelRect != null)
            infoPanelRect.anchoredPosition = new Vector2(0f, -panelSlideY);
        if (scaleComparison != null)
            scaleComparison.gameObject.SetActive(false);
        if (temperatureIndicator != null)
            temperatureIndicator.gameObject.SetActive(false);
        panelVisible = false;
    }

    void SetupBtn(Button b, Color c)
    {
        if (b == null) return;
        var col              = b.colors;
        col.normalColor      = c;
        col.highlightedColor = new Color(
            Mathf.Min(c.r+0.15f,1f),
            Mathf.Min(c.g+0.15f,1f),
            Mathf.Min(c.b+0.15f,1f), 1f);
        col.pressedColor     = new Color(c.r*0.7f, c.g*0.7f, c.b*0.7f, 1f);
        col.fadeDuration     = 0.1f;
        b.colors             = col;
    }

    private bool enteringZoom = false; // tambah variable ini di atas

    void Update()
    {
        
        if (isZoomMode || enteringZoom) return;  // cek keduanya
        if (!ARSessionManager.SystemPlaced) return;

        var touches = Touch.activeTouches;
        if (touches.Count != 1) { tapBegan = false; return; }

        var t = touches[0];
        Debug.Log($"Touch: {t.phase}, tapBegan: {tapBegan}");
        if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            tapBegan = true;
            tapMoved = false;
        }
        else if (t.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            if (t.delta.magnitude > 8f) tapMoved = true;
        }
        else if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            // Hanya EnterZoomMode kalau TIDAK sedang drag
            bool wasDragging = pinchController != null && pinchController.WasDragging;
            if (tapBegan && !tapMoved && !wasDragging)
                EnterZoomMode();
            tapBegan = false;
        }
    }

    int GetBodyIndex(string name)
    {
        for (int i = 0; i < data.Length; i++)
            if (data[i].name == name) return i;
        return -1;
    }

    public void EnterZoomMode()
    {
        if (isZoomMode) return;
        if (solarSystemManager == null ||
            solarSystemManager.SolarSystemRoot == null) return;

        isZoomMode   = true;
        solarSystemManager.SetOrbitPaused(true);
        currentIndex = 0;

        if (canvasRaycaster != null) canvasRaycaster.enabled = true;
        if (!origSaved)
        {
            origPos   = arCamera.transform.position;
            origRot   = arCamera.transform.rotation;
            origSaved = true;
        }
        if (zoomModeUI != null) zoomModeUI.SetActive(true);

        // Zoom ke Matahari (index 0) dulu
        ZoomToBody(currentIndex);
        UpdateNavPreview();
    }

    void ZoomToBody(int idx)
    {
        GameObject target = null;
        if (idx == 0)
            target = solarSystemManager.SpawnedSun;
        else if (solarSystemManager.SpawnedPlanets != null &&
                idx - 1 < solarSystemManager.SpawnedPlanets.Length)
            target = solarSystemManager.SpawnedPlanets[idx - 1];

        if (target == null) { ShowPanel(idx); return; }

        // Posisi tepat di depan kamera
        Vector3 camPos     = arCamera.transform.position;
        Vector3 camForward = arCamera.transform.forward;
        Vector3 desiredPos = camPos + camForward * zoomDistance;

        // Geser ROOT sebesar selisih posisi planet sekarang vs posisi yang diinginkan
        Vector3 delta      = desiredPos - target.transform.position;
        Vector3 newRootPos = solarSystemManager.SolarSystemRoot.transform.position + delta;

        DOTween.Kill(solarSystemManager.SolarSystemRoot.transform);
        solarSystemManager.SolarSystemRoot.transform
            .DOMove(newRootPos, zoomDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                foreach (var go in FindObjectsByType<MoonOrbit>(FindObjectsSortMode.None))
                    Debug.Log($"Moon {go.name}, pos: {go.transform.position}, planet: {go.planet?.position}");
                ShowPanel(idx);
            });
    }

    public void ExitZoomMode()
    {
        if (!isZoomMode) return;
        if (scaleComparison != null)      scaleComparison.gameObject.SetActive(false);
        if (temperatureIndicator != null) temperatureIndicator.gameObject.SetActive(false);

        HidePanel(() =>
        {
            isZoomMode = false;
            solarSystemManager.SetOrbitPaused(false);

            // Nonaktifkan raycaster kembali saat kembali ke AR mode
            if (canvasRaycaster != null)
                canvasRaycaster.enabled = false;

            if (zoomModeUI != null) zoomModeUI.SetActive(false);
            if (origSaved)
            {
                arCamera.transform.DOMove(origPos, zoomDuration).SetEase(Ease.OutCubic);
                arCamera.transform.DORotateQuaternion(origRot, zoomDuration)
                    .SetEase(Ease.OutCubic);
            }
        });
    }

    public void GoNext() => NavigateTo((currentIndex+1)%TOTAL, 1);
    public void GoPrev() => NavigateTo((currentIndex-1+TOTAL)%TOTAL, -1);

    void NavigateTo(int idx, int dir)
    {
        if (idx == currentIndex) return;
        if (scaleComparison != null)      scaleComparison.gameObject.SetActive(false);
        if (temperatureIndicator != null) temperatureIndicator.gameObject.SetActive(false);

        currentIndex = idx;
        UpdateNavPreview();

        ZoomToBody(idx);

        Button pb = dir > 0 ? btnNext : btnPrev;
        if (pb != null) pb.transform.DOPunchScale(Vector3.one*0.2f, 0.2f, 5, 0.5f);

        DOTween.Kill(infoPanelRect);
        float outX = dir > 0 ? -500f : 500f;
        infoPanelRect.DOAnchorPosX(outX, panelDuration*0.4f).SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                currentIndex = idx;
                FillPanel(idx);
                UpdateNavPreview();
                infoPanelRect.anchoredPosition = new Vector2(-outX, shownPosY);
                infoPanelRect.DOAnchorPos(new Vector2(0f, shownPosY), panelDuration*0.6f)
                    .SetEase(Ease.OutCubic).OnComplete(() => ShowExtras(idx));
            });
    }

    void ShowPanel(int idx)
    {
        FillPanel(idx);
        if (infoPanel == null || infoPanelRect == null) return;

        DOTween.Kill(infoPanelRect);
        DOTween.Kill(infoPanel);

        infoPanelRect.anchoredPosition = new Vector2(0f, -panelSlideY);
        infoPanel.alpha          = 0f;
        infoPanel.interactable   = true;
        infoPanel.blocksRaycasts = true;

        infoPanelRect.DOAnchorPos(new Vector2(0f, shownPosY), panelDuration)
            .SetEase(Ease.OutBack);
        infoPanel.DOFade(1f, panelDuration*0.7f)
            .OnComplete(() => ShowExtras(idx));

        panelVisible = true;
    }

    void HidePanel(System.Action done = null)
    {
        if (!panelVisible || infoPanelRect == null) { done?.Invoke(); return; }
        DOTween.Kill(infoPanelRect);
        DOTween.Kill(infoPanel);

        infoPanelRect.DOAnchorPos(new Vector2(0f, -panelSlideY), panelDuration)
            .SetEase(Ease.InBack);
        infoPanel.DOFade(0f, panelDuration*0.5f).OnComplete(() =>
        {
            infoPanel.interactable   = false;
            infoPanel.blocksRaycasts = false;
            panelVisible = false;
            done?.Invoke();
        });
    }

    void FillPanel(int idx)
    {
        if (idx < 0 || idx >= data.Length) return;
        if (labelBodyName != null) labelBodyName.text = data[idx].name;
        if (labelDistance != null) labelDistance.text = data[idx].distance;
        if (labelFact     != null) labelFact.text     = data[idx].fact;
    }

    void ShowExtras(int idx)
    {
        if (scaleComparison != null)
        {
            scaleComparison.gameObject.SetActive(true);
            scaleComparison.ShowComparison(idx);
        }
        if (temperatureIndicator != null)
        {
            temperatureIndicator.gameObject.SetActive(true);
            temperatureIndicator.ShowTemperature(idx);
        }
    }

    void UpdateNavPreview()
    {
        int n = (currentIndex+1)%TOTAL;
        int p = (currentIndex-1+TOTAL)%TOTAL;
        if (labelNextPreview != null) labelNextPreview.text = data[n].name;
        if (labelPrevPreview != null) labelPrevPreview.text = data[p].name;
    }
}