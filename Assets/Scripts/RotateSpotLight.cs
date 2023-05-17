using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpotLight : MonoBehaviour
{


    // speed of rotation
    public const float speed = 0.5f;

    // from vector indicating from direction (start)
    public Vector3 fromV = new Vector3(0, 20.0f, 0.0f);

    // to vector indicating to direction (end)
    public Vector3 toV = new Vector3(0, -20.0f, 0.0f);
    
 
    void Update () 
    {
        // quaternions
        Quaternion from = Quaternion.Euler(this.fromV);
        Quaternion to = Quaternion.Euler(this.toV);

        // lerp setup using sin function, which causes it to oscillate back and forth
        float lerp = (1.0f + Mathf.Sin(Mathf.PI * Time.realtimeSinceStartup * speed)) * 0.5f;

        // rotate spotlight using lerp
        this.transform.localRotation = Quaternion.Lerp(from, to, lerp);
    }

}
