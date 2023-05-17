using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;   // target we want to follow w camera
    public float smoothTime = 0.001f;   // time it takes to smoothen/right itself

    public Vector3 velocity = Vector3.zero; // velocity reference
    private Vector3 offset = new Vector3(0, 6, -8); // initial camera offset

    void Start() {

        // reset camera at start
        Reset();
    }

    // camera position and rotation updates after the position and rotation of the player updates
    void LateUpdate()
    {
        SmoothFollow();
    }

    // function for damp and slerp contributing to smooth camera pan following the player
    void SmoothFollow()
    {
        // get the target position by getting the position of the target and adding the original camera position offset to it
        Vector3 targetPosition = target.TransformPoint(offset);

        // set the position of the camera to smoothdamp position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // set the rotation of the camera to slerp rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target.transform.rotation, smoothTime);
    }

    // function to reset to respawn origin and set direction of camera
    public void Reset()
    {
        transform.position = target.TransformPoint(offset);
        transform.rotation = target.rotation;
    }
}
