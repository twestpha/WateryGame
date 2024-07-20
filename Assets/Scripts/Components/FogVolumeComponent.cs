using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FogVolumeComponent : MonoBehaviour {
    
    public float changeTime;
    public Color targetColor;
    
    private Color currentColor;
    private Camera cachedCamera;
    
    void Start(){
        cachedCamera = Camera.main;
    }
    
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            StartCoroutine(ChangeFogColor());
        }
    }
    
    private IEnumerator ChangeFogColor(){
        Timer changeTimer = new Timer(changeTime);
        changeTimer.Start();
        
        currentColor = RenderSettings.fogColor;
        
        while(!changeTimer.Finished()){
            RenderSettings.fogColor = Color.Lerp(currentColor, targetColor, changeTimer.Parameterized());
            cachedCamera.backgroundColor = RenderSettings.fogColor;
            yield return null;
        }
        
        RenderSettings.fogColor = targetColor;
        cachedCamera.backgroundColor = targetColor;
    }
}