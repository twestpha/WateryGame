using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BossFightTentacleComponent : MonoBehaviour {
    
    private const float RANDOM_ANGLE = 15.0f;
    private const float TENTACLE_LENGTH = 20.0f;
    private const float TENTACLE_MOVE_TIME = 3.5f;
    private const float LIGHTNING_TIME = 2.5f;
    
    public SkinnedMeshRenderer mesh;
    public Animator tentacleAnimator;
    public ParticleSystem lightningParticles;
    public DamageMeshComponent lightningDamageMesh;
    [Space(10)]
    public Transform[] bounds;
    [Space(10)]
    public Vector2 lightningDamage;
    
    private Vector3 retractedPosition;
    private Vector3 extendedPosition;
    private Vector3 centroid;
    
    public enum TentacleState {
        None,
        WaitingForExtend,
        Extending,
        Idle,
        Shocking,
        Retracting,
    }
    
    private bool immediatelyRetract;
    public TentacleState tentacleState;
    
    private Timer tentacleMoveTimer = new Timer(TENTACLE_MOVE_TIME);
    private Timer waitForExtendTimer = new Timer();
    private Timer lightningTimer = new Timer(LIGHTNING_TIME);
    
    void Start(){
        mesh.enabled = false;
        lightningParticles.gameObject.SetActive(false);
        
        centroid = Vector3.zero;
        for(int i = 0, count = bounds.Length - 1; i < count; ++i){
            centroid += bounds[i].position;
        }
        centroid = centroid / ((float) bounds.Length - 1);
    }
    
    void Update(){
        if(tentacleState == TentacleState.WaitingForExtend){
            if(waitForExtendTimer.Finished()){
                if(immediatelyRetract){
                    tentacleState = TentacleState.None;
                    mesh.enabled = false;
                } else {
                    tentacleState = TentacleState.Extending;
                }
                
                tentacleMoveTimer.Start();
            }
        } else if(tentacleState == TentacleState.Extending){
            float t = CustomMath.EaseInOut(tentacleMoveTimer.Parameterized());
            transform.position = Vector3.Lerp(retractedPosition, extendedPosition, t);
            
            if(tentacleMoveTimer.Finished()){
                if(immediatelyRetract){
                    tentacleState = TentacleState.Retracting;
                    tentacleMoveTimer.Start();
                } else {
                    tentacleState = TentacleState.Idle;
                }
            }
        } else if(tentacleState == TentacleState.Shocking){
            if(lightningTimer.Finished()){
                lightningParticles.gameObject.SetActive(false);
                tentacleState = TentacleState.Retracting;
                lightningDamageMesh.StopCasting();
                tentacleMoveTimer.Start();
            }
        } else if(tentacleState == TentacleState.Retracting){
            float t = CustomMath.EaseInOut(tentacleMoveTimer.Parameterized());
            transform.position = Vector3.Lerp(extendedPosition, retractedPosition, t);
            
            if(tentacleMoveTimer.Finished()){
                tentacleState = TentacleState.None;
                mesh.enabled = false;
            }
        }
    }
    
    [ContextMenu("Debug Extend")]
    public void Extend(){
        immediatelyRetract = false;
        
        // Setup extended position
        int pickedBoundIndex = UnityEngine.Random.Range(1, bounds.Length);
        extendedPosition = Vector3.Lerp(bounds[pickedBoundIndex-1].position, bounds[pickedBoundIndex].position, UnityEngine.Random.value);
        
        // Offset x just a little so the tentacles don't z-fight as clearly
        extendedPosition.x += UnityEngine.Random.Range(-0.1f, 0.1f);
        
        // Setup direction
        Vector3 faceDirection = centroid - extendedPosition;
        transform.rotation = Quaternion.LookRotation(faceDirection, Vector3.forward) * Quaternion.Euler(UnityEngine.Random.Range(-RANDOM_ANGLE, RANDOM_ANGLE), 0.0f, 0.0f);
        
        // Terrible terrible hack
        if(Vector3.Dot(transform.right, Vector3.right) > 0){
            transform.rotation *= Quaternion.Euler(0.0f, 0.0f, 180.0f);
        }
        
        // Setup retracted position
        retractedPosition = extendedPosition + (transform.forward * -TENTACLE_LENGTH);
        transform.position = retractedPosition;
        
        
        mesh.enabled = true;
        tentacleState = TentacleState.WaitingForExtend;
        waitForExtendTimer.SetDuration(UnityEngine.Random.Range(0.0f, 1.5f));
        waitForExtendTimer.Start();
        
        // Offset animation so they're not all synched
        tentacleAnimator.SetFloat("speed", UnityEngine.Random.Range(0.9f, 1.1f));
    }
    
    [ContextMenu("Debug Electrify")]
    public void Electrify(){
        if(tentacleState == TentacleState.Idle){
            lightningParticles.gameObject.SetActive(true);
            lightningDamageMesh.CastDamageMesh(gameObject, 0.0f, LIGHTNING_TIME, lightningDamage, DamageType.BossLightning);
            
            tentacleState = TentacleState.Shocking;
            lightningTimer.Start();
        }
    }
    
    [ContextMenu("Debug Retract")]
    public void Retract(){
        // Either set the flag for "when you're done extending, retract"
        immediatelyRetract = true;
        
        // Or just do it directly
        if(tentacleState == TentacleState.Shocking || tentacleState == TentacleState.Idle){
            lightningParticles.gameObject.SetActive(false);
            tentacleState = TentacleState.Retracting;
            lightningDamageMesh.StopCasting();
            tentacleMoveTimer.Start();
        }
    }
}