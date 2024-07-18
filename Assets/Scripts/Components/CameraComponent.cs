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
    [Tooltip("Distance ahead the character the camera will target while the character is still")]
    public float lookAheadDistanceStill = 1.0f;
    [Tooltip("Distance ahead the character the camera will target while the character is in motion or not grounded")]
    public float lookAheadDistanceMoving = 2.0f;
    [Tooltip("Distance up the camera will look at all times")]
    public float lookUpDistance = 0.2f;
    
    private Vector3 acceleration;
    
    void Start(){
        
    }
    
    void Update(){
        bool moving = (Mathf.Abs(player.MoveVelocity) > 0.1f) || !player.Grounded;
        
        Vector3 targetPosition = player.transform.position;
        targetPosition += (Vector3.forward * player.LookDirection * (moving ? lookAheadDistanceMoving : lookAheadDistanceStill));
        targetPosition += (Vector3.up * lookUpDistance);
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref acceleration, moving ? movingSeekTime : stillSeekTime);
    }
}