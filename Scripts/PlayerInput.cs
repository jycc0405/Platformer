using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public string xAxisName = "Horizontal";
    public string yAxisName = "Vertical";
    public string dashKeyName = "Dash";
    public string jumpKeyName = "Jump";
    public string climbKeyName = "Climb";
    
    public float Xmove { get; private set; }
    public float Ymove { get; private set; }
    public bool Dash { get; private set; }
    public bool JumpButtonDown { get; private set; }
    public bool JumpButtonUp { get; private set; }
    public bool Climb { get; private set; }
    public bool ClimbUp { get; private set; }


    private void Update()
    {
        Xmove = Input.GetAxisRaw(xAxisName);
        Ymove = Input.GetAxisRaw(yAxisName);
        Dash = Input.GetButtonDown(dashKeyName);
        JumpButtonDown = Input.GetButtonDown(jumpKeyName);
        JumpButtonUp = Input.GetButtonUp(jumpKeyName);
        Climb = Input.GetButton(climbKeyName);
        ClimbUp = Input.GetButtonUp(climbKeyName);
    }
}
