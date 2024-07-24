using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Control Data", menuName = "WateryGame/Control Data", order =1 )]
public class ControlData : ScriptableObject {
    public KeyCode up;
    public KeyCode right;
    public KeyCode left;
    public KeyCode down;
    public KeyCode attack;
    public KeyCode ability;
}