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
    Damaged,
    Dead,
}

public class EnemyFishAIComponent : MonoBehaviour {
    
    private readonly Vector3 NONZERO_VECTOR = new Vector3(0.0f, 0.0f, 0.001f);

    private const float PATROL_IDLE_TIME = 1.5f;
    private const float SPOTTED_ALERT_TIME = 1.0f;
    private const float PLAYER_ATTACK_RADIUS = 2.0f;
    private const float PURSUIT_UPDATE_TIME = 0.5f;
    private const float ATTACK_MAX_TIME = 1.5f;
    private const float DAMAGE_STUN_TIME = 1.5f;
    
    public const float DAMAGED_VELOCITY = 5.0f;
    
    [Header("Movement Attributes")]
    [Tooltip("Move speed while patrolling")]
    public float patrolSpeed;
    [Tooltip("Move speed while doing anything other than patrolling")]
    public float pursueSpeed;
    [Tooltip("Time it takes to get up to max speed")]
    public float accelerationTime;
    [Tooltip("Rate the fish turns when not moving")]
    public float idleRotationRate;
    [Tooltip("Rate the fish turns while moving")]
    public float movingRotationRate;
    
    [Header("Player Awakening Attributes")]
    [Tooltip("How far away to spot the player")]
    public float playerSpotDistance;
    [Tooltip("What angle along the horizontal that the fish can spot the player")]
    public float playerSpotAngle;
    [Tooltip("How far one side of the patrol should be")]
    public float patrolDistance;
    [Tooltip("How high up and down the patrol will randomly be")]
    public float patrolHeight;
    [Tooltip("UNUSED")]
    public float goBackToIdleDistance;
    [Header("State Attributes")]
    [Tooltip("What state to start the AI in")]
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
    [Tooltip("Attach Mesh component connection")]
    public DamageMeshComponent attackMesh;
    [Tooltip("how long from the start of an 'attack' the mesh should enabled")]
    public float attackStartDelay;
    [Tooltip("once the mesh is enabled, how long it stays enabled")]
    public float attackDuration;
    [Tooltip("range of randomly rolled damage dealt on an attack hit")]
    public Vector2 attackDamageRange;
    [Tooltip("type of damage applied on an attack hit")]
    public DamageType attackDamageType;
    
    [Header("Dying Attributes")]
    public GameObject deathEffectsPrefab;
    public AbilityType deathAbilityToGiveToPlayer;
    
    [Header("Ability Attributes")]
    [Tooltip("UNUSED")]
    [Range(0.0f, 1.0f)]
    public float specialAbilityChance;
    [Tooltip("UNUSED")]
    public GameObject attackSpecialAbility;
    [Header("Animation Connections")]
    public Transform modelRoot;
    public Animator modelAnimator;
    
    
    public enum PatrolState {
        IdleA, AtoB, IdleB, BtoA
    }
    
    [Header("DEBUG")]
    public FishAIState currentState;
    public PatrolState patrolState;
    
    private CharacterController character;
    private DamageableComponent damageable;
    private Vector3 originPosition;
    
    private Timer patrolTimer = new Timer(PATROL_IDLE_TIME);
    private Timer spotAlertTimer = new Timer(SPOTTED_ALERT_TIME);
    private Timer pursuitUpdateTimer = new Timer(PURSUIT_UPDATE_TIME);
    private Timer attackMaxTimer = new Timer(ATTACK_MAX_TIME);
    private Timer damageStunTimer = new Timer(DAMAGE_STUN_TIME);
    
    // Movement variables
    private Vector3 moveTarget;
    private float previousMoveDistance;
    private Vector3 velocity;
    private Vector3 acceleration;
    
    private Vector3 previousMoveVelocityRecorded = new Vector3(0.0f, 0.0f, 1.0f);
    private List<ImpartedVelocity> impartedVelocities = new();
    
    void Start(){
        originPosition = transform.position;
        character = GetComponent<CharacterController>();
        damageable = GetComponent<DamageableComponent>();
        
        damageable.damagedDelegates.Register(OnDamaged);
        damageable.killedDelegates.Register(OnKilled);
        
        currentState = startState;
    }
    
