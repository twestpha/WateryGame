using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerSpikeAbilityComponent : AbilityComponent {
    
    public float windupTime;
    public float holdSpikesTime;
    public float cooldownTime;
    public DamageMeshComponent damageMesh;
    public Vector2 spikeDamageRange;
    public AudioSource abilitySound;
    
    private PlayerComponent player;
    
    private bool setup;
    private Timer windupTimer;
    private Timer holdSpikesTimer;
    private Timer cooldownTimer;
    
    private enum AbilityState {
        WindingUp,
        Spiking,
    }
    
    private AbilityState state;
    
    public override void CastAbility(){
        if(!setup){
            setup = true;
            
            windupTimer = new Timer(windupTime);
            holdSpikesTimer = new Timer(holdSpikesTime);
            cooldownTimer = new Timer(cooldownTime);
            cooldownTimer.SetParameterized(1.0f);
        }
        
        if(!cooldownTimer.Finished()){
            return;
        }
        
        player = GetComponent<PlayerComponent>();
        
        needsUpdate = true;
        
        state = AbilityState.WindingUp;
        player.modelAnimator.SetTrigger("abilityspikes");
        
        windupTimer.Start();
        abilitySound.Play();
    }
    
    public override void CustomUpdate(){
        if(state == AbilityState.WindingUp && windupTimer.Finished()){
            state = AbilityState.Spiking;
            holdSpikesTimer.Start();
            
            damageMesh.CastDamageMesh(gameObject, 0.0f, holdSpikesTime, spikeDamageRange, DamageType.SpikeFishAttack);
        } else if(state == AbilityState.Spiking){
            if(holdSpikesTimer.Finished()){
                cooldownTimer.Start();
                needsUpdate = false;
            }
        }
    }
}