using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FogVolumeComponent : MonoBehaviour {
    
    [Range(0, 1)]
    public float daylightPercentage;
        
    private void OnTriggerEnter(Collider other){
        Debug.Log(other.gameObject);
        if(other.tag == "Player"){
            LightingManagerComponent.instance.ChangeLighting(daylightPercentage);
        }
    }
}