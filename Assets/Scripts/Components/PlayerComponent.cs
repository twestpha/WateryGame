using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
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
    
    [Header("AbilityMeshes")]
    public GameObject[] dashAbilityMeshes;
    public GameObject[] spikeAbilityMeshes;
    
    [Header("Ability Dropoff Particles")]
    public GameObject[] dashAbilityDropoffParticles;
    public GameObject[] spikeAbilityDropoffParticles;
    
    [Header("Ability Pickup Particles")]
    public GameObject healthPickupParticle;
    public GameObject armorPickupParticle;
    public GameObject lightPickupParticle;
    public GameObject dashPickupParticle;
    public GameObject spikePickupParticle;
    
    [Header("Audio")]
    public AudioSource attackSound;
    public AudioSource damagedSound;
    
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
    private Vector3 respawnPosition;
    
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
        respawnPosition = transform.position;
        
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
            if(currentAbility == AbilityType.PlayerDash){
                for(int i = 0, count = dashAbilityMeshes.Length; i < count; ++i){
                    dashAbilityMeshes[i].SetActive(false);
                    dashAbilityDropoffParticles[i].SetActive(true);
                }
            } else if(currentAbility == AbilityType.PlayerSpikes){
                for(int i = 0, count = spikeAbilityMeshes.Length; i < count; ++i){
                    spikeAbilityMeshes[i].SetActive(false);
                    spikeAbilityDropoffParticles[i].SetActive(true);
                }
            }
            
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
        
        if(Input.GetKeyDown(controlData.ability) && currentAbility != AbilityType.None){
            abilityManager.CastAbility(currentAbility);
        }
        
        bool casting = currentAbility != AbilityType.None && abilityManager.Casting(currentAbility);
        
        if(!casting && Input.GetKeyDown(controlData.attack)){
            modelAnimator.SetTrigger("basicslash");
            attackSound.Play();

            // Snap instantly to 85% of the input direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection + NONZERO_VECTOR);
            modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, targetRotation, 0.85f);
            
            if(attackMesh != null){
                attackMesh.CastDamageMesh(gameObject, attackStartDelay, attackDuration, attackDamageRange, attackDamageType);
            } else { 
                Debug.LogError("NO ATTACK MESH");
            }
        }
        
        if(keyPress || !movementInputsEnabled){
            downVelocityApplyTimer.Start();
        }
        
        Vector3 targetVelocity = inputDirection.normalized * moveSpeed;
        moveVelocity = Vector3.SmoothDamp(moveVelocity, targetVelocity, ref acceleration, keyPress ? accelerationTime : decelerationTime);
        moveVelocity.x = 0.0f;
        
        Vector3 actualVelocityToApply = moveVelocity;
        
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

        if(actualVelocityToApply.magnitude > (moveSpeed / 4.0f)){
            previousMoveVelocityRecorded = actualVelocityToApply;
        }
        
        // Apply the move
        actualVelocityToApply += (Vector3.up * downVelocityApplyTimer.Parameterized() * downVelocity);
        characterController.Move(actualVelocityToApply * Time.deltaTime);
    
        // Always hard-clamp x
        Vector3 pos = transform.position;
        pos.x = 0.0f;
        transform.position = pos;
    }
    
    private void UpdateModelAndAnimations(){
        if(moveVelocity.magnitude < (moveSpeed / 4.0f)){
            Vector3 targetLook = previousMoveVelocityRecorded + NONZERO_VECTOR;
            targetLook.y = 0.0f; // Don't remove this allie, it makes the player look right/left when idling
            
            Quaternion targetRotation = Quaternion.LookRotation(targetLook);
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
        
        damagedSound.Play();

        SlowTime(0.6f);
        
        invincible = true;
        damageable.SetInvincible(true);
        invincibilityTimer.Start();
        
        AudioManager.instance.NotifyOfCombat();
    }
    
    private void OnKilled(DamageableComponent damage){
        movementInputsEnabled = false;
        PlayerUIComponent.instance.FadeInOutForRespawn();
    }
    
    public void FinalizeRespawn(){
        #if UNITY_EDITOR
            currentLightAmount = 0.0f;
            currentAbility = AbilityType.None;
            movementInputsEnabled = true;
            
            characterController.enabled = false;
            transform.position = respawnPosition;
            characterController.enabled = true;
            
            damageable.Respawn();
        #else
            SceneManager.LoadScene(0);
        #endif
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
            PlayerUIComponent.instance.ShowDialogue("Transmuted to Health");
            damageable.Heal(health);
            healthPickupParticle.SetActive(true);
        }
        if(armor){
            Debug.Log("Player getting " + armor + " armor!");
            PlayerUIComponent.instance.ShowDialogue("Transmuted to Armor");
            damageable.hasArmor = true;
            armorPickupParticle.SetActive(true);
        }
        if(light > 0.0f){
            Debug.Log("Player getting " + light + " light!");
            PlayerUIComponent.instance.ShowDialogue("Transmuted to Light");
            currentLightAmount = Mathf.Clamp(currentLightAmount + light, 0.0f, 1.0f);
            lightPickupParticle.SetActive(true);
        }
        if(ability != AbilityType.None){
            Debug.Log("Player getting " + ability + " ability!");
            
            currentAbility = ability;
            abilityTimer.Start();
            
            if(currentAbility == AbilityType.PlayerDash){
                for(int i = 0, count = dashAbilityMeshes.Length; i < count; ++i){
                    dashAbilityMeshes[i].SetActive(true);
                }
                dashPickupParticle.SetActive(true);
                PlayerUIComponent.instance.ShowDialogue("Transmuted to Dash Power");
                
                for(int i = 0, count = spikeAbilityMeshes.Length; i < count; ++i){
                    spikeAbilityMeshes[i].SetActive(false);
                }
            } else if(currentAbility == AbilityType.PlayerSpikes){
                for(int i = 0, count = spikeAbilityMeshes.Length; i < count; ++i){
                    spikeAbilityMeshes[i].SetActive(true);
                }
                spikePickupParticle.SetActive(true);
                PlayerUIComponent.instance.ShowDialogue("Transmuted to Spike Power");
                
                for(int i = 0, count = dashAbilityMeshes.Length; i < count; ++i){
                    dashAbilityMeshes[i].SetActive(false);
                }
            }        
        }
    }
    
    [ContextMenu("Debug Give Dash")]
    public void DebugGiveDash(){
        GiveResources(0.0f, false, 0.0f, AbilityType.PlayerDash);
    }
    
    [ContextMenu("Debug Give Spikes")]
    public void DebugGiveSpikes(){
        GiveResources(0.0f, false, 0.0f, AbilityType.PlayerSpikes);
    }
    
    public Vector3 GetPreviousMoveVelocity(){
        return previousMoveVelocityRecorded;
    }
}
