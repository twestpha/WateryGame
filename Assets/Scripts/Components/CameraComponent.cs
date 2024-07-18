using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraComponent : MonoBehaviour {
    
    [Header("Connections")]
    public PlayerComponent player;
    
    [Header("Camera Attributes")]
    public float movingSeekTime = 0.2f;
    public float stillSeekTime = 0.2f;
    public float lookAheadDistanceStill = 1.0f;
    public float lookAheadDistanceMoving = 2.0f;
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