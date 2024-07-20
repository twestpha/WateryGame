using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class DamageMeshComponent : MonoBehaviour {

    private Collider damageCollider;
    private GameObject caster;
    // private AbilityData abilityData;
    // private MobaUnitComponent casterUnit;
    private DamageableComponent casterDamageable;

    private bool casting;
    private Timer castTimer = new Timer();

    void Start(){
        damageCollider = GetComponent<Collider>();
        damageCollider.enabled = false;
    }

    // public void CastDamageMesh(AbilityData abilityData_, GameObject caster_){
    //     caster = caster_;
    //     abilityData = abilityData_;
    // 
    //     casterUnit = caster.GetComponentInParent<MobaUnitComponent>();
    //     casterDamageable = caster.GetComponentInParent<DamageableComponent>();
    // 
    //     casting = true;
    //     StartCoroutine(CastDamageMeshRoutine());
    // }

    private IEnumerator CastDamageMeshRoutine(){
        damageCollider.enabled = true;

        // castTimer.SetDuration(abilityData.damageMeshDuration);
        castTimer.Start();

        // Run for at least five frames, because of physics stuff
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        while(casting && !castTimer.Finished()){
            yield return null;
        }

        damageCollider.enabled = false;
        casting = false;
    }

    private void OnTriggerEnter(Collider other){
        // DamageableComponent otherDamageable = other.gameObject.GetComponentInParent<DamageableComponent>();
        // 
        // if(otherDamageable != null && otherDamageable != casterDamageable){
        //     if(abilityData.affectEnemies && DamageableComponent.Hostile(casterDamageable.team, otherDamageable.team)){
        //         otherDamageable.DealDamage(
        //             AbilityData.CalculateDamage(casterUnit.GetUnitData(), casterDamageable, abilityData),
        //             abilityData.damageType,
        //             transform.position,
        //             caster
        //         );
        // 
        //         if(abilityData.damageMeshImpactEffects != null){
        //             GameObject fx = GameObject.Instantiate(abilityData.damageMeshImpactEffects);
        //             fx.transform.position = damageCollider.ClosestPoint(otherDamageable.transform.position);
        //         }
        //         if(abilityData.damageFirstHit){
        //             casting = false;
        //             damageCollider.enabled = false;
        //         }
        //     }
        //     if(abilityData.affectAllies && DamageableComponent.Friendly(casterDamageable.team, otherDamageable.team)){
        // 
        //         otherDamageable.Heal(
        //             AbilityData.CalculateDamage(casterUnit.GetUnitData(), casterDamageable, abilityData)
        //         );
        // 
        //         if(abilityData.damageMeshImpactEffects != null){
        //             GameObject fx = GameObject.Instantiate(abilityData.damageMeshImpactEffects);
        //             fx.transform.position = damageCollider.ClosestPoint(otherDamageable.transform.position);
        //         }
        //         if(abilityData.damageFirstHit){
        //             casting = false;
        //             damageCollider.enabled = false;
        //         }
        //     }
        // }
    }
}