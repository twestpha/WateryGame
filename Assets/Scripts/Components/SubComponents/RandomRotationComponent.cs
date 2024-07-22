using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RandomRotationComponent : MonoBehaviour {
    
    public Vector3 low;
    public Vector3 high;
    
    void Start(){
        transform.localRotation = Quaternion.Euler(
            UnityEngine.Random.Range(low.x, high.x),
            UnityEngine.Random.Range(low.y, high.y),
            UnityEngine.Random.Range(low.z, high.z)
        );
    }
}