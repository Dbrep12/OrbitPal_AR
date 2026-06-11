using UnityEngine;
using System.Collections;

public class AdaptiveMusicManager : MonoBehaviour
{
    public static AdaptiveMusicManager Instance;

    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    [Header("Music Clips")]
    public AudioClip clipCalm;
    public AudioClip clipIntense;
    public AudioClip clipEdit;

    [Header("Settings")]
    public float crossfadeDuration = 2f;
    public float velocityThreshold = 0.4f;
    public float checkInterval     = 0.5f;

    private AudioSource activeSource;
    private AudioClip   currentClip;
    private Vector3     lastCamPos;
    private bool        isEditMode = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        activeSource = sourceA;
        sourceA.loop = true;
        sourceB.loop = true;
        sourceA.volume = 1f;
        sourceB.volume = 0f;

        lastCamPos = Camera.main ? Camera.main.transform.position : Vector3.zero;
        StartCoroutine(CheckVelocityLoop());
    }

    IEnumerator CheckVelocityLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            if (isEditMode || Camera.main == null) continue;

            float velocity = (Camera.main.transform.position - lastCamPos).magnitude
                             / checkInterval;
            lastCamPos = Camera.main.transform.position;

            if (velocity > velocityThreshold && currentClip != clipIntense)
                CrossfadeTo(clipIntense);
            else if (velocity <= velocityThreshold && currentClip != clipCalm)
                CrossfadeTo(clipCalm);
        }
    }

    public void SwitchToEditMusic()
    {
        isEditMode = true;
        CrossfadeTo(clipEdit);
    }

    public void SwitchToARMusic()
    {
        isEditMode = false;
        CrossfadeTo(clipCalm);
    }

    void PlayClip(AudioClip clip)
    {
        currentClip = clip;
        activeSource.clip = clip;
        activeSource.Play();
    }

    void CrossfadeTo(AudioClip newClip)
    {
        if (currentClip == newClip) return;
        AudioSource next = (activeSource == sourceA) ? sourceB : sourceA;
        next.clip   = newClip;
        next.volume = 0f;
        next.Play();
        StartCoroutine(CrossfadeRoutine(activeSource, next));
        activeSource = next;
        currentClip  = newClip;
    }

    IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to)
    {
        float elapsed = 0f;
        while (elapsed < crossfadeDuration)
        {
            elapsed    += Time.deltaTime;
            float t     = elapsed / crossfadeDuration;
            from.volume = Mathf.Lerp(1f, 0f, t);
            to.volume   = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        from.volume = 0f;
        to.volume   = 1f;
        from.Stop();
    }
}