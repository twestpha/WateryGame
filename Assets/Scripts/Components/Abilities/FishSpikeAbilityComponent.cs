using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FishSpikeAbilityComponent : AbilityComponent {
    
    public float windupTime;
    public float holdSpikesTime;
    public DamageMeshComponent damageMesh;
    public Vector2 spikeDamageRange;
    public AudioSource abilitySound;
    
    private EnemyFishAIComponent fish;
    
    private Timer windupTimer;
    private Timer holdSpikesTimer;
    
    private enum AbilityState {
        WindingUp,
        Spiking,
    }
    
    private AbilityState state;
    
    public override void CastAbility(){
        fish = GetComponent<EnemyFishAIComponent>();
        
        windupTimer = new Timer(windupTime);
        holdSpikesTimer = new Timer(holdSpikesTime);
        
        needsUpdate = true;
        
        state = AbilityState.WindingUp;
        fish.modelAnimator.SetTrigger("abilityattack");
        
        windupTimer.Start();
        fish.StopMoving();
        
        abilitySound.Play();
    }
    
    public override void CustomUpdate(){
        if(state == AbilityState.WindingUp && windupTimer.Finished()){
            state = AbilityState.Spiking;
            holdSpikesTimer.Start();
            
            damageMesh.CastDamageMesh(gameObject, 0.0f, holdSpikesTime, spikeDamageRange, DamageType.SpikeFishAttack);
        } else if(state == AbilityState.Spiking){
            if(holdSpikesTimer.Finished()){
                needsUpdate = false;
            }
        }
    }
}