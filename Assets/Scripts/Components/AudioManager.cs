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

    private IndependentTimer combatTimer;
    public float combatTimeout = 10f;
    private bool fading;

    public float fadeDuration = 2f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayIdleTrack();
        PlayAmbience();

        InitializeCombatTimer();
    }

    private void Update()
    {
        if (!fading)
        {
            if (combatTimer.Finished())
            {
                SwitchToIdleTrack();
            }
        }

        // Debug hotkey for testing audio
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P key pressed");
            if (musicSource.clip == idleTrack)
            {
                ResetCombatTimer();
            }
            else
            {
                Debug.Log("Switching to idle track");
                combatTimer = new IndependentTimer(combatTimeout);
                combatTimer.Start(); // It's okat to start the combat timer without finishing it here because the Update function looks to see if it's finished
                StartCoroutine(FadeTracks(musicSource.clip, idleTrack));
            }
        }
    }

    public void ResetCombatTimer()
    {
        Debug.Log("Combat Timer Reset");
        InitializeCombatTimer();
        if (musicSource.clip != combatTrack)
        {
            StartCoroutine(FadeTracks(musicSource.clip, combatTrack));
        }
    }

    private void InitializeCombatTimer()
    {
        combatTimer = new IndependentTimer(combatTimeout);
        combatTimer.Start();
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
        var fadeTimer = new IndependentTimer(fadeDuration);
        fadeTimer.Start();

        while (!fadeTimer.Finished())
        {
            float t = fadeTimer.Parameterized(); 
            musicSource.volume = Mathf.Lerp(startVolume, 0, t);
            yield return null;
        }

        musicSource.Stop();

        // Set and play the new track
        musicSource.clip = to;
        musicSource.volume = 0;
        musicSource.Play();

        // Fade in new track
        fadeTimer.Start();
        while (!fadeTimer.Finished())
        {
            float t = fadeTimer.Parameterized();
            musicSource.volume = Mathf.Lerp(0, startVolume, t);
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
}
