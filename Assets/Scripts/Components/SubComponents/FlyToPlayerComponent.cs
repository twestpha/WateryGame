using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FlyToPlayerComponent : MonoBehaviour {
    
    private const float DURATION = 1.2f;
    private const float INITIAL_SPEED = 20.0f;
    
    public float healthToGive;
    public float armorToGive;
    public float lightToGive;
    public AbilityType abilityToGive;
    
    private Vector3 initialVelocity;
    private Vector3 velocity;
    
    private IndependentTimer durationTimer = new IndependentTimer(DURATION);
    
    void Start(){
        Vector3 toPlayer = PlayerComponent.player.transform.position - transform.position;
        initialVelocity = Vector3.Cross(toPlayer, Vector3.right).normalized;
        initialVelocity *= UnityEngine.Random.value < 0.5f ? 1.0f : -1.0f;
        initialVelocity = (initialVelocity - toPlayer.normalized).normalized * INITIAL_SPEED;
                
        durationTimer.Start();
    }
    
    void Update(){
        transform.position += initialVelocity * Time.unscaledDeltaTime;
        
        float t = durationTimer.Parameterized();
        // t *= t;
        transform.position = Vector3.Lerp(transform.position, PlayerComponent.player.transform.position, t);
        
        // clamp x yay
        Vector3 pos = transform.position;
        pos.x = 0.0f;
        transform.position = pos;
        
        if(durationTimer.Finished()){
            // Give shit to player!
            Destroy(gameObject);
        }
    }
}