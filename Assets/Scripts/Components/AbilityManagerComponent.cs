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
    PlayerBubble,
    FishBubble,
}

public class AbilityManagerComponent : MonoBehaviour {
    
    private static Dictionary<AbilityType, System.Type> AbilityLookup = new(){
        { AbilityType.PlayerDash,   typeof(PlayerDashAbilityComponent) },
        { AbilityType.FishDash,     typeof(FishDashAbilityComponent)   },
        { AbilityType.PlayerSpikes, typeof(PlayerDashAbilityComponent) }, // WRONG
        { AbilityType.FishSpikes,   typeof(FishSpikeAbilityComponent)  },
        { AbilityType.PlayerBubble, typeof(PlayerDashAbilityComponent) }, // WRONG
        { AbilityType.FishBubble,   typeof(PlayerDashAbilityComponent) }, // WRONG
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
        if(!Casting(abilityType)){
            if(!abilities.ContainsKey(abilityType)){
                abilities[abilityType] = GetComponent(AbilityLookup[abilityType]) as AbilityComponent;
            }
            
            abilities[abilityType].CastAbility();
        }
    }
    
    public bool Casting(AbilityType abilityType){
        return abilities.ContainsKey(abilityType) ? abilities[abilityType].needsUpdate : false;
    }
}