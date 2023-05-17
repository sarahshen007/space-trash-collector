using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SwitchCamera : MonoBehaviour
{

    // boolean to see whether the back camera is active or not
    public bool backCam; 

    // on fire of button press
    public void OnFire(InputAction.CallbackContext context) {

        // if it started or is being performed, then backcam is active
        if (context.started)
        {
            backCam = true;
        }
        if (context.performed)
        {
            backCam = true;
        }

        // if the finger was lifted, then back cam is not active
        if (context.canceled)
        {
            backCam = false;
        }
    }
}
