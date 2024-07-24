using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum AbilityType {
    None,
    PlayerDash,
    FishDash,
    PlayerSpikes,
    FishSpikes,
}

public class AbilityManagerComponent : MonoBehaviour {
    
    private static Dictionary<AbilityType, System.Type> AbilityLookup = new(){
        { AbilityType.PlayerDash,   typeof(PlayerDashAbilityComponent) },
        { AbilityType.FishDash,     typeof(FishDashAbilityComponent)},
        { AbilityType.PlayerSpikes, typeof(PlayerDashAbilityComponent) }, // WRONG
        { AbilityType.FishSpikes,   typeof(PlayerDashAbilityComponent) }, // WRONG
    };
    
    private Dictionary<AbilityType, AbilityComponent> abilities = new();
    
    void Update(){
        foreach(AbilityComponent abilityComponent in abilities.Values){
            if(abilityComponent.needsUpdate){
                abilityComponent.CustomUpdate();
            }
        }
    }
    
    public void CastAbility(AbilityType abilityType){
        if(!abilities.ContainsKey(abilityType)){
            abilities[abilityType] = GetComponent(AbilityLookup[abilityType]) as AbilityComponent;
        }
        
        abilities[abilityType].CastAbility();
    }
    
    public bool Casting(AbilityType abilityType){
        return !abilities[abilityType].needsUpdate;
    }
}