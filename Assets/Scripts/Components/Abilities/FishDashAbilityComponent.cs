using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FishDashAbilityComponent : AbilityComponent {
    
    public float dashTime;
    public float dashDistance;
    public DamageMeshComponent damageMesh;
    
    public override void CastAbility(){
        Debug.Log("BUTTS!");
        needsUpdate = true;
    }
    
    public override void CustomUpdate(){
        
    }
}