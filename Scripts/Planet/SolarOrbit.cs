using UnityEngine;

public class SolarOrbit : MonoBehaviour
{
    public float orbitRadius = 1f;
    public float orbitSpeed  = 30f;

    private float   angle      = 0f;
    private Vector3 centerPos;

    void Start()
    {
        angle     = Random.Range(0f, 360f);
        // Simpan posisi tengah (SolarSystemRoot)
        centerPos = transform.parent != null ? 
                    transform.parent.position : Vector3.zero;
    }

    void Update()
    {
        angle += orbitSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        float rad = angle * Mathf.Deg2Rad;

        // Orbit mengelilingi parent (SolarSystemRoot = posisi matahari)
        transform.localPosition = new Vector3(
            Mathf.Cos(rad) * orbitRadius,
            0f,
            Mathf.Sin(rad) * orbitRadius
        );
    }
}