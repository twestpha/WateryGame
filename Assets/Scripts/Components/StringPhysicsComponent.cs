using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StringPhysicsComponent : MonoBehaviour {

    public Transform rootTransform;
    public Transform[] descendingTransforms;
    
    public float[] distances;
    public Vector3[] previousWorldSpaces;
        
    void Start(){
        distances = new float[descendingTransforms.Length];
        previousWorldSpaces = new Vector3[descendingTransforms.Length];
        
        for(int i = 0, count = descendingTransforms.Length; i < count; ++i){
            distances[i] = (i == 0) ?
                (rootTransform.position - descendingTransforms[i].position).magnitude :
                (descendingTransforms[i-1].position - descendingTransforms[i].position).magnitude;
                
            previousWorldSpaces[i] = descendingTransforms[i].position;
        }
    }
    
    void Update(){
        for(int i = 0, count = descendingTransforms.Length; i < count; ++i){
            Vector3 beforeMove = previousWorldSpaces[i];
            
            previousWorldSpaces[i] = (i == 0) ?
                rootTransform.position + ((previousWorldSpaces[i] - rootTransform.position).normalized * distances[i]) :
                previousWorldSpaces[i-1] + ((previousWorldSpaces[i] - previousWorldSpaces[i-1]).normalized * distances[i]);

            descendingTransforms[i].position = previousWorldSpaces[i];
            
            if(i == 0){
                rootTransform.rotation = Quaternion.LookRotation(previousWorldSpaces[i]- rootTransform.position);
            } else {
                // descendingTransforms[i].rotation = Quaternion.LookRotation(previousWorldSpaces[i]- rootTransform.position);
            }
        }
    }
}