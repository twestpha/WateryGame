using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossFightManagerComponent : MonoBehaviour {
    
    private const bool ENABLE = true;
    private const bool DISABLE = false;
    private const bool ALL = true;
    
    private const int MAX_EYES = 4;
    private const int MAX_COMBAT_ROUNDS = 3;
    
    private const int TENTACLES_ROUND_0 = 4;
    private const int TENTACLES_ROUND_1 = 8;
    private const int TENTACLES_ROUND_2 = 12;
    
    private readonly Vector2 TENTACLE_COOLDOWN = new Vector2(2.0f, 5.0f);
    
    public enum BossFightState {
        None,
        Intro,
        IdleTentacleCooldown,
        IdleTentacleExtend,
        IdleTentacleLightning,
        InjuredBetweenStages,
        Leaving,
    }
    
    public BossFightState bossFightState;
    public bool injuredState;
    public Animator bossAnimator;
    [Space(10)]
    public MeshRenderer[] eyeballMeshes;
    public DamageableComponent[] eyeballDamageables;
    [Space(10)]
    public BossFightTentacleComponent[] lightningTentacles;
    
    // public Animator lightAnimator;
    // public Collider playerHideSpots; // TODO
    
    // And also, have places for fish to spawn into level in a hidden way?
    // use timer for this, spawn type randomly (health or dash?)
    
    private Timer tentacleCooldownTimer = new Timer();
    private Timer tentacleExtendTimer = new Timer(6.5f);
    private Timer tentacleLightningTimer = new Timer(6.5f);
    
    private int remainingEyes;
    private int combatRound;
    
    void Start(){
        for(int i = 0, count = eyeballDamageables.Length; i < count; ++i){
            eyeballDamageables[i].damagedDelegates.Register(OnEyeballDamaged);
            eyeballDamageables[i].killedDelegates.Register(OnEyeballKilled);
        }
        
        // TODO make a box trigger the intro and animation
        bossFightState = BossFightState.Intro;
        AudioManager.instance.NotifyOfCombat(true);
        
        AbleEyeMeshes(0, DISABLE, ALL);
        AbleEyeDamageables(0, DISABLE, ALL);
    }
    
    void Update(){
        if(bossFightState == BossFightState.Intro){
            if(bossAnimator.GetCurrentAnimatorStateInfo(0).IsName("bosscreature_idleback")){
                
                bossFightState = BossFightState.IdleTentacleCooldown;
                
                tentacleCooldownTimer.SetDuration(UnityEngine.Random.Range(TENTACLE_COOLDOWN.x, TENTACLE_COOLDOWN.y));
                tentacleCooldownTimer.Start();
                
                remainingEyes = MAX_EYES;
                AbleEyeMeshes(0, ENABLE, ALL);
                AbleEyeDamageables(0, ENABLE, ALL);
            }            
        } else if(bossFightState == BossFightState.IdleTentacleCooldown){
            if(tentacleCooldownTimer.Finished()){
                for(int i = 0, count = GetTentacleCount(); i < count; ++i){
                    lightningTentacles[i].Extend();
                }
                
                bossFightState = BossFightState.IdleTentacleExtend;
                tentacleExtendTimer.Start();
            }
        } else if(bossFightState == BossFightState.IdleTentacleExtend){
            if(tentacleExtendTimer.Finished()){
                for(int i = 0, count = GetTentacleCount(); i < count; ++i){
                    lightningTentacles[i].Electrify();
                }
                
                bossFightState = BossFightState.IdleTentacleLightning;
                tentacleLightningTimer.Start();
            }
        } else if(bossFightState == BossFightState.IdleTentacleLightning){
            if(tentacleLightningTimer.Finished()){
                bossFightState = BossFightState.IdleTentacleCooldown;
                
                tentacleCooldownTimer.SetDuration(UnityEngine.Random.Range(TENTACLE_COOLDOWN.x, TENTACLE_COOLDOWN.y));
                tentacleCooldownTimer.Start();
            }
        } else if(bossFightState == BossFightState.InjuredBetweenStages){
            // ???
        }
    }
    
    private int GetTentacleCount(){
        if(combatRound == 0){
            return TENTACLES_ROUND_0;
        } else if(combatRound == 1){
            return TENTACLES_ROUND_1;
        } else if(combatRound == 2){
            return TENTACLES_ROUND_2;
        }
        return -1;
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
        remainingEyes--;
        
        for(int i = 0, count = eyeballDamageables.Length; i < count; ++i){
            if(damagedEyeball == eyeballDamageables[i]){
                eyeballMeshes[i].gameObject.SetActive(false);
                eyeballDamageables[i].gameObject.SetActive(false);
                break;
            }
        }
        
        if(remainingEyes == 0){
            // Retract all
            for(int i = 0, count = lightningTentacles.Length; i < count; ++i){
                lightningTentacles[i].Retract();
            }
            
            combatRound++;
            remainingEyes = MAX_EYES;
            
            if(combatRound == MAX_COMBAT_ROUNDS){
                Debug.Log("FIGHT OVER");
                AudioManager.instance.NotifyBossfightFinished();
                // Fade out to static art screen?
            } else {
                bossFightState = BossFightState.InjuredBetweenStages;
                bossAnimator.SetTrigger("biginjured");
            }
        } else {
            bossAnimator.SetTrigger("smallinjured");
        }
    }
}