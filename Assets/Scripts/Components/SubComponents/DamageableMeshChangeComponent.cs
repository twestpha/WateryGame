using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class DamageableMeshChangeComponent : MonoBehaviour {
    
    private int CYCLES = 6;
    private float DURATION = 0.2f;

    public Material damagedMaterial;
    public MeshRenderer[] renderersToChange;
    public SkinnedMeshRenderer[] skinnedRenderersToChange;
    
    private DamageableComponent damageable;
    private Material[] originalMaterialRenderers;
    private Material[] originalMaterialSkinnedRenderers;
        
    void Start(){
        damageable = GetComponentInParent<DamageableComponent>();
        damageable.damagedDelegates.Register(OnDamaged);
        
        originalMaterialRenderers = new Material[renderersToChange.Length];
        originalMaterialSkinnedRenderers = new Material[skinnedRenderersToChange.Length];
        
        for(int j = 0, jcount = renderersToChange.Length; j < jcount; ++j){
            originalMaterialRenderers[j] = renderersToChange[j].material;
        }
        
        for(int k = 0, kcount = skinnedRenderersToChange.Length; k < kcount; ++k){
            originalMaterialSkinnedRenderers[k] = skinnedRenderersToChange[k].material;
        }
    }
    
    private void OnDamaged(DamageableComponent damage){
        StartCoroutine(DamageFlash());
    }
    
    private IEnumerator DamageFlash(){
        Timer durationTimer = new Timer(DURATION);
        
        for(int i = 0; i < CYCLES; ++i){
            durationTimer.Start();
            
            while(!durationTimer.Finished()){
                yield return null;
            }
            
            for(int j = 0, jcount = renderersToChange.Length; j < jcount; ++j){
                renderersToChange[j].material = (i % 2 == 0) ? damagedMaterial : originalMaterialRenderers[j];
            }
            
            for(int k = 0, kcount = skinnedRenderersToChange.Length; k < kcount; ++k){
                skinnedRenderersToChange[k].material = (i % 2 == 0) ? damagedMaterial : originalMaterialSkinnedRenderers[k];
            }
        }
        
        // Put it back
        for(int j = 0, jcount = renderersToChange.Length; j < jcount; ++j){
            renderersToChange[j].material = originalMaterialRenderers[j];
        }
        for(int k = 0, kcount = skinnedRenderersToChange.Length; k < kcount; ++k){
            skinnedRenderersToChange[k].material = originalMaterialSkinnedRenderers[k];
        }
    }
}