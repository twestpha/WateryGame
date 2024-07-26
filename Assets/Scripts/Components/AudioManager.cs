using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip idleTrack;
    public AudioClip combatTrack;
    public AudioSource musicSource;

    public AudioClip ambienceClip;
    public AudioSource ambienceSource;

    private float combatTimer;
    public float combatTimeout = 10f;
    private bool fading;

    public float fadeDuration = 2f;

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
        PlayIdleTrack();
        PlayAmbience();
    }

    private void Update()
    {
        if (!fading)
        {
            combatTimer += Time.deltaTime;
            if (combatTimer >= combatTimeout)
            {
                SwitchToIdleTrack();
            }
        }

        // Debug hotkey for testing audio (DELETE WHEN PROPERLY IMPLEMENTED!)
        if (Input.GetKeyDown(KeyCode.P))
        {
            SimulateCombat();
        }
    }

    public void ResetCombatTimer()
    {
        combatTimer = 0f;
        if (musicSource.clip != combatTrack)
        {
            StartCoroutine(FadeTracks(idleTrack, combatTrack));
        }
    }

    private void PlayIdleTrack()
    {
        musicSource.clip = idleTrack;
        musicSource.Play();
    }

    private void PlayCombatTrack()
    {
        musicSource.clip = combatTrack;
        musicSource.Play();
    }

    private void SwitchToIdleTrack()
    {
        if (musicSource.clip != idleTrack)
        {
            StartCoroutine(FadeTracks(musicSource.clip, idleTrack));
        }
    }

    private IEnumerator FadeTracks(AudioClip from, AudioClip to)
    {
        fading = true;
        float startVolume = musicSource.volume;

        // Fade out current track
        for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();

        // Set and play the new track
        musicSource.clip = to;
        musicSource.volume = 0;
        musicSource.Play();

        // Fade in new track
        for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, startVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
        fading = false;
    }

    private void PlayAmbience()
    {
        ambienceSource.clip = ambienceClip;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }

    private void SimulateCombat()
    {
        if (musicSource.clip == idleTrack)
        {
            ResetCombatTimer();
        }
        else
        {
            combatTimer = combatTimeout;
        }
    }
}
