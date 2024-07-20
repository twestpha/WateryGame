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
    
    [Header("Connections")]
    public Transform modelRoot;
    public Animator modelAnimator;
    
    private Vector3 moveVelocity;
    public Vector3 MoveVelocity {
        get { return moveVelocity; }
    }
    private Vector3 previousMoveVelocityRecorded = new Vector3(0.0f, 0.0f, 1.0f);
    
    private Vector3 acceleration;
    private CharacterController characterController;
    private Timer downVelocityApplyTimer;
    
    private List<ImpartedVelocity> impartedVelocities = new();
    private List<int> impartedVelocitiesToRemove = new();
    
    void Awake(){
        player = this;
        downVelocityApplyTimer = new Timer(downVelocityApplyTime);
    }
    
    void Start(){
        characterController = GetComponent<CharacterController>();
    }

    void Update(){
        UpdateInput();
        UpdateModelAndAnimations();
    }
    
    private void UpdateInput(){
        Vector3 targetVelocity = Vector3.zero;
        bool keyPress = false;
        
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
            targetVelocity.z = -1.0f;
            keyPress = true;
        } else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
            targetVelocity.z = 1.0f;
            keyPress = true;
        }
        
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
            targetVelocity.y = 1.0f;
            keyPress = true;
        } else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
            targetVelocity.y = -1.0f;
            keyPress = true;
        }
        
        if(Input.GetKeyDown(KeyCode.Space)){
            modelAnimator.SetTrigger("basicslash");
            
            // TODO move this to hit-react and deal-damage-react
            // ImpartVelocity(new ImpartedVelocity(-moveVelocity * 2.0f, 0.5f, true));
        }
        
        if(keyPress){
            downVelocityApplyTimer.Start();
        }
        
        targetVelocity = targetVelocity.normalized * moveSpeed;
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
            Quaternion targetRotation = Quaternion.LookRotation(previousMoveVelocityRecorded);
            modelRoot.localRotation = Quaternion.RotateTowards(modelRoot.localRotation, targetRotation, idleRotationRate * Mathf.Deg2Rad); 
            
            modelAnimator.SetBool("swimming", false);
        } else {
            Quaternion targetRotation = Quaternion.LookRotation(moveVelocity);
            modelRoot.localRotation = Quaternion.RotateTowards(modelRoot.localRotation, targetRotation, movingRotationRate * Mathf.Deg2Rad); 
            
            modelAnimator.SetBool("swimming", true);
            
            previousMoveVelocityRecorded = moveVelocity;
        }
    }
    
    public void ImpartVelocity(ImpartedVelocity v){
        impartedVelocities.Add(v);
        v.impartTimer.Start();
    }
}
