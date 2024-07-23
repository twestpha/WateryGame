using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LightingManagerComponent : MonoBehaviour {
    
    private const float CHANGE_TIME = 0.5f;
    
    public static LightingManagerComponent instance;
    
    public Vector2 daylightMainLightBrightnessRange;
    public Vector2 daylightCharacterLightBrightnessRange;
    public Vector2 daylightUnderLightBrightnessRange;
    public Color daylightBackgroundColor;
    public Color underwaterBackgroundColor;
    
    [Space(10)]
    public Light mainLight;
    public Light characterLight;
    public Light underLight;
    
    private float currentMainLightBrightness;
    private float currentCharacterLightBrightness;
    private float currentUnderLightBrightness;
    private Color currentBackgroundColor;
    
    private Camera cachedCamera;
    
    void Awake(){
        instance = this;
    }
    
    void Start(){
        cachedCamera = Camera.main;
    }
    
    public void ChangeLighting(float daylight){
        StartCoroutine(UpdateLighting(daylight));
    }
    
    private IEnumerator UpdateLighting(float daylight){
        Timer changeTimer = new Timer(CHANGE_TIME);
        changeTimer.Start();
        
        currentMainLightBrightness = mainLight.intensity;
        currentCharacterLightBrightness = characterLight.intensity;
        currentUnderLightBrightness = underLight.intensity;
        currentBackgroundColor = RenderSettings.fogColor;
        
        float targetMainLightBrightness = Mathf.Lerp(daylightMainLightBrightnessRange.x, daylightMainLightBrightnessRange.y, daylight);
        float targetCharacterLightBrightness = Mathf.Lerp(daylightCharacterLightBrightnessRange.x, daylightCharacterLightBrightnessRange.y, daylight);
        float targetUnderLightBrightness = Mathf.Lerp(daylightUnderLightBrightnessRange.x, daylightUnderLightBrightnessRange.y, daylight);
        Color targetBackgroundColor = Color.Lerp(underwaterBackgroundColor, daylightBackgroundColor, daylight);
        
        while(!changeTimer.Finished()){
            float t = changeTimer.Parameterized();
            
            mainLight.intensity = Mathf.Lerp(currentMainLightBrightness, targetMainLightBrightness, t);
            characterLight.intensity = Mathf.Lerp(currentCharacterLightBrightness, targetCharacterLightBrightness, t);
            underLight.intensity = Mathf.Lerp(currentUnderLightBrightness, targetUnderLightBrightness, t);
            
            RenderSettings.fogColor = Color.Lerp(currentBackgroundColor, targetBackgroundColor, t);
            cachedCamera.backgroundColor = RenderSettings.fogColor;
            
            yield return null;
        }
        
        
        RenderSettings.fogColor = targetBackgroundColor;
        cachedCamera.backgroundColor = targetBackgroundColor;
    }
}