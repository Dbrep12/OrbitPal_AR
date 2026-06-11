using UnityEngine;
using System.Collections;

public class CometSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject cometPrefab;
    public float minInterval = 12f;
    public float maxInterval = 30f;
    public float cometSpeed  = 4f;
    public float spawnRadius = 7f;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip   cometSound;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(5f);

        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
            SpawnComet();
        }
    }

    void SpawnComet()
    {
        if (Camera.main == null) return;

        Vector3 camPos   = Camera.main.transform.position;
        Vector3 spawnDir = Random.onUnitSphere;
        spawnDir.y = Mathf.Abs(spawnDir.y) * 0.3f;
        Vector3 spawnPos  = camPos + spawnDir * spawnRadius;
        Vector3 targetPos = camPos + (-spawnDir) * spawnRadius;
        Vector3 direction = (targetPos - spawnPos).normalized;

        GameObject comet = Instantiate(cometPrefab, spawnPos,
                                       Quaternion.LookRotation(direction));

        Rigidbody rb = comet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = direction * cometSpeed;

        if (sfxSource != null && cometSound != null)
            sfxSource.PlayOneShot(cometSound, 0.6f);

        Destroy(comet, 8f);
    }
}