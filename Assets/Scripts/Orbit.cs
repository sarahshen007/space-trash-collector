using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    // is randomly generated (ie not from collision w/ planet or moon)
    public bool isRandom = true;

    // origin point
    public Vector3 origin;

    // speed of rotation
    public float speed = 1;

    // parent object (orbital center)
    public GameObject targetObj;
    private Transform targetTransform;

    // location coordinate minimums and threshold beyond
    public float locMin = 35;
    public float locMax = 100;

    // speed multiplier
    public const float speedMult = 300;

    // rotation axis
    public Vector3 axis = Vector3.up;


    void Start()
    {
        SetOrbit();

        // set orbit and position
        if (isRandom) {
            SetLocation();
        }

        // set orbital speed according to radius
        SetSpeed();

    }

    void Update()
    {
        // update origin
        origin = targetTransform.position;

        // update speed (in case object has shifted)
        SetSpeed();  

        // rotate around the origin at some axis and speed
        transform.RotateAround(origin, axis, speed * Time.deltaTime);
    }


    // set orbital speed according to radius
    void SetSpeed()
    {
        // speed is inversely related to distance from the orbital origin
        speed = speedMult / (transform.position - origin).magnitude; 
    }

    private void SetOrbit() {
        // find target object
        targetObj = gameObject.transform.parent.gameObject;   

        // find target transform
        targetTransform = targetObj.transform;

        // set orbital origin
        origin = targetTransform.position;

        // objects can at minimum orbit one radius away from center of parent object
        locMin = targetObj.GetComponent<MeshRenderer>().bounds.size.x / 2;

        // objects can at minimum orbit two radiuses away from the center of parent object
        locMax = targetObj.GetComponent<MeshRenderer>().bounds.size.x;

        // if the orbit object isn't the planet or if the game object is a satellite, choose a new random axis
        if (!targetObj.CompareTag("Planet") || gameObject.tag == "Satellite") {
            axis = Vector3.Normalize(new Vector3(Random.Range(-1,1), Random.Range(-1,1), Random.Range(-1,1)));
        }

    }

    public void SetLocation(float x = 0, float y = 0, float z = 0)
    {
        // random location
        
        // keep choosing random values until they are in realistic range
        while (x < locMin && x > -locMin) {
            x = Random.Range(-locMax, locMax);
        }

        while (z < locMin && z > -locMin) {
            z = Random.Range(-locMax, locMax);
        }
    
        // set orbit object position to new random location
        transform.position = targetObj.transform.position + new Vector3(x, y, z);
    }

}
