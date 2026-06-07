using UnityEngine;

/*
 * SoundGenerator
 * --------------
 * Emits sound from this object.
 * Supports:
 *   - Continuous looping audio
 *   - Random interval one-shot sounds
 *   - External one-shot triggers
 *
 * Each object using this component works independently.
 */
public class SoundGenerator : MonoBehaviour
{
    [Header("Looping sound")]
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private float loopVolume = 1f;

    [Header("Random SFX")]
    [SerializeField] private AudioClip[] randomClips;
    [SerializeField] private float minDelay = 3f;
    [SerializeField] private float maxDelay = 8f;
    [SerializeField] private float randomVolume = 1f;

    [Header("External one-shots")]
    [SerializeField] private float externalVolume = 1f;

    private AudioSource source;
    private float timer;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f; 
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 0.5f;
        source.maxDistance = 50f;
        source.playOnAwake = false;
    }

    private void Start()
    {
        if (loopClip != null)
        {
            source.loop = true;
            source.clip = loopClip;
            source.volume = loopVolume;
            source.Play();
        }

        ResetTimer();
    }

    private void Update()
    {
        if (randomClips == null || randomClips.Length == 0)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            AudioClip clip = randomClips[UnityEngine.Random.Range(0, randomClips.Length)];
            source.PlayOneShot(clip, randomVolume);
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timer = UnityEngine.Random.Range(minDelay, maxDelay);
    }

    /*
     * Plays a one-shot sound triggered by external scripts.
     */
    public void PlayExternalOneShot(AudioClip clip)
    {
        if (clip == null)
            return;

        source.PlayOneShot(clip, externalVolume);
    }
}
