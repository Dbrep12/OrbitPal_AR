using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PinchController : MonoBehaviour
{
    [Header("Referensi")]
    public SolarSystemManager solarSystemManager;
    public ZoomModeController zoomModeController;
    public Camera             arCamera;

    [Header("AR Mode Scale")]
    public float minScale         = 0.02f;
    public float maxScale         = 0.15f;
    public float scaleSensitivity = 1f;

    [Header("Zoom Mode Camera")]
    public float minZoomDist      = 0.15f;
    public float maxZoomDist      = 1.2f;
    public float zoomSensitivity  = 1f;

    [Header("Drag Settings")]
    public float dragSensitivity  = 0.0003f;
    public float dragThreshold    = 8f;

    [Header("Scale Indicator (opsional)")]
    public CanvasGroup           scaleIndicator;
    public TMPro.TextMeshProUGUI scaleLabel;

    private bool    isPinching   = false;
    private float   startDist    = 0f;
    private float   startScale   = 0f;
    private float   startCamDist = 0f;

    // Drag
    private bool    isDragging   = false;
    private bool    dragMoved    = false;
    private Vector2 lastDragPos;
    public bool WasDragging => dragMoved;

    void OnEnable()  { EnhancedTouchSupport.Enable(); }
    void OnDisable() { EnhancedTouchSupport.Disable(); }

    void Start()
    {
        if (arCamera == null) arCamera = Camera.main;
        if (scaleIndicator != null)
        {
            scaleIndicator.alpha = 0f;
            scaleIndicator.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!ARSessionManager.SystemPlaced) return;
        if (solarSystemManager.SolarSystemRoot == null) return;

        var touches = Touch.activeTouches;

        // ── 2 JARI: Pinch scale / zoom ──────────────────
        if (touches.Count == 2)
        {
            isDragging = false;
            dragMoved  = false;

            float dist = Vector2.Distance(
                touches[0].screenPosition,
                touches[1].screenPosition);

            bool inZoom = zoomModeController != null &&
                          zoomModeController.IsZoomMode;

            if (!isPinching)
            {
                isPinching = true;
                startDist  = dist;

                if (inZoom)
                {
                    Vector3 center = solarSystemManager.SolarSystemRoot.transform.position;
                    startCamDist   = Vector3.Distance(arCamera.transform.position, center);
                }
                else
                {
                    startScale = solarSystemManager.SolarSystemRoot.transform.localScale.x;
                }
                ShowIndicator();
            }
            else
            {
                if (startDist <= 0f) return;
                float ratio = dist / startDist;

                if (inZoom)
                    DoZoomCamera(ratio);
                else
                    DoScaleSystem(ratio);
            }
            return;
        }

        // Reset pinch
        if (isPinching)
        {
            isPinching = false;
            HideIndicator();
        }

        // ── 1 JARI: Drag (hanya di AR mode, bukan zoom mode) ──
        if (touches.Count == 1 && 
            zoomModeController != null && 
            !zoomModeController.IsZoomMode)
        {
            var t = touches[0];

            if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                isDragging  = true;
                dragMoved   = false;
                lastDragPos = t.screenPosition;
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                if (!isDragging) return;

                float moveMag = t.delta.magnitude;
                if (moveMag > dragThreshold)
                {
                    dragMoved = true;

                    Vector2 delta = t.screenPosition - lastDragPos;
                    lastDragPos   = t.screenPosition;

                    // Geser sesuai orientasi kamera (horizontal plane)
                    Vector3 right   = arCamera.transform.right;
                    Vector3 forward = arCamera.transform.forward;
                    right.y   = 0f;
                    forward.y = 0f;

                    if (right.sqrMagnitude > 0.001f)   right.Normalize();
                    if (forward.sqrMagnitude > 0.001f) forward.Normalize();

                    Vector3 worldMove = right   * delta.x * dragSensitivity +
                                        forward * delta.y * dragSensitivity;

                    solarSystemManager.SolarSystemRoot.transform.position += worldMove;
                }
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
        else if (touches.Count == 0)
        {
            isDragging = false;
            dragMoved  = false;
        }
    }

    void DoScaleSystem(float ratio)
    {
        var root = solarSystemManager.SolarSystemRoot;
        if (root == null) return;

        float newScale = Mathf.Clamp(
            startScale * Mathf.Pow(ratio, scaleSensitivity),
            minScale, maxScale);

        root.transform.localScale = Vector3.one * newScale;

        float initial = solarSystemManager.systemScale;
        float display = newScale / initial;
        SetLabel($"{display:F1}×");
    }

    void DoZoomCamera(float ratio)
    {
        if (arCamera == null) return;
        var root = solarSystemManager.SolarSystemRoot;
        if (root == null) return;

        Vector3 center      = root.transform.position;
        Vector3 camToCenter = (center - arCamera.transform.position).normalized;

        float newDist = Mathf.Clamp(
            startCamDist / Mathf.Pow(ratio, zoomSensitivity),
            minZoomDist, maxZoomDist);

        arCamera.transform.position = center - camToCenter * newDist;
        SetLabel($"{(startCamDist / Mathf.Max(newDist, 0.01f)):F1}×");
    }

    void ShowIndicator()
    {
        if (scaleIndicator == null) return;
        scaleIndicator.gameObject.SetActive(true);
        DOTween.Kill(scaleIndicator);
        scaleIndicator.DOFade(1f, 0.2f);
    }

    void HideIndicator()
    {
        if (scaleIndicator == null) return;
        DOTween.Kill(scaleIndicator);
        scaleIndicator.DOFade(0f, 0.3f)
            .OnComplete(() => scaleIndicator.gameObject.SetActive(false));
    }

    void SetLabel(string txt)
    {
        if (scaleLabel != null) scaleLabel.text = txt;
    }
}