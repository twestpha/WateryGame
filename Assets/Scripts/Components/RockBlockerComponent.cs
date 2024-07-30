using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBlockerComponent : MonoBehaviour {
    
    public EnemyFishAIComponent[] fishesToKill;
    public Collider selfCollider;
    public MeshRenderer selfRenderer;
    public GameObject particlesA;
    public GameObject particlesB;
    
    private bool finished;
    
    void Update(){
        if(!finished){
            bool allDead = true;
            
            for(int i = 0, count = fishesToKill.Length; i < count; ++i){
                allDead = (allDead && fishesToKill[i] == null);
            }
            
            if(allDead){
                finished = true;
                
                selfCollider.enabled = false;
                selfRenderer.enabled = false;
                
                particlesA.SetActive(true);
                particlesB.SetActive(true);
            }
        }
    }

}