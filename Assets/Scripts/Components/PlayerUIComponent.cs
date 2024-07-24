using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class PlayerUIComponent : MonoBehaviour {
    
    public Text armorText;
    public Text healthText;
    public Text abilityText;
    
    private PlayerComponent player;
    
    void Start(){
        player = PlayerComponent.player;
    }
    
    void Update(){
        armorText.text = "Armor: " + (player.Damageable.hasArmor);
        healthText.text = "HP: " + player.Damageable.CurrentHealth() + "/" + player.Damageable.maxHealth;
        abilityText.text = player.CurrentAbility + ": " + player.AbilityTimer.Remaining().ToString("0.00");
    }
}