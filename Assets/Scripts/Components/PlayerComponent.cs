using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ImpartedVelocity {
    public Vector3 velocity;
    public Timer impartTimer;
    public bool decreaseOverTime;
    
    public ImpartedVelocity(Vector3 velocity_, float time, bool decreaseOverTime_)
    {
        velocity = velocity_;
        impartTimer = new Timer(time);
        decreaseOverTime = decreaseOverTime_;
    }
}

public class PlayerComponent : MonoBehaviour {
    
    private readonly Vector3 NONZERO_VECTOR = new Vector3(0.0f, 0.0f, 0.001f);
    public const float DAMAGED_VELOCITY = 5.0f;
    
    public static PlayerComponent player;

    [Header("Movement Attributes")]
    [Tooltip("Max speed directionally the character can move")]
    public float moveSpeed = 0.1f;
    [Tooltip("How much time it takes to reach moveSpeed when a button is pressed")]
    public float accelerationTime = 0.5f;
    [Tooltip("How much time it takes to reach moveSpeed when no buttons are pressed")]
    public float decelerationTime = 1.0f;
    [Tooltip("The character's downward velocity from gravity")]
    public float downVelocity = 1.0f;
    [Tooltip("How long after the player presses buttons until we apply the downVelocity")]
    public float downVelocityApplyTime;
    
    [Header("Model Rotation Attributes")]
    [Tooltip("How fast (in degrees per second) the model root returns to the idle state")]
    public float idleRotationRate;
    [Tooltip("How fast (in degrees per second) the model root moves toward the velocity while the character is moving")]
    public float movingRotationRate;
    
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
    
    [Header("Connections")]
    public Transform modelRoot;
    public Animator modelAnimator;
    
    [Header("Misc")]
    public AnimationCurve timeSlowdownCurve;
    
    private Vector3 moveVelocity;
    public Vector3 MoveVelocity {
        get { return moveVelocity; }
    }
    
    private Vector3 previousMoveVelocityRecorded = new Vector3(0.0f, 0.0f, 1.0f);
    private Vector3 acceleration;
    private Vector3 inputDirection;
    private Timer downVelocityApplyTimer;
    
    private CharacterController characterController;
    private DamageableComponent damageable;
    
    private List<ImpartedVelocity> impartedVelocities = new();
    
    void Awake(){
        player = this;
        downVelocityApplyTimer = new Timer(downVelocityApplyTime);
    }
    
    void Start(){
        characterController = GetComponent<CharacterController>();
        damageable = GetComponent<DamageableComponent>();
        
        damageable.damagedDelegates.Register(OnDamaged);
        damageable.killedDelegates.Register(OnKilled);
    }

    void Update(){
        UpdateInput();
        UpdateModelAndAnimations();
    }
    
    private void UpdateInput(){
        inputDirection = Vector3.zero;
        bool keyPress = false;
        
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
            inputDirection.z = -1.0f;
            keyPress = true;
        } else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
            inputDirection.z = 1.0f;
            keyPress = true;
        }
        
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
            inputDirection.y = 1.0f;
            keyPress = true;
        } else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
            inputDirection.y = -1.0f;
            keyPress = true;
        }
        
        if(Input.GetKeyDown(KeyCode.Space)){
            modelAnimator.SetTrigger("basicslash");

            // Snap instantly to 85% of the input direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection + NONZERO_VECTOR);
            modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, targetRotation, 0.85f);
            
            if(attackMesh != null){
                attackMesh.CastDamageMesh(gameObject, attackStartDelay, attackDuration, attackDamageRange, attackDamageType);
            } else { 
                Debug.LogError("NO ATTACK MESH");
            }
        }
        
        if(keyPress){
            downVelocityApplyTimer.Start();
        }
        
        Vector3 targetVelocity = inputDirection.normalized * moveSpeed;
        moveVelocity = Vector3.SmoothDamp(moveVelocity, targetVelocity, ref acceleration, keyPress ? accelerationTime : decelerationTime);
        moveVelocity.x = 0.0f;
        
        Vector3 actualVelocityToApply = moveVelocity + (Vector3.up * downVelocityApplyTimer.Parameterized() * downVelocity);
        
        // Apply imparted velocities
        for(int i = 0; i < impartedVelocities.Count; ++i){
            ImpartedVelocity v = impartedVelocities[i];
            actualVelocityToApply += v.velocity * (v.decreaseOverTime ? 1.0f - v.impartTimer.Parameterized() : 1.0f);
            
            if(v.impartTimer.Finished()){
                impartedVelocities[i] = impartedVelocities[impartedVelocities.Count - 1];
                impartedVelocities.RemoveAt(impartedVelocities.Count - 1);
                i--;
            }
        }
        
        // Apply the move
        characterController.Move(actualVelocityToApply * Time.deltaTime);
    
        // Always hard-clamp x
        Vector3 pos = transform.position;
        pos.x = 0.0f;
        transform.position = pos;
    }
    
    private void UpdateModelAndAnimations(){
        if(moveVelocity.magnitude < (moveSpeed / 4.0f)){
            previousMoveVelocityRecorded.y = 0.0f;
            Quaternion targetRotation = Quaternion.LookRotation(previousMoveVelocityRecorded + NONZERO_VECTOR);
            modelRoot.localRotation = Quaternion.RotateTowards(modelRoot.localRotation, targetRotation, idleRotationRate * Time.deltaTime); 
            
            modelAnimator.SetBool("swimming", false);
        } else {
            Quaternion targetRotation = Quaternion.LookRotation(moveVelocity + NONZERO_VECTOR);
            modelRoot.localRotation = Quaternion.RotateTowards(modelRoot.localRotation, targetRotation, movingRotationRate * Time.deltaTime); 
            
            modelAnimator.SetBool("swimming", true);
            
            previousMoveVelocityRecorded = moveVelocity;
        }
    }
    
    private void OnDamaged(DamageableComponent damage){
        Vector3 fromDamager = transform.position - damageable.GetDamagerOrigin();
        ImpartVelocity(new ImpartedVelocity(fromDamager.normalized * DAMAGED_VELOCITY, 0.5f, true));
        
        SlowTime(0.6f);
    }
    
    private void OnKilled(DamageableComponent damage){
        // TODO
    }
    
    // Helper functions for global interactions
    public void ImpartVelocity(ImpartedVelocity v){
        impartedVelocities.Add(v);
        v.impartTimer.Start();
    }
    
    public void SlowTime(float realTimeDuration){
        StartCoroutine(SlowTimeCoroutine(realTimeDuration));
    }
    
    private bool slowingTime;
    private IEnumerator SlowTimeCoroutine(float duration){
        if(slowingTime){
            yield break;
        }
        slowingTime = true;
        
        IndependentTimer timeSlowdownTimer = new IndependentTimer(duration);
        timeSlowdownTimer.Start();
        
        while(!timeSlowdownTimer.Finished()){            
            Time.timeScale = timeSlowdownCurve.Evaluate(timeSlowdownTimer.Parameterized());
            yield return null;
        }
        
        Time.timeScale = 1.0f;        
        slowingTime = false;
    }
}
