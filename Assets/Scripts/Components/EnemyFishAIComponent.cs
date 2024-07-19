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
    AttackAbility,
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
    public float goBackToIdleDistance;
    [Header("State Attributes")]
    public FishAIState startState;
    [Space(10)]
    public bool allowMotionlessState = true;
    public bool allowPatrollingState = true;
    public bool allowSpottedPlayerState = true;
    public bool allowPursuingPlayerState = true;
    public bool allowAttackingPlayerState = true;
    public bool allowAttackAbilityPlayerState = true;
    public bool allowWaitingToAttackPlayerState = true;
    public bool allowFleeingState = true;
    public bool allowDeadState = true;
    [Header("Attack Attributes")]
    public GameObject attackMesh;
    public float attackStartDelay;
    public float attackDuration;
    public Vector2 attackDamageRange;
    public DamageType attackDamageType;
    [Header("Ability Attributes")]
    [Range(0.0f, 1.0f)]
    public float specialAbilityChance;
    public GameObject attackSpecialAbility;
    [Header("Animation Attributes")]
    public string idleAnimationName;
    public string movingAnimationName;
    public string spottedAnimationName;
    public string attackAnimationName;
    public string specialAbilityAnimationName;
    public string dyingAnimationName;
    
    public enum PatrolState {
        IdleA, AtoB, IdleB, BtoA
    }
    
    [Header("DEBUG")]
    public FishAIState currentState;
    public PatrolState patrolState;
    
    private CharacterController character;
    private Vector3 originPosition;
    
    // Patrol variables
    private Timer patrolTimer = new Timer(PATROL_IDLE_TIME);
    
    // Movement variables
    private Vector3 moveTarget;
    private float previousMoveDistance;
    private Vector3 velocity;
    private Vector3 acceleration;
    
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
            if(patrolState == PatrolState.BtoA){
                if(AtGoal()){
                    patrolTimer.Start();
                    patrolState = PatrolState.IdleA;
                }
            } else if(patrolState == PatrolState.IdleA){
                if(patrolTimer.Finished()){
                    Vector3 moveTarget = originPosition
                                         + (Vector3.forward * patrolDistance) 
                                         + (Vector3.up * UnityEngine.Random.Range(-patrolHeight, patrolHeight));
                    MoveTo(moveTarget);
                    patrolState = PatrolState.AtoB;
                }
            } else if(patrolState == PatrolState.AtoB){
                if(AtGoal()){
                    patrolTimer.Start();
                    patrolState = PatrolState.IdleB;
                }
            } else if(patrolState == PatrolState.IdleB){
                
                if(patrolTimer.Finished()){
                    Vector3 moveTarget = originPosition
                                         - (Vector3.forward * patrolDistance) 
                                         + (Vector3.up * UnityEngine.Random.Range(-patrolHeight, patrolHeight));
                    MoveTo(moveTarget);
                    patrolState = PatrolState.BtoA;
                }
            }
            
            if(CanSeePlayer()){
                if(allowSpottedPlayerState){
                    currentState = FishAIState.SpottedPlayer;
                }
                if(allowPursuingPlayerState){
                    currentState = FishAIState.PursuingPlayer;
                }
            }
        } else if(currentState == FishAIState.SpottedPlayer){
            // Pause for a moment, playing a "!" animation
        } else if(currentState == FishAIState.PursuingPlayer){
            // Move towards player
        }
    }
    
    private void UpdateMovement(){
        Vector3 toTarget = moveTarget - transform.position;
        float targetDistance = toTarget.magnitude;
        
        if(previousMoveDistance < targetDistance){
            moveTarget = transform.position;
        }
        previousMoveDistance = targetDistance;
        
        // Debug.DrawLine(transform.position, moveTarget, Color.red, 0.0f, false);
        
        velocity = Vector3.SmoothDamp(velocity, toTarget.normalized * moveSpeed, ref acceleration, accelerationTime);
        character.Move(velocity * Time.deltaTime);
        
        // Always hard-clamp x
        Vector3 pos = transform.position;
        pos.x = 0.0f;
        transform.position = pos;
    }
    
    private bool CanSeePlayer(){
        return false;
    }
    
    private void MoveTo(Vector3 position){
        moveTarget = position;
        previousMoveDistance = 99999999.0f;
    }
    
    private bool AtGoal(){
        Vector3 toTarget = moveTarget - transform.position;
        return toTarget.sqrMagnitude < 0.1f;
    }
}