    void Update(){
        UpdateState();
        UpdateMovement();
        UpdateModel();
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
                    // TODO random chance rolls?
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
                // TODO random chance rolls?
                if(allowAttackAbilityPlayerState){
                    SetState(FishAIState.AttackAbility);
                } else if(allowAttackingPlayerState){
                    SetState(FishAIState.AttackingPlayer);
                }
            }
        } else if(currentState == FishAIState.AttackingPlayer){
            if(attackMaxTimer.Finished()){
                // TODO go into waiting mode if more than one attacking?
                if(allowPursuingPlayerState){
                    SetState(FishAIState.PursuingPlayer);
                } else if(allowPatrollingState){
                    SetState(FishAIState.Patrolling);
                }
            }
        } else if(currentState == FishAIState.AttackAbility){
        } else if(currentState == FishAIState.WaitingToAttackPlayer){
        } else if(currentState == FishAIState.Fleeing){
        } else if(currentState == FishAIState.Damaged){
            if(damageStunTimer.Finished()){
                if(allowPursuingPlayerState){
                    MoveTo(GetPlayerAttackReadyPosition());
                    SetState(FishAIState.PursuingPlayer);
                } else if(allowAttackAbilityPlayerState){
                    // TODO random chance rolls?
                    SetState(FishAIState.AttackAbility);
                } else if(allowAttackingPlayerState){
                    SetState(FishAIState.AttackingPlayer);
                }
            }
        } else if(currentState == FishAIState.Dead){
        
        }
    }
    
    private void SetState(FishAIState state){
        if(currentState == FishAIState.Dead){
            return;
        }
        
        if(state == FishAIState.Patrolling){
            patrolState = PatrolState.BtoA;
            StopMoving();
        } else if(state == FishAIState.SpottedPlayer){
            StopMoving();
            spotAlertTimer.Start();
            modelAnimator.SetTrigger("alert");
        } else if(state == FishAIState.PursuingPlayer){
            // Nop
        } else if(state == FishAIState.AttackingPlayer){
            // Move to slightly past player
            Vector3 toPlayer = PlayerComponent.player.transform.position - transform.position;
            MoveTo(transform.position + (toPlayer * 0.8f));
            
            // Snap instantly to 85% of the input direction
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer + NONZERO_VECTOR);
            modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, targetRotation, 0.85f);
            
            modelAnimator.SetTrigger("attack");
            
            if(attackMesh != null){
                attackMesh.CastDamageMesh(gameObject, attackStartDelay, attackDuration, attackDamageRange, attackDamageType);
            } else { 
                Debug.LogError("NO ATTACK MESH");
            }
            
            attackMaxTimer.Start();
        } else if(state == FishAIState.AttackAbility){
            StopMoving();
            // Animation ability attack
        } else if(state == FishAIState.WaitingToAttackPlayer){
            // Animation idle
        } else if(state == FishAIState.Fleeing){
            // Animation moving
        } else if(state == FishAIState.Damaged){
            StopMoving();
            damageStunTimer.Start();
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
        
        Debug.DrawLine(transform.position, moveTarget, Color.red, 0.0f, false);
        
        float speed = currentState == FishAIState.Patrolling ? patrolSpeed : pursueSpeed;
        velocity = Vector3.SmoothDamp(velocity, toTarget.normalized * speed, ref acceleration, accelerationTime);
        
        // Apply imparted velocities
        Vector3 actualVelocityToApply = velocity;
        for(int i = 0; i < impartedVelocities.Count; ++i){
            ImpartedVelocity v = impartedVelocities[i];
            actualVelocityToApply += v.velocity * (v.decreaseOverTime ? 1.0f - v.impartTimer.Parameterized() : 1.0f);
            
            if(v.impartTimer.Finished()){
                impartedVelocities[i] = impartedVelocities[impartedVelocities.Count - 1];
                impartedVelocities.RemoveAt(impartedVelocities.Count - 1);
                i--;
            }
        }
        
        character.Move(actualVelocityToApply * Time.deltaTime);
        
        // Always hard-clamp x
        Vector3 pos = transform.position;
        pos.x = 0.0f;
        transform.position = pos;
    }
    
    private void UpdateModel(){
        // TODO better look direction calculations; using the target if applicable
        if(velocity.magnitude < (patrolSpeed / 4.0f)){
            modelAnimator.SetBool("swimming", false);
            // previousMoveVelocityRecorded.y = 0.0f;

            Quaternion targetRotation = Quaternion.LookRotation(previousMoveVelocityRecorded + NONZERO_VECTOR);
            modelRoot.localRotation = Quaternion.RotateTowards(modelRoot.localRotation, targetRotation, idleRotationRate * Time.deltaTime);
        } else {
            modelAnimator.SetBool("swimming", true);
            
            Vector3 displayVelocity = velocity;
            displayVelocity.z *= displayVelocity.z * (displayVelocity.z < 0.0f ? -1.0f : 1.0f); // make the z movement more significant
            Quaternion targetRotation = Quaternion.LookRotation(displayVelocity + NONZERO_VECTOR);
            modelRoot.localRotation = Quaternion.RotateTowards(modelRoot.localRotation, targetRotation, movingRotationRate * Time.deltaTime);
            
            previousMoveVelocityRecorded = velocity;
        }
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
        previousMoveVelocityRecorded = position - transform.position;
    }
    
    private bool AtGoal(){
        Vector3 toTarget = moveTarget - transform.position;
        return toTarget.sqrMagnitude < 0.1f;
    }
    
    private void OnDamaged(DamageableComponent damage){
        SetState(FishAIState.Damaged);
        
        Vector3 fromDamager = transform.position - damageable.GetDamagerOrigin();
        ImpartVelocity(new ImpartedVelocity(fromDamager.normalized * DAMAGED_VELOCITY, 0.5f, true));
        
        PlayerComponent.player.SlowTime(0.6f);
        PlayerComponent.player.ImpartVelocity(new ImpartedVelocity(-fromDamager.normalized * PlayerComponent.DAMAGED_VELOCITY, 0.5f, true));
    }
    
    private void OnKilled(DamageableComponent damage){
        GameObject deathEffects = GameObject.Instantiate(deathEffectsPrefab);
        deathEffects.transform.position = transform.position;
        
        if(deathAbilityToGiveToPlayer != AbilityType.None){
            Debug.Log("TODO give the player " + deathAbilityToGiveToPlayer);
        }
        
        Destroy(gameObject);
    }
    
    public void ImpartVelocity(ImpartedVelocity v){
        impartedVelocities.Add(v);
        v.impartTimer.Start();
    }
}