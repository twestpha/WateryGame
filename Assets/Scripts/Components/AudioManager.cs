using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public float combatTimeout = 10f;
    public float fadeDuration = 2f;
    [Space(10)]
    public AudioClip idleTrack;
    public AudioClip combatTrack;
    public AudioClip bossTrack;
    public AudioSource musicSource;
    [Space(10)]
    public AudioClip ambienceClip;
    public AudioSource ambienceSource;
    
    private enum MusicState {
        Idle,
        Combat,
        Boss,
    }
    
    private enum FadeState {
        None,
        FadeOut,
        FadeIn,
    }
        
    private FadeState fadeState;
    
    private MusicState previousState;
    private MusicState currentState;

    private IndependentTimer combatTimer;
    private IndependentTimer fadeTimer;
    
    private float maxVolume;
    
    void Awake(){
        instance = this;
        maxVolume = musicSource.volume;
    }

    void Start(){
        combatTimer = new IndependentTimer(combatTimeout);
        fadeTimer = new IndependentTimer(fadeDuration);
        
        // Always play ambient bg sfx
        ambienceSource.clip = ambienceClip;
        ambienceSource.loop = true;
        ambienceSource.Play();
        
        // Kick off idle music
        musicSource.clip = idleTrack;
        musicSource.loop = true;
        musicSource.Play();
    }

    void Update(){
        // If in combat and timer elapsed, leave combat
        if(currentState == MusicState.Combat && combatTimer.Finished()){
            currentState = MusicState.Idle;
        }
        
        // If state changed
        if(currentState != previousState){
            fadeState = FadeState.FadeOut;
            fadeTimer.Start();
        }
        
        if(fadeState != FadeState.None){
            if(fadeState == FadeState.FadeOut){
                musicSource.volume = maxVolume * (1.0f - fadeTimer.Parameterized());
                
                if(fadeTimer.Finished()){
                    musicSource.volume = 0.0f;
                    
                    if(currentState == MusicState.Idle){
                        musicSource.clip = idleTrack;
                    } else if(currentState == MusicState.Combat){
                        musicSource.clip = combatTrack;
                    } else if(currentState == MusicState.Boss){
                        musicSource.clip = bossTrack;
                    }
                    musicSource.Play();
                    
                    fadeState = FadeState.FadeIn;
                    fadeTimer.Start();
                }
            } else if(fadeState == FadeState.FadeIn){
                musicSource.volume = maxVolume * fadeTimer.Parameterized();
                
                if(fadeTimer.Finished()){
                    musicSource.volume = maxVolume;
                }
            }
        }
        
        previousState = currentState;
        
        // if(Input.GetKeyDown(KeyCode.P)){
        //     NotifyOfCombat(true);
        // }
        // if(Input.GetKeyDown(KeyCode.L)){
        //     NotifyBossfightFinished();
        // }
    }
    
    public void NotifyOfCombat(bool inBossFight = false){
        if(!inBossFight){ return; }// temp
        combatTimer.Start();
        
        if(currentState != MusicState.Boss){
            currentState = inBossFight ? MusicState.Boss : MusicState.Combat;
        }
    }
    
    public void NotifyBossfightFinished(){
        currentState = MusicState.Idle;
    }
}
