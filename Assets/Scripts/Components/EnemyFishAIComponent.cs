using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum FishAIState {
    Motionless,
    Patrolling,
    SpottedPlayer,
    PursuingPlayer,
    AttackingPlayer,
    WaitingToAttackPlayer,
    Fleeing,
    Dead,
}

public class EnemyFishAIComponent : MonoBehaviour {
    
    private const float PATROL_IDLE_TIME = 1.5f;
    
    [Header("Movement Attributes")]
    public float moveSpeed;
    public float accelerationTime;
    [Header("Player Awakening Attributes")]
    public float playerSpotDistance;
    public float playerSpotAngle;
    public float patrolDistance;
    public float patrolHeight;
    [Header("State Attributes")]
    public FishAIState startState;
    public FishAIState currentState;
    [Space(10)]
    public bool allowMotionlessState = true;
    public bool allowPatrollingState = true;
    public bool allowSpottedPlayerState = true;
    public bool allowPursuingPlayerState = true;
    public bool allowAttackingPlayerState = true;
    public bool allowWaitingToAttackPlayerState = true;
    public bool allowFleeingState = true;
    public bool allowDeadState = true;
    [Header("Attack Attributes")]
    public GameObject attackMesh;
    public float attackStartDelay;
    public float attackDuration;
    public Vector2 attackDamageRange;
    public DamageType attackDamageType;
    
    public enum PatrolState {
        IdleA, AtoB, IdleB, BtoA
    }
    
    private CharacterController character;
    private Vector3 originPosition;
    
    private PatrolState patrolState;
    private Timer patrolTimer = new Timer(PATROL_IDLE_TIME);
    
    void Start(){
        originPosition = transform.position;
        character = GetComponent<CharacterController>();
        
        currentState = startState;
    }
    
    void Update(){
        UpdateState();
        UpdateMovement();
    }
    
    private void UpdateState(){
        if(currentState == FishAIState.Motionless){
            if(CanSeePlayer()){
                if(allowSpottedPlayerState){
                    currentState = FishAIState.SpottedPlayer;
                }
                if(allowPursuingPlayerState){
                    currentState = FishAIState.PursuingPlayer;
                }
            }
        } else if(currentState == FishAIState.Patrolling){
            // patrol logic
            
            // patrolTimer
            // PatrolState
            // IdleA, AtoB, IdleB, BtoA
            
            if(CanSeePlayer()){
                if(allowSpottedPlayerState){
                    currentState = FishAIState.SpottedPlayer;
                }
                if(allowPursuingPlayerState){
                    currentState = FishAIState.PursuingPlayer;
                }
            }
        }
    }
    
    private void UpdateMovement(){
        
    }
    
    private bool CanSeePlayer(){
        return false;
    }
    
    private void MoveTo(Vector3 position){
        
    }
}