using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DialogueVolumeComponent : MonoBehaviour {
    
    public bool repeatable;
    [TextAreaAttribute]
    public string dialogue;
    
    private bool triggered;
        
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            if(repeatable || !triggered){
                PlayerUIComponent.instance.ShowDialogue(dialogue);
            }
        }
    }
}