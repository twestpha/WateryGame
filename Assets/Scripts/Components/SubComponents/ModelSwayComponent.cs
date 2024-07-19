using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSwayComponent : MonoBehaviour {
    
    public Transform model;
    [Space(10)]
    public Vector3 amplitudes;
    public Vector3 frequencies;
    [Space(10)]
    public bool randomize;
    
    private Vector3 origin;
    private Vector3 offset;
    
    void Start(){
        origin = model.localRotation.eulerAngles;
        
        if(randomize){
            offset.x = Random.Range(0.0f, 10.0f);
            offset.y = Random.Range(0.0f, 10.0f);
            offset.z = Random.Range(0.0f, 10.0f);
        }
    }
    
    void Update(){
        model.localRotation = Quaternion.Euler(
            origin.x + (Mathf.Sin((Time.time * frequencies.x) + offset.x) * amplitudes.x),
            origin.y + (Mathf.Sin((Time.time * frequencies.y) + offset.y) * amplitudes.y),
            origin.z + (Mathf.Sin((Time.time * frequencies.z) + offset.z) * amplitudes.z)
        );
    }
}