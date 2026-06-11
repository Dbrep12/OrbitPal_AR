using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System.Collections;

public class ARSessionManager : MonoBehaviour
{
    [Header("Referensi")]
    public ARRaycastManager   raycastManager;
    public ARPlaneManager     planeManager;
    public SolarSystemManager solarSystemManager;

    [Header("UI")]
    public GameObject tapHintUI;
    public GameObject tapAgainHintUI;

    // FIX: Static agar bisa diakses ZoomModeController
    // tanpa perlu referensi langsung
    public static bool SystemPlaced { get; private set; } = false;

    // Instance property untuk backward compatibility
    public bool Placed => SystemPlaced;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void OnEnable()  { EnhancedTouchSupport.Enable(); }
    void OnDisable() { EnhancedTouchSupport.Disable(); }

    // Reset static flag saat scene load ulang
    void Awake()
    {
        SystemPlaced = false;
    }

    void Update()
    {
        if (SystemPlaced) return;

        if (Touch.activeTouches.Count == 0) return;
        var touch = Touch.activeTouches[0];
        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;
        if (IsTouchOverUI(touch.screenPosition)) return;

        if (raycastManager.Raycast(touch.screenPosition, hits,
                                   TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            solarSystemManager.SpawnSolarSystem(hitPose.position, hitPose.rotation);

            
            Debug.Log("[ARS] SystemPlaced set to true");
            StartCoroutine(SetPlacedNextFrame());
            planeManager.requestedDetectionMode = PlaneDetectionMode.None;
            SetPlanesActive(false);

            if (tapHintUI      != null) tapHintUI.SetActive(false);
            if (tapAgainHintUI != null) tapAgainHintUI.SetActive(true);
        }
    }

    IEnumerator SetPlacedNextFrame()
    {
        yield return new WaitForSeconds(0.5f); // tunggu 0.5 detik
        SystemPlaced = true;
        Debug.Log("[ARS] SystemPlaced set to true");
    }

    void SetPlanesActive(bool active)
    {
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(active);
    }

    bool IsTouchOverUI(Vector2 pos)
    {
        var pd = new UnityEngine.EventSystems.PointerEventData(
            UnityEngine.EventSystems.EventSystem.current) { position = pos };
        var res = new System.Collections.Generic.List<
            UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pd, res);
        return res.Count > 0;
    }
}