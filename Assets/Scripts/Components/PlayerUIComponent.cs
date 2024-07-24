using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class PlayerUIComponent : MonoBehaviour {
    
    public Text armorText;
    public Text healthText;
    
    public Image abilityFill;
    
    public Sprite dashAbilitySprite;
    
    private PlayerComponent player;
    
    void Start(){
        player = PlayerComponent.player;
    }
    
    void Update(){
        armorText.text = "Armor: " + (player.Damageable.hasArmor);
        healthText.text = "HP: " + player.Damageable.CurrentHealth() + "/" + player.Damageable.maxHealth;
        
        Sprite targetSprite = null;
        if(player.CurrentAbility == AbilityType.PlayerDash){
            targetSprite = dashAbilitySprite;
        }
        
        abilityFill.enabled = player.CurrentAbility != AbilityType.None;
        abilityFill.sprite = targetSprite;
        abilityFill.fillAmount = (1.0f - player.AbilityTimer.Parameterized());
    }
}