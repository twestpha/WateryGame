using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerDashAbilityComponent : AbilityComponent {
    
    public float dashTime;
    public float dashDistance;
    public DamageMeshComponent damageMesh;
    
    public override void CastAbility(){
        Debug.Log("PLAYER DASH!");
        needsUpdate = true;
    }
    
    public override void CustomUpdate(){
        
    }
}