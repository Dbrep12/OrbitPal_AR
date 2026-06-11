using UnityEngine;

/// <summary>
/// MoonOrbit.cs — dipasang otomatis oleh SolarSystemManager
/// Orbit mengelilingi planet induk secara real-time
/// </summary>
public class MoonOrbit : MonoBehaviour
{
    public Transform planet;
    public float     radius = 0.05f;
    public float     speed  = 60f;

    private float angle = 0f;

    void Start()
    {
        angle = Random.Range(0f, 360f);
    }

    void Update()
    {
        if (planet == null) return;

        angle += speed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        float rad = angle * Mathf.Deg2Rad;

        // FIX: ikuti posisi planet yang terus bergerak
        transform.position = planet.position + new Vector3(
            Mathf.Cos(rad) * radius,
            0f,
            Mathf.Sin(rad) * radius);
    }

    void LateUpdate()
    {
        if (!enabled) return; // skip kalau disabled
        if (planet == null) return;

        // Tetap update posisi mengikuti planet meski orbit pause
        float rad = angle * Mathf.Deg2Rad;
        transform.position = planet.position + new Vector3(
            Mathf.Cos(rad) * radius,
            0f,
            Mathf.Sin(rad) * radius);
    }
}