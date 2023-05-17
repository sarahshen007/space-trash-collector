using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawnBehavior : MonoBehaviour
{

    // all the game objects that need to be spawned
    public GameObject moon;
    public GameObject spaceTrash;
    public GameObject moonSpaceTrash;
    public GameObject satellite;

    // origin point
    public Vector3 origin = new Vector3(0,0,0);

    void Awake()
    {

        // list of moon references
        var moons = new ArrayList();

        // make 8 moons and store them in moons arraylist
        for (int i = 0; i < 8; i++) {
            GameObject newMoon = Instantiate(moon, gameObject.transform.position, Random.rotation, gameObject.transform);
            moons.Add(newMoon);

            // make a piece of space trash
            Instantiate(spaceTrash, gameObject.transform.position, Random.rotation, gameObject.transform);
        }

        // for every moon in moons, make 2 pieces of moon space trash children
        for (int i = 0; i < moons.Count; i++) {
            Instantiate(moonSpaceTrash, gameObject.transform.position, Random.rotation, ((GameObject) moons[i]).transform);
            Instantiate(moonSpaceTrash, gameObject.transform.position, Random.rotation, ((GameObject) moons[i]).transform);
        }

        // make 2 satellites for the planet

        for (int i = 0; i < 2; i++) {
            Instantiate(satellite, gameObject.transform.position, Random.rotation, gameObject.transform);
        }
    }
}
