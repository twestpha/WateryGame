using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class DamageMeshComponent : MonoBehaviour {

    public GameObject hitEffects;
    public GameObject ignoredEffects;
    
    private Collider damageCollider;
    private GameObject caster;
    private DamageableComponent casterDamageable;

    private bool casting;
    private Vector2 attackDamageRange;
    private DamageType attackDamageType;

    void Start(){
        damageCollider = GetComponent<Collider>();
        damageCollider.enabled = false;
    }

    public void CastDamageMesh(GameObject caster_, float delay, float duration, Vector2 attackDamageRange_, DamageType attackDamageType_){
        caster = caster_;
        casterDamageable = caster.GetComponentInParent<DamageableComponent>();
        
        if(!casting){
            attackDamageRange = attackDamageRange_;
            attackDamageType = attackDamageType_;
            StartCoroutine(CastDamageMeshRoutine(delay, duration));
        }
    }

    private IEnumerator CastDamageMeshRoutine(float delay, float duration){
        casting = true;
        
        Timer delayTimer = new Timer(delay);
        delayTimer.Start();
        
        while(!delayTimer.Finished()){
            yield return null;
        }
        
        Timer castTimer = new Timer(duration);
        castTimer.Start();
        damageCollider.enabled = true;
        
        while(casting && !castTimer.Finished()){
            yield return null;
        }

        damageCollider.enabled = false;
        casting = false;
    }

    private void OnTriggerEnter(Collider other){
        DamageableComponent otherDamageable = other.gameObject.GetComponentInParent<DamageableComponent>();
        
        if(otherDamageable != null && otherDamageable != casterDamageable){
            if(DamageableComponent.Hostile(casterDamageable.team, otherDamageable.team)){
                bool damageApplied = otherDamageable.DealDamage(
                    UnityEngine.Random.Range(attackDamageRange.x, attackDamageRange.y),
                    attackDamageType,
                    transform.position,
                    caster
                );

                GameObject fx = GameObject.Instantiate(damageApplied ? hitEffects : ignoredEffects);
                fx.transform.position = damageCollider.ClosestPoint(otherDamageable.transform.position);
                
                casting = false;
                damageCollider.enabled = false;
            }
        }
    }
}