using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityComponent : MonoBehaviour {
    public bool needsUpdate;
    public virtual void CastAbility(){}
    public virtual void CustomUpdate(){}
}