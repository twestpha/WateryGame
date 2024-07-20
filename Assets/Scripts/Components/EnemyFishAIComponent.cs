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
    private const float SPOTTED_ALERT_TIME = 1.0f;
    private const float PLAYER_ATTACK_RADIUS = 2.0f;
    private const float PURSUIT_UPDATE_TIME = 0.5f;
    
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
    
    private Timer patrolTimer = new Timer(PATROL_IDLE_TIME);
    private Timer spotAlertTimer = new Timer(SPOTTED_ALERT_TIME);
    private Timer pursuitUpdateTimer = new Timer(PURSUIT_UPDATE_TIME);
    
    // Movement variables
    private Vector3 moveTarget;
    private float previousMoveDistance;
    private Vector3 velocity;
    private Vector3 acceleration;
    
    private Vector3 previousMoveVelocityRecorded = new Vector3(0.0f, 0.0f, 1.0f);
    
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
                    SetState(FishAIState.SpottedPlayer);
                } else if(allowPursuingPlayerState){
                    SetState(FishAIState.PursuingPlayer);
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
                    SetState(FishAIState.SpottedPlayer);
                } else if(allowPursuingPlayerState){
                    SetState(FishAIState.PursuingPlayer);
                }
            }
        } else if(currentState == FishAIState.SpottedPlayer){
            if(spotAlertTimer.Finished()){
                if(allowPursuingPlayerState){
                    MoveTo(GetPlayerAttackReadyPosition());
                    SetState(FishAIState.PursuingPlayer);
                } else if(allowAttackAbilityPlayerState){
                    SetState(FishAIState.AttackAbility);
                } else if(allowAttackingPlayerState){
                    SetState(FishAIState.AttackingPlayer);
                }
            }
        } else if(currentState == FishAIState.PursuingPlayer){
            if(!AtGoal()){
                if(pursuitUpdateTimer.Finished()){
                    MoveTo(GetPlayerAttackReadyPosition());
                }
            } else {
                if(allowAttackAbilityPlayerState){
                    SetState(FishAIState.AttackAbility);
                } else if(allowAttackingPlayerState){
                    SetState(FishAIState.AttackingPlayer);
                }
            }
        } else if(currentState == FishAIState.AttackAbility){
            // blep
        }
    }
    
    private void SetState(FishAIState state){
        if(currentState == FishAIState.Dead){
            return;
        }
        
        if(state == FishAIState.Patrolling){
            patrolState = PatrolState.BtoA;
            StopMoving();
            // Animation idle
        } else if(state == FishAIState.SpottedPlayer){
            StopMoving();
            spotAlertTimer.Start();
            // Animation alert
        } else if(state == FishAIState.PursuingPlayer){
            // Animation moving
        } else if(state == FishAIState.AttackingPlayer){
            StopMoving();
            // Animation attack
        } else if(state == FishAIState.AttackAbility){
            StopMoving();
            // Animation ability attack
        } else if(state == FishAIState.WaitingToAttackPlayer){
            // Animation idle
        } else if(state == FishAIState.Fleeing){
            // Animation moving
        } else if(state == FishAIState.Dead){
            StopMoving();
            // Animation death
        }
        
        // Debug.Log(gameObject + ": " + currentState + "-->" + state);
        currentState = state;
    }
    
    private Vector3 GetPlayerAttackReadyPosition(){
        // TODO add in some sin(time) to change the heights
        Vector3 fromPlayer = transform.position - PlayerComponent.player.transform.position;
        Vector3 target = PlayerComponent.player.transform.position + (fromPlayer.normalized * PLAYER_ATTACK_RADIUS);
        return target;
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
        
        if(velocity.magnitude >= (moveSpeed / 4.0f)){
            previousMoveVelocityRecorded = velocity;
        }
        
        // TODO apply previousMoveVelocityRecorded to model root transform
        
        // Always hard-clamp x
        Vector3 pos = transform.position;
        pos.x = 0.0f;
        transform.position = pos;
    }
    
    private bool CanSeePlayer(){
        Vector3 toPlayer = PlayerComponent.player.transform.position - transform.position;
        
        if(toPlayer.magnitude < playerSpotDistance){
            Vector3 lookDirection = velocity;
            lookDirection.x = 0.0f;
            
            if(Vector3.Angle(toPlayer, lookDirection) < playerSpotAngle){
                if(!Physics.Raycast(transform.position, toPlayer, toPlayer.magnitude, 1 << 0 /* default only */, QueryTriggerInteraction.Ignore)){
                    // Debug.DrawLine(transform.position, PlayerComponent.player.transform.position, Color.green, 0.0f, false);
                    return true;
                }
            }
        }
        
        // Debug.DrawLine(transform.position, PlayerComponent.player.transform.position, Color.red, 0.0f, false);
        return false;
    }
    
    private void StopMoving(){
        MoveTo(transform.position);
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