using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour {

    public float moveSpeed = 0.1f;
    public float inAirMoveSpeed = 0.1f;
    public float accelerationTime = 0.5f;
    [Space(10)]
    public float jumpSpeed = 10.0f;
    public float gravity = 10.0f;
    
    private float moveVelocity;
    private float acceleration;
    private float verticalVelocity;
    
    private CharacterController characterController;
    
    void Start(){
        characterController = GetComponent<CharacterController>();
    }

    void Update(){
        bool grounded = characterController.isGrounded;
        
        float direction = 0.0f;
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
            direction = -1.0f;
        } else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
            direction = 1.0f;
        }
        
        direction *= (grounded ? moveSpeed : inAirMoveSpeed);
        moveVelocity = Mathf.SmoothDamp(moveVelocity, direction, ref acceleration, accelerationTime);   
    
        // Keep a little down-velocity on the ground, otherwise apply gravity downward
        if(grounded){
            verticalVelocity = -0.5f;
        } else {
            verticalVelocity += -(gravity) * Time.deltaTime;
        }
        
        if(Input.GetKeyDown(KeyCode.Space)){
            verticalVelocity = jumpSpeed;
        }
        
        Vector3 velocity = new Vector3(0.0f, verticalVelocity, moveVelocity);
        Vector3 previousPosition = transform.position;
        characterController.Move(velocity * Time.deltaTime);
        
        // Zero out velocity if we bumped our head
        float verticalMove = Mathf.Abs(transform.position.y - previousPosition.y) / Time.deltaTime;
        if(verticalVelocity > 0.1f && verticalMove < 0.1f){
            verticalVelocity = 0.0f;
        }
        
        // If grounded, add velocity uppp
        // else, down just a bit
        // Coyote time
        // Hold-to-jump
        
        // Always hard-clamp x
        Vector3 pos = transform.position;
        pos.x = 0.0f;
        transform.position = pos;
    }
}
