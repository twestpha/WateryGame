using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FishDashAbilityComponent : AbilityComponent {
    
    public float dashTime;
    public float dashDistance;
    public DamageMeshComponent damageMesh;
    public Vector2 dashDamageRange;
    public AudioSource abilitySound;
    
    private Vector3 cachedDirection;
    
    private EnemyFishAIComponent fish;
    
    private Timer windupTimer = new Timer(1.0f);
    private Timer dashTimer;
    
    private enum AbilityState {
        WindingUp,
        Dashing,
    }
    
    private AbilityState state;
    
    public override void CastAbility(){
        fish = GetComponent<EnemyFishAIComponent>();
        
        dashTimer = new Timer(dashTime);
        
        needsUpdate = true;
        cachedDirection = fish.GetPreviousMoveVelocity().normalized;
        
        state = AbilityState.WindingUp;
        fish.modelAnimator.SetTrigger("abilityattack");
        abilitySound.Play();
        
        windupTimer.Start();
    }
    
    public override void CustomUpdate(){
        if(state == AbilityState.WindingUp && windupTimer.Finished()){
            state = AbilityState.Dashing;
            dashTimer.Start();
            
            fish.ImpartVelocity(new ImpartedVelocity(cachedDirection * (dashDistance / dashTime), dashTime, false));
            damageMesh.CastDamageMesh(gameObject, 0.0f, dashTime, dashDamageRange, DamageType.DashFishAttack);
        } else if(state == AbilityState.Dashing){
            fish.StopMoving();
            
            if(dashTimer.Finished()){
                needsUpdate = false;
                fish.modelAnimator.SetBool("swimming", false);
            }
        }
    }
}