using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class PlayerUIComponent : MonoBehaviour {
    
    public static PlayerUIComponent instance;
    
    private const float TEXT_DURATION = 5.0f;
    private const float FADE_DURATION = 1.0f;
    
    public Text dialogueText;
    public Text shadowText;
    
    [Space(10)]
    public Image fade;
    
    [Space(10)]
    public Image abilityFill;
    public Image[] healthTines;
    public Image armorOverlay;
    
    [Space(10)]
    public Sprite dashAbilitySprite;
    public Sprite spikeAbilitySprite;
    
    private PlayerComponent player;
    
    private Timer textTimer = new Timer(TEXT_DURATION);
    private Timer textFadeTimer = new Timer(FADE_DURATION);
    
    private bool lockUI;
    
    public enum TextState {
        None,
        FadingIn,
        Idle,
        FadingOut,
    }
    
    public TextState textState;
    
    void Awake(){
        instance = this;
    }
    
    void Start(){
        player = PlayerComponent.player;
    }
    
    void Update(){
        for(int i = 0, count = healthTines.Length; i < count; ++i){
            healthTines[i].enabled = i < player.Damageable.CurrentHealth();
        }
        
        armorOverlay.enabled = player.Damageable.hasArmor;
        
        Sprite targetSprite = null;
        if(player.CurrentAbility == AbilityType.PlayerDash){
            targetSprite = dashAbilitySprite;
        } else if(player.CurrentAbility == AbilityType.PlayerSpikes){
            targetSprite = spikeAbilitySprite;
        }        
        
        abilityFill.enabled = player.CurrentAbility != AbilityType.None;
        abilityFill.sprite = targetSprite;
        abilityFill.fillAmount = (1.0f - player.AbilityTimer.Parameterized());
        
        if(textState == TextState.FadingIn){
            Color prev = dialogueText.color;
            prev.a = textFadeTimer.Parameterized();
            dialogueText.color = prev;
            
            prev = shadowText.color;
            prev.a = textFadeTimer.Parameterized();
            shadowText.color = prev;
            
            if(textFadeTimer.Finished()){
                textState = TextState.Idle;
                textTimer.Start();
            }
        } else if(textState == TextState.Idle){
            if(textTimer.Finished() && !lockUI){
                textState = TextState.FadingOut;
                textFadeTimer.Start();
            }
        } else if(textState == TextState.FadingOut){
            Color prev = dialogueText.color;
            prev.a = 1.0f - textFadeTimer.Parameterized();
            dialogueText.color = prev;
            
            prev = shadowText.color;
            prev.a = 1.0f - textFadeTimer.Parameterized();
            shadowText.color = prev;
            
            if(textFadeTimer.Finished()){
                textState = TextState.None;
            }
        }
    }
    
    [ContextMenu("Test Dialogue")]
    public void TestDialogue(){
        ShowDialogue("Test Words Blah Blah Blah");
    }
    
    public void ShowDialogue(string text, bool neverClear = false){
        if(!lockUI){
            dialogueText.text = text;
            shadowText.text = text;
            
            textState = TextState.FadingIn;
            textFadeTimer.Start();
        }
        
        if(neverClear){
            lockUI = true;
        }
    }
    
    public void FadeInOutForRespawn(){
        StartCoroutine(FadeInOutForRespawnCoroutine());
    }
    
    private const float FADE_TIME = 1.0f;
    private IEnumerator FadeInOutForRespawnCoroutine(){
        IndependentTimer fadeTimer = new IndependentTimer(FADE_TIME);
        
        fadeTimer.Start();
        while(!fadeTimer.Finished()){
            fade.color = new Color(0.0f, 0.0f, 0.0f, fadeTimer.Parameterized());
            yield return null;
        }
        fade.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        
        PlayerComponent.player.FinalizeRespawn();
        
        fadeTimer.Start();
        while(!fadeTimer.Finished()){
            fade.color = new Color(0.0f, 0.0f, 0.0f, 1.0f - fadeTimer.Parameterized());
            yield return null;
        }
        fade.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }
}