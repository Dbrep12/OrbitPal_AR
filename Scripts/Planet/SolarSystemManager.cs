using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SolarSystemManager : MonoBehaviour
{
    [System.Serializable]
    public class PlanetData
    {
        public string       name;
        public GameObject   prefab;
        public float        orbitRadius;
        public float        orbitSpeed;
        public float        rotationSpeed;
        public float        scale;
        public float        orbitTilt;
        public GameObject[] moons;
        public float[]      moonOrbitRadius;
        public float[]      moonOrbitSpeed;
        public bool         hasRing;
    }

    [Header("Referensi Asset")]
    public GameObject sunPrefab;
    public GameObject moonPrefab;

    [Header("Asteroid")]
    public GameObject asteroidBeltPrefab;
    public GameObject asteroidSpherePrefab;

    [Header("Planet Data")]
    public PlanetData[] planets;

    [Header("Skala Sistem")]
    public float systemScale     = 0.04f;  // FIX: dikecilkan dari 0.08 → 0.04
    public float sunScale        = 0.08f;  // FIX: dikecilkan dari 0.15 → 0.08
    public float orbitSpeedMulti = 1f;

    [Header("Asteroid Belt")]
    public int   beltCount       = 18;     // FIX: dikurangi dari 24 → 18
    public float beltOrbitRadius = 28f;
    public float beltOrbitSpeed  = 12f;
    public float beltWidth       = 3f;
    public float beltScaleMin    = 0.005f; // FIX: diperkecil
    public float beltScaleMax    = 0.015f; // FIX: diperkecil

    [Header("Asteroid Melintas")]
    public float passingInterval = 25f;
    public int   passingCount    = 2;
    public float passingSpeed    = 0.4f;
    public float passingScaleMin = 0.005f; // FIX: diperkecil
    public float passingScaleMax = 0.015f;

    // Public akses untuk ZoomModeController & PinchController
    public GameObject[]  SpawnedPlanets  => spawnedPlanets;
    public GameObject    SpawnedSun      => spawnedSun;
    public GameObject    SolarSystemRoot => solarSystemRoot;

    private GameObject       solarSystemRoot;
    private GameObject       spawnedSun;
    private GameObject[]     spawnedPlanets;
    private List<GameObject> beltObjects  = new List<GameObject>();
    private float            passingTimer = 0f;
    private bool             systemSpawned = false;

    // ─────────────────────────────────────────────────────
    public void SpawnSolarSystem(Vector3 position, Quaternion rotation)
    {
        if (solarSystemRoot != null) Destroy(solarSystemRoot);
        beltObjects.Clear();
        systemSpawned = false;

        solarSystemRoot = new GameObject("SolarSystemRoot");
        solarSystemRoot.transform.position = position + Vector3.up * 0.05f;
        solarSystemRoot.transform.rotation = rotation;

        // Matahari di tengah
        if (sunPrefab != null)
        {
            spawnedSun = Instantiate(sunPrefab, solarSystemRoot.transform);
            spawnedSun.transform.localPosition = Vector3.zero;
            spawnedSun.transform.localScale    = Vector3.one * sunScale;

            var sr   = spawnedSun.AddComponent<SelfRotate>();
            sr.speed = 2f;

            if (spawnedSun.GetComponent<Collider>() == null)
            {
                var sc    = spawnedSun.AddComponent<SphereCollider>();
                sc.radius = 0.5f;
            }
            var sunInfo        = spawnedSun.AddComponent<PlanetInfo>();
            sunInfo.planetName = "Sun";
        }

        spawnedPlanets = new GameObject[planets.Length];
        for (int i = 0; i < planets.Length; i++)
            SpawnPlanet(i);

        SpawnAsteroidBelt();

        passingTimer  = 0f;
        systemSpawned = true;
    }

    void SpawnPlanet(int index)
    {
        PlanetData data = planets[index];
        if (data.prefab == null) return;

        GameObject planet = Instantiate(data.prefab, solarSystemRoot.transform);
        planet.name = data.name;

        float startAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius     = data.orbitRadius * systemScale;

        planet.transform.localPosition = new Vector3(
            Mathf.Cos(startAngle) * radius, 0f,
            Mathf.Sin(startAngle) * radius);
        planet.transform.localScale = Vector3.one * data.scale * systemScale;

        var orbit         = planet.AddComponent<SolarOrbit>();
        orbit.orbitRadius = radius;
        orbit.orbitSpeed  = data.orbitSpeed * orbitSpeedMulti;

        var selfRot   = planet.AddComponent<SelfRotate>();
        selfRot.speed = data.rotationSpeed;

        if (planet.GetComponent<Collider>() == null)
        {
            var col    = planet.AddComponent<SphereCollider>();
            col.radius = 0.5f;
        }

        var info        = planet.AddComponent<PlanetInfo>();
        info.planetName = data.name;

        spawnedPlanets[index] = planet;

        // Bulan
        if (data.moons != null)
        {
            for (int m = 0; m < data.moons.Length; m++)
            {
                if (data.moons[m] == null) continue;

                GameObject moon = Instantiate(data.moons[m], solarSystemRoot.transform);
                moon.name = data.name + "_Moon" + m;

                float moonRadius = (data.moonOrbitRadius != null && 
                                    data.moonOrbitRadius.Length > m)
                    ? data.moonOrbitRadius[m] * systemScale 
                    : 0.05f;

                float moonSpeed = (data.moonOrbitSpeed != null && 
                                   data.moonOrbitSpeed.Length > m)
                    ? data.moonOrbitSpeed[m] 
                    : 60f;

                Debug.Log($"Moon {moon.name} orbit radius: {moonRadius}, speed: {moonSpeed}, " +
                          $"rawRadius: {(data.moonOrbitRadius != null && data.moonOrbitRadius.Length > m ? data.moonOrbitRadius[m] : -1)}");
                moon.transform.localScale = planet.transform.localScale * 0.27f;

                var moonOrbit     = moon.AddComponent<MoonOrbit>();
                moonOrbit.planet  = planet.transform;
                moonOrbit.radius  = moonRadius;
                moonOrbit.speed   = moonSpeed;
                Debug.Log($"Moon {moon.name} orbit radius: {moonRadius}, speed: {moonSpeed}");
            }
        }
    }

    void SpawnAsteroidBelt()
    {
        if (asteroidBeltPrefab == null) return;

        for (int i = 0; i < beltCount; i++)
        {
            float radiusVar = Random.Range(-beltWidth * 0.5f, beltWidth * 0.5f);
            float radius    = (beltOrbitRadius + radiusVar) * systemScale;
            float angle     = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            Vector3 localPos = new Vector3(
                Mathf.Cos(angle) * radius,
                Random.Range(-0.001f, 0.001f), // FIX: hampir flat horizontal
                Mathf.Sin(angle) * radius);

            GameObject asteroid = Instantiate(asteroidBeltPrefab, solarSystemRoot.transform);
            asteroid.name = "BeltAsteroid_" + i;
            asteroid.transform.localPosition = localPos;

            float sc = Random.Range(beltScaleMin, beltScaleMax);
            asteroid.transform.localScale    = Vector3.one * sc;
            asteroid.transform.localRotation = Random.rotation;

            var orbit         = asteroid.AddComponent<SolarOrbit>();
            orbit.orbitRadius = radius;
            orbit.orbitSpeed  = beltOrbitSpeed * Random.Range(0.85f, 1.15f) * orbitSpeedMulti;

            var selfRot   = asteroid.AddComponent<SelfRotate>();
            selfRot.speed = Random.Range(5f, 20f);

            beltObjects.Add(asteroid);
        }
    }

    void Update()
    {
        if (!systemSpawned || solarSystemRoot == null) return;

        passingTimer += Time.deltaTime;
        if (passingTimer >= passingInterval)
        {
            passingTimer = 0f;
            StartCoroutine(SpawnPassingAsteroids());
        }
    }

    IEnumerator SpawnPassingAsteroids()
    {
        for (int i = 0; i < passingCount; i++)
        {
            SpawnOnePassingAsteroid();
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    void SpawnOnePassingAsteroid()
    {
        if (asteroidSpherePrefab == null || solarSystemRoot == null) return;

        Vector3 center    = solarSystemRoot.transform.position;
        float   angle     = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 dir       = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        float   edge      = planets[planets.Length - 1].orbitRadius * systemScale * 1.4f;

        // FIX: height offset sangat kecil agar tetap horizontal
        float   heightOff = Random.Range(-0.01f, 0.02f);

        Vector3 spawnPos = center + dir * edge + Vector3.up * heightOff;
        Vector3 endPos   = center + (-dir) * edge + Vector3.up * heightOff;
        Vector3 moveDir  = (endPos - spawnPos).normalized;

        GameObject asteroid = Instantiate(asteroidSpherePrefab,
            spawnPos, Quaternion.LookRotation(moveDir));

        float sc = Random.Range(passingScaleMin, passingScaleMax);
        asteroid.transform.localScale = Vector3.one * sc;

        var selfRot   = asteroid.AddComponent<SelfRotate>();
        selfRot.speed = Random.Range(15f, 40f);

        Rigidbody rb  = asteroid.GetComponent<Rigidbody>();
        if (rb == null) rb = asteroid.AddComponent<Rigidbody>();
        rb.useGravity     = false;
        rb.linearVelocity = moveDir * passingSpeed;

        float lifetime = (edge * 2f) / passingSpeed + 1f;
        Destroy(asteroid, lifetime);
    }

    public void SetOrbitPaused(bool paused)
    {
        if (solarSystemRoot == null) return;
        
        var orbits = solarSystemRoot.GetComponentsInChildren<SolarOrbit>();
        foreach (var o in orbits) o.enabled = !paused;
        
        var rotations = solarSystemRoot.GetComponentsInChildren<SelfRotate>();
        foreach (var r in rotations) r.enabled = !paused;
        
        var moonOrbits = solarSystemRoot.GetComponentsInChildren<MoonOrbit>();
        foreach (var m in moonOrbits) m.enabled = !paused;
    }
    public GameObject GetSolarSystemRoot() => solarSystemRoot;
}