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
    public const float ABILITY_TIME = 10.0f;
    
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
    [Tooltip("After taking damage, how long the player should be incible from other damage")]
    public float invincibilityTime;
    
    [Header("Attack Attributes")]
    [Range(0, 1)]
    [Tooltip("DEBUG: What percentage is the players light at right now")]
    public float currentLightAmount;
    [Tooltip("How fast should the player's light drain")]
    public float lightDrainRate;
    [Tooltip("What intensities should the light be, where when lightAmount is 0 it's x, and at 1, y")]
    public Vector2 lightIntensityRange;
    [Tooltip("What radii should the light be, where when lightAmount is 0 it's x, and at 1, y")]
    public Vector2 lightRadiusRange;
    
    [Header("Connections")]
    public Transform modelRoot;
    public Animator modelAnimator;
    public Light playerLight;
    
    [Header("Misc")]
    public AnimationCurve timeSlowdownCurve;
    public bool movementInputsEnabled;
    public ControlData controlData;
    
    private Vector3 moveVelocity;
    public Vector3 MoveVelocity {
        get { return moveVelocity; }
    }
    
    private Vector3 previousMoveVelocityRecorded = new Vector3(0.0f, 0.0f, 1.0f);
    private Vector3 acceleration;
    private Vector3 inputDirection;
    
    private Timer downVelocityApplyTimer;
    private Timer invincibilityTimer;
    private Timer abilityTimer = new Timer(ABILITY_TIME);
    public Timer AbilityTimer { get { return abilityTimer; }}
    
    private bool invincible;
    private bool slowingTime;
    
    private AbilityType currentAbility;
    public AbilityType CurrentAbility { get { return currentAbility; }}
    
    
    private CharacterController characterController;
    private DamageableComponent damageable;
    public DamageableComponent Damageable { get { return damageable; }}
    private AbilityManagerComponent abilityManager;
    public AbilityManagerComponent AbilityManager { get { return abilityManager; }}
    
    private List<ImpartedVelocity> impartedVelocities = new();
    
    void Awake(){
        player = this;
    }
    
    void Start(){
        characterController = GetComponent<CharacterController>();
        damageable = GetComponent<DamageableComponent>();
        abilityManager = GetComponent<AbilityManagerComponent>();
        
        damageable.damagedDelegates.Register(OnDamaged);
        damageable.killedDelegates.Register(OnKilled);
        
        downVelocityApplyTimer = new Timer(downVelocityApplyTime);
        invincibilityTimer = new Timer(invincibilityTime);
    }

    void Update(){
        UpdateInput();
        UpdateModelAndAnimations();
        UpdateLight();
        
        if(invincible && invincibilityTimer.Finished()){
            damageable.SetInvincible(false);
        }
        
        if(currentAbility != AbilityType.None && abilityTimer.Finished()){
            currentAbility = AbilityType.None;
        }
    }
    
    private void UpdateInput(){
        inputDirection = Vector3.zero;
        bool keyPress = false;
        
        if(movementInputsEnabled){
            if(Input.GetKey(controlData.left)){
                inputDirection.z = -1.0f;
                keyPress = true;
            } else if(Input.GetKey(controlData.right)){
                inputDirection.z = 1.0f;
                keyPress = true;
            }
            
            if(Input.GetKey(controlData.up)){
                inputDirection.y = 1.0f;
                keyPress = true;
            } else if(Input.GetKey(controlData.down)){
                inputDirection.y = -1.0f;
                keyPress = true;
            }
        }
        
        if(Input.GetKeyDown(controlData.attack)){
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
        
        if(Input.GetKeyDown(controlData.ability) && currentAbility != AbilityType.None){
            abilityManager.CastAbility(currentAbility);
        }
        
        if(keyPress || !movementInputsEnabled){
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
        
        if(actualVelocityToApply.magnitude > (moveSpeed / 4.0f)){
            previousMoveVelocityRecorded = actualVelocityToApply;
        }
    
        // Always hard-clamp x
        Vector3 pos = transform.position;
        pos.x = 0.0f;
        transform.position = pos;
    }
    
    private void UpdateModelAndAnimations(){
        if(previousMoveVelocityRecorded.magnitude < (moveSpeed / 4.0f)){
            Quaternion targetRotation = Quaternion.LookRotation(previousMoveVelocityRecorded + NONZERO_VECTOR);
            modelRoot.localRotation = Quaternion.RotateTowards(modelRoot.localRotation, targetRotation, idleRotationRate * Time.deltaTime); 
            
            modelAnimator.SetBool("swimming", false);
        } else {
            Quaternion targetRotation = Quaternion.LookRotation(previousMoveVelocityRecorded);
            modelRoot.localRotation = Quaternion.RotateTowards(modelRoot.localRotation, targetRotation, movingRotationRate * Time.deltaTime); 
            
            modelAnimator.SetBool("swimming", true);
        }
    }
    
    private void UpdateLight(){
        currentLightAmount = Mathf.Max(currentLightAmount - (lightDrainRate * Time.deltaTime), 0.0f);
        playerLight.intensity = Mathf.Lerp(lightIntensityRange.x, lightIntensityRange.y, currentLightAmount);
        playerLight.range = Mathf.Lerp(lightRadiusRange.x, lightRadiusRange.y, currentLightAmount);
    }
    
    private void OnDamaged(DamageableComponent damage){
        Vector3 fromDamager = transform.position - damageable.GetDamagerOrigin();
        ImpartVelocity(new ImpartedVelocity(fromDamager.normalized * DAMAGED_VELOCITY, 0.5f, true));
        
        SlowTime(0.6f);
        
        invincible = true;
        damageable.SetInvincible(true);
        invincibilityTimer.Start();
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
    
    public void GiveResources(float health, bool armor, float light, AbilityType ability){
        if(health > 0.0f){
            Debug.Log("Player getting " + health + " health!");
            damageable.Heal(health);
        }
        if(armor){
            Debug.Log("Player getting " + armor + " armor!");
            damageable.hasArmor = true;
        }
        if(light > 0.0f){
            Debug.Log("Player getting " + light + " light!");
            currentLightAmount = Mathf.Clamp(currentLightAmount + light, 0.0f, 1.0f);
        }
        if(ability != AbilityType.None){
            Debug.Log("Player getting " + ability + " ability!");
            currentAbility = ability;
            abilityTimer.Start();
        }
    }
    
    [ContextMenu("Debug Give Dash")]
    public void DebugGiveDash(){
        currentAbility = AbilityType.PlayerDash;
        abilityTimer.Start();
    }
    
    public Vector3 GetPreviousMoveVelocity(){
        return previousMoveVelocityRecorded;
    }
}
