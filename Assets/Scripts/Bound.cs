using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bound : MonoBehaviour
{

    // spawn point
    public Vector3 spawnPoint = new Vector3(0,0,0);
    
    // when the player exits the boundaries, respawn
    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            other.gameObject.GetComponent<PlayerController>().Respawn(0.7f);
        }
    }
}
