using UnityEngine;
using System.Collections;

public class AudioManagerAlt : MonoBehaviour
{
    public static AudioManagerAlt Instance;

    public AudioClip idleTrack;
    public AudioClip combatTrack;
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public float fadeDuration = 1.0f;

    private bool isFading;
    private AudioSource activeSource;
    private AudioSource nextSource;
    private AudioClip activeClip;
    private float trackPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        activeSource = audioSource1;
        nextSource = audioSource2;
        activeClip = idleTrack;
        PlayTrack(idleTrack, audioSource1);
    }

    private void Update()
    {
        // Debug hotkey for testing (DELETE LATER)
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleTrack();
        }
    }

    private void ToggleTrack()
    {
        if (!isFading)
        {
            AudioClip newClip = (activeClip == idleTrack) ? combatTrack : idleTrack;
            StartCoroutine(CrossfadeTracks(newClip));
        }
    }

    private void PlayTrack(AudioClip clip, AudioSource source)
    {
        source.clip = clip;
        source.volume = 1;
        source.time = 0; // Start from the beginning to ensure no offset
        source.Play();
    }

    private IEnumerator CrossfadeTracks(AudioClip newClip)
    {
        isFading = true;

        // Save current track position
        trackPosition = activeSource.time;

        // Swap sources
        AudioSource tempSource = activeSource;
        activeSource = nextSource;
        nextSource = tempSource;

        // Set up the new source
        nextSource.clip = newClip;
        nextSource.volume = 0;
        nextSource.time = trackPosition; // Use saved track position
        nextSource.Play();

        // Fade out the active track and fade in the new track simultaneously
        for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
        {
            float blend = t / fadeDuration;
            activeSource.volume = Mathf.Lerp(1, 0, blend);
            nextSource.volume = Mathf.Lerp(0, 1, blend);
            yield return null;
        }

        // Ensure final volume levels
        activeSource.volume = 0;
        nextSource.volume = 1;

        // Stop the old track
        activeSource.Stop();
        activeClip = newClip;

        isFading = false;
    }
}
