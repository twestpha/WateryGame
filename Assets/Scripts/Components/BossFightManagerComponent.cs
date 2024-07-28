using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossFightManagerComponent : MonoBehaviour {
    
    private const bool ENABLE = true;
    private const bool DISABLE = false;
    private const bool ALL = true;
    
    private const int MAX_EYES = 4;
    
    public enum BossFightState {
        None,
        Intro,
        IdleStage0,
        IdleStage1,
        IdleStage2,
    }
    
    public BossFightState bossFightState;
    public bool injuredState;
    public Animator bossAnimator;
    [Space(10)]
    public MeshRenderer[] eyeballMeshes;
    public DamageableComponent[] eyeballDamageables;
    [Space(10)]
    public GameObject[] lightningTentacles;
    // randomly position and rotation, move inward over time offset
    // after timer, shock bzzzt effects
    // pull away
    
    public Animator lightAnimator;
    // Play animation, fade on
    // run a bunch of raycasts to all fish in some range? That kinda sucks...
    // Have volume keep track of who's in safe zones? maybe...
    
    public Collider playerHideSpots; // TODO
    
    
    // And also, have places for fish to spawn into level in a hidden way
    // use timer for this, spawn type randomly (health or dash?)
    
    private int remainingEyes;
    
    void Start(){
        for(int i = 0, count = eyeballDamageables.Length; i < count; ++i){
            eyeballDamageables[i].damagedDelegates.Register(OnEyeballDamaged);
            eyeballDamageables[i].killedDelegates.Register(OnEyeballKilled);
        }
        
        // TODO make a box trigger the intro and animation
        bossFightState = BossFightState.Intro;
        
        AbleEyeMeshes(0, DISABLE, ALL);
        AbleEyeDamageables(0, DISABLE, ALL);
    }
    
    void Update(){
        if(bossFightState == BossFightState.Intro){
            if(bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("bosscreature_idleback")){
                bossFightState = BossFightState.IdleStage0;
                
                remainingEyes = MAX_EYES;
                AbleEyeMeshes(0, ENABLE, ALL);
                AbleEyeDamageables(0, ENABLE, ALL);
            }
        }
    }
    
    private void AbleEyeMeshes(int index, bool able, bool all = false){
        for(int i = 0, count = eyeballMeshes.Length; i < count; ++i){
            if(i == index || all){
                eyeballMeshes[i].gameObject.SetActive(able);
            }
        }
        
        if(able && all){
            StartCoroutine(FlickerEyeMeshes());
        }
    }
    
    private IEnumerator FlickerEyeMeshes(){
        // Look this ain't great code but here we are
        Timer blinkTimer = new Timer(0.1f);
        blinkTimer.Start();
        while(!blinkTimer.Finished()){ yield return null; }
        
        for(int i = 0, count = eyeballMeshes.Length; i < count; ++i){
            eyeballMeshes[i].gameObject.SetActive(false);
        }
        
        blinkTimer = new Timer(0.1f);
        blinkTimer.Start();
        while(!blinkTimer.Finished()){ yield return null; }
        
        for(int i = 0, count = eyeballMeshes.Length; i < count; ++i){
            eyeballMeshes[i].gameObject.SetActive(true);
        }
    }
    
    private void AbleEyeDamageables(int index, bool able, bool all = false){
        for(int i = 0, count = eyeballDamageables.Length; i < count; ++i){
            if(i == index || all){
                eyeballDamageables[i].gameObject.SetActive(able);
            }
        }
    }
    
    private void RespawnAllEyeDamageables(){
        for(int i = 0, count = eyeballDamageables.Length; i < count; ++i){
            eyeballDamageables[i].Respawn();
        }
    }
    
    private void OnEyeballDamaged(DamageableComponent damagedEyeball){
        PlayerComponent.player.SlowTime(0.6f);
    }
    
    private void OnEyeballKilled(DamageableComponent damagedEyeball){    
        for(int i = 0, count = eyeballDamageables.Length; i < count; ++i){
            if(damagedEyeball == eyeballDamageables[i]){
                eyeballMeshes[i].gameObject.SetActive(false);
                eyeballDamageables[i].gameObject.SetActive(false);
                break;
            }
        }
        
        bossAnimator.SetTrigger("smallinjured");
        
        remainingEyes--;
        if(remainingEyes == 0){
            Debug.Log("STUFF!");
        }
    }
}