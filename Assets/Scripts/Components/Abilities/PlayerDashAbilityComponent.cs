using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerDashAbilityComponent : AbilityComponent {
    
    public float windupTime;
    public float dashTime;
    public float dashDistance;
    public DamageMeshComponent damageMesh;
    public Vector2 dashDamageRange;
    public AudioSource abilitySound;
    
    private Vector3 cachedDirection;
    
    private PlayerComponent player;
    
    private Timer windupTimer;
    private Timer dashTimer;
    
    private enum AbilityState {
        WindingUp,
        Dashing,
    }

    private AbilityState state;
    
    public override void CastAbility(){
        player = GetComponent<PlayerComponent>();
        
        windupTimer = new Timer(windupTime);
        dashTimer = new Timer(dashTime);
        
        needsUpdate = true;
        cachedDirection = player.GetPreviousMoveVelocity().normalized;
        
        state = AbilityState.WindingUp;
        player.modelAnimator.SetTrigger("abilitydash");
        player.movementInputsEnabled = false;
        player.gameObject.layer = 9 /* player no collide */;
        player.Damageable.SetInvincible(true);
        
        abilitySound.Play();
        
        windupTimer.Start();
    }
    
    public override void CustomUpdate(){
        // This aint ideal though
        Quaternion targetRotation = Quaternion.LookRotation(cachedDirection);
        player.modelRoot.localRotation = Quaternion.RotateTowards(player.modelRoot.localRotation, targetRotation, 360.0f * Time.deltaTime); 
        
        if(state == AbilityState.WindingUp && windupTimer.Finished()){
            state = AbilityState.Dashing;
            dashTimer.Start();
            
            player.ImpartVelocity(new ImpartedVelocity(cachedDirection * (dashDistance / dashTime), dashTime, false));
            damageMesh.CastDamageMesh(gameObject, 0.0f, dashTime, dashDamageRange, DamageType.DashFishAttack);
        } else if(state == AbilityState.Dashing){
            if(dashTimer.Finished()){
                player.movementInputsEnabled = true;
                needsUpdate = false;
                player.Damageable.SetInvincible(false);
                player.gameObject.layer = 6 /* player */;
                player.modelAnimator.SetBool("swimming", false);
            }
        }
    }
}