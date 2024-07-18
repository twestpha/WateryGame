using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraComponent : MonoBehaviour {
    
    [Header("Connections")]
    public PlayerComponent player;
    
    [Header("Camera Attributes")]
    [Tooltip("Time the camera will take to move into position while the character is in motion or not grounded")]
    public float movingSeekTime = 0.2f;
    [Tooltip("Time the camera will take to move into position while the character is still")]
    public float stillSeekTime = 0.2f;
    [Tooltip("Distance ahead the character the camera will target while the character is in motion or not grounded")]
    public float lookAheadDistanceMoving = 2.0f;
    [Tooltip("Distance ahead the character the camera will target while the character is still")]
    public float lookAheadDistanceStill = 1.0f;
    
    private Vector3 acceleration;
    
    void Start(){
        
    }
    
    void Update(){
        Vector3 playerVelocity = player.MoveVelocity;
        bool moving = playerVelocity.magnitude > 0.1f;
        playerVelocity.Normalize();
 
        Vector3 targetPosition = player.transform.position + (playerVelocity * (moving ? lookAheadDistanceMoving : lookAheadDistanceStill));
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref acceleration, moving ? movingSeekTime : stillSeekTime);
    }
}