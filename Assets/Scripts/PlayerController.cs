using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TouchPhase = UnityEngine.TouchPhase;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // player input
    PlayerInput playerInput;
    SwitchCamera switchCam;

    // cameras
    public Camera cam;
    public Camera camBack;

    // prefabs
    public GameObject moonSpaceTrash;
    public GameObject spaceTrash;
    public GameObject explosion;
    public GameObject pointsNegative;
    public GameObject pointsPositive;

    // constants for speed, thrust
    public const float SPEED = 10;
    public const float THRUST = 20;

    // magnitudes for speed, thrust, pitch, yaw, and roll
    public float speed = SPEED;
    public float thrust = THRUST;
    public const float pitch = 70;
    public const float yaw = 70;
    public const float roll = 55;

    // respawn offset, world originpoint, and spawn rotation
    private const float originOffset = 1.5f;
    private Vector3 originPoint = new Vector3(0, 0, 0);
    private Vector3 originRotation = Vector3.left;

    // score
    public TextMeshProUGUI scoreText;
    private float score = 0;

    // health
    public Image healthBar;
    private float health = 5;

    // countdown
    float currentTime = 0;
    private const float startingTime = 150f;
    public TextMeshProUGUI timeText;

    // controls and UI
    public Image joystick;
    public Image throttle;
    public Image throttleParent;
    public Image backCam;
    public Canvas canvas;

    // messaging
    public TextMeshProUGUI message;

    // smoothtime for camera
    private float smoothTime;

    void Awake()
    {

        // switch camera
        switchCam = GetComponent<SwitchCamera>();

        // set message text to nothing and disable it
        message.text = "";
        message.enabled = false;

        // set score
        SetScore();

        // set current time
        currentTime = startingTime;

        // get playerinput component on awake
        playerInput = GetComponent<PlayerInput>();

        // set spawn position
        transform.position = new Vector3(70,0,0);

        // reset player location and direction
        ResetSpawnPoint(originOffset);

        // set smoothtime
        smoothTime = cam.GetComponent<CameraController>().smoothTime;
    }

    void Update() 
    {
        // motions of player
        Thrust();
        Forward();
        Rotate();
        
        // touch interactions
        OnTouch();   

        // check which camera should be main
        CheckCam();
    }

    void LateUpdate() 
    {
        // update ui
        SetTime();
        SetScore();
    }


    // collision exit handler for bounds
    private void OnTriggerExit(Collider other) {

        // exiting bounds
        if (other.gameObject.CompareTag("Bounds")) {
            // effects
            StartCoroutine(FlickerMessage("Out of Bounds"));

            // decrement score
            ChangeScore(true);
            SetScore();
        }
    }

    // collision handler for player
    private void OnTriggerEnter(Collider other) 
    {

        // if collide with space trash
        if (other.gameObject.CompareTag("SpaceTrash") || other.gameObject.CompareTag("HitTrash")) {

            // add to score
            ChangeScore(false);

            // disable collided object
            other.gameObject.SetActive(false);
        } 

        // else if not out of bounds object
        else if (!(other.gameObject.CompareTag("Bounds"))) {

            // crash animation
            Crash();

            // if planet collision
            if (other.gameObject.CompareTag("Planet")) {

                // respawn
                Respawn(3f);

                // spawn random number of trash
                int num = Random.Range(3,6);

                for (; num > 0; num--) {
                    GameObject trash = Instantiate(spaceTrash, transform.position, Random.rotation, (other.gameObject).transform) as GameObject;
                    Orbit orbit = trash.GetComponent<Orbit>();
                    orbit.isRandom = false;
                    orbit.speed = Random.Range(1,2);
                    trash.transform.position = gameObject.transform.position + new Vector3(Random.Range(-2,2), Random.Range(-2,2), Random.Range(0,5));
                    trash.transform.rotation = Random.rotation;
                }
            }

            // if moon collision
            else if (other.gameObject.CompareTag("Moon")) {

                // respawn
                Respawn();

                // spawn random number of trash
                int num = Random.Range(1,2);

                for ( ; num > 0; num--) {
                    GameObject trash = Instantiate(moonSpaceTrash, transform.position, Random.rotation, (other.gameObject).transform) as GameObject;
                    Orbit orbit = trash.GetComponent<Orbit>();
                    orbit.isRandom = false;
                    orbit.speed = Random.Range(1,2);
                    trash.transform.position = gameObject.transform.position + new Vector3(Random.Range(-2,2), Random.Range(-2,2), Random.Range(0,5));
                    trash.transform.rotation = Random.rotation;
                }                
            }

            // if satellite collision
            else {

                // respawn
                Respawn();
            }

            // decrease score
            ChangeScore(true);

            // decrement health
            health--;      
        }

        // update ui
        SetScore();
        SetHealth();

        // check if all space trash has been collected
        // or if all health is gone
        if(GameObject.FindGameObjectsWithTag("SpaceTrash").Length == 0 || health == 0)
        {
            // display game over
            DisplayMessage("Game Over");

            // end game
            StopGame();
        }
    }

    // on touch input handler
    public void OnTouch() {

        // checking
        if (!IsPointerOverUI()) {
            for (int i = 0; i < Input.touchCount; ++i)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        RaycastHit hit;
                        
                        // construct a ray using touch coordinates
                        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);

                        // if hit
                        if (Physics.Raycast(ray, out hit))
                        {
                            // get hit object
                            GameObject hitobj = hit.collider.gameObject;

                            // if space trash
                            if (hitobj.CompareTag("SpaceTrash")) 
                            {
                                Renderer ren = hitobj.GetComponent<Renderer>();
                                MeshRenderer mesh = hitobj.GetComponent<MeshRenderer>();

                                // switch tag to hit
                                hitobj.tag = "HitTrash";

                                // switch emission color to green
                                mesh.material.SetColor("_EmissionColor", Color.green);

                                // bring trash to player
                                StartCoroutine(TouchedSpaceTrash(hitobj, ren));                        
                            } 

                            // if its not space trash that was touched
                            else if (!hitobj.CompareTag("HitTrash"))
                            {

                                // subtract from score
                                ChangeScore(true);

                                // flicker the object red
                                StartCoroutine(FlickerRed(hitobj));
                            }
                        }
                    }
                }
        }
    }

    private void Forward() {
        // spaceship constantly moves forward
        transform.position += transform.forward * speed * Time.deltaTime;
    }


    public void Thrust() {

        // if thrust button gets pushed
        if (playerInput.actions["Thrust"].triggered)
        {
            // start thrust coroutine
            StartCoroutine(ThrustEnum());
        }
    }

    private void CheckCam() {

        // if switching camera to back camera
        bool switching = switchCam.backCam;


        // switch camera based on boolean switching
        if (switching)
        {
            cam.tag = "Untagged";
            cam.enabled = !switching;
            camBack.enabled = switching;
            camBack.tag = "MainCamera";
        } else {
            cam.tag = "MainCamera";
            cam.enabled = !switching;
            camBack.enabled = switching;
            camBack.tag = "Untagged";
        }
    }

    public void Rotate() {
        // get input vector 2D from joystick
        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();

        // calculate rotation vector for rotation of spaceship
        Vector3 newRotation = new Vector3(-input.y * pitch * Time.deltaTime, input.x * yaw * Time.deltaTime, -input.x * roll * Time.deltaTime);

        // rotate spaceship
        transform.Rotate(newRotation);

    }

    public void Crash()
    {
        // crash explosion
        GameObject obj = Instantiate(explosion, transform.position, transform.rotation) as GameObject;

        // vibrate
        Handheld.Vibrate();
    }

    public void Respawn(float offset = originOffset)
    {
        // temporarily halt player movement and camera 
        StartCoroutine(RespawnWait(offset));
    }

    // function to send player back to a respawn location that is further away from where 
    // they collided with a planet, moon, or satellite
    // respawn facing the planet / originpoint
    public void ResetSpawnPoint(float offset) 
    {
        // set the position of the gameobject to current position * offset
        gameObject.transform.position = gameObject.transform.position * offset;

        // set rotation to vector facing the world origin
        originRotation = originPoint - gameObject.transform.position;
        gameObject.transform.forward = (originRotation);

        // reset camera
        cam.GetComponent<CameraController>().Reset();
    }

    // change score by default amount 100
    public void ChangeScore(bool lose, float amount = 100) 
    {

        // if lose, decrement score
        if (lose) 
        {
            score -= amount;
        } 

        // if win, add score
        else 
        {
            score += amount;
        }

        // show points lost
        StartCoroutine(Points(lose));
    }

    // set score text
    private void SetScore()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    // set health bar
    private void SetHealth()
    {
        StartCoroutine(healthBarEase(healthBar.fillAmount));
    }

    // set countdown
    private void SetTime()
    {
        currentTime -= 1 * Time.deltaTime;
        timeText.text = (currentTime).ToString() + "s";
    }

    // end game, remove all controls except reset
    private void StopGame()
    {
        enabled = false;
        joystick.gameObject.SetActive(false);
        throttle.enabled = false;
        backCam.enabled = false;
        throttleParent.enabled = false;
    }

    // check if pointer is over ui element
    private bool IsPointerOverUI() {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // coroutine for when space trash is touched
    IEnumerator TouchedSpaceTrash(GameObject hitobj, Renderer ren)
    {
        // beam space trash to spaceship
        while (hitobj.activeSelf) {
            hitobj.transform.position = Vector3.MoveTowards(hitobj.transform.position, transform.position, 400 * Time.deltaTime);

            yield return null;
        }         

    }

    IEnumerator ThrustEnum() 
    {
        // make smooth time for camera smaller
        cam.GetComponent<CameraController>().smoothTime = smoothTime / 2;

        // disable throttle action
        playerInput.actions["Thrust"].Disable();

        // change fill amount on button and transform position
        for (int i = 100; i > 0; i--) {
            throttle.fillAmount = (100 - i) * (1f / 100f);
            transform.position += transform.forward * thrust * Time.deltaTime;
            yield return null;
        }

        // revert smooth time to og
        cam.GetComponent<CameraController>().smoothTime = smoothTime;

        // reenable throttle
        playerInput.actions["Thrust"].Enable();
    }

    // respawn flicker animation
    IEnumerator Flicker() 
    {
        // get mesh
        MeshRenderer ren = gameObject.GetComponent<MeshRenderer>();

        // flicker mesh on and off between 0.45s
        for (int i = 18; i > 0; i--) {
            ren.enabled = !ren.enabled;

            yield return new WaitForSeconds(0.15f);
        }
        
        // enable mesh at end
        ren.enabled = true;
    }

    // flicker message 
    IEnumerator FlickerMessage(string msg) 
    {

        // enable message
        message.enabled = true;

        // change text
        message.text = msg;

        // flicker on and off
        for (int i = 19; i > 0; i--) {
            message.enabled = !message.enabled;

            yield return new WaitForSeconds(0.1f);
        }
        
        yield return new WaitForSeconds(0.1f);

        // disable message
        message.enabled = false;
    }

    // display message
    void DisplayMessage(string msg)
    {

        // set text
        message.text = msg;

        // enable message
        message.enabled = true;
    }

    // wait 3 seconds after respawn
    IEnumerator RespawnWait(float offset) {

        // collider
        Collider coll = gameObject.GetComponent<Collider>();
        Renderer ren = gameObject.GetComponent<Renderer>();

        // switch off collider, disable move controls
        
        // cam.GetComponent<CameraController>().smoothTime = 0;
        ren.enabled=false;
        playerInput.actions["Move"].Disable();
        speed = 0;
        thrust = 0;

        coll.enabled = false;        

        yield return new WaitForSeconds(0.5f);

        // reset spawn
        ResetSpawnPoint(offset);
        ren.enabled = true;

        // flicker
        StartCoroutine(Flicker());

        yield return new WaitForSeconds(3);

        // enable collider, enable move controls, and switch all values back to their original value
        coll.enabled = true;
        // cam.GetComponent<CameraController>().smoothTime = smoothTime;
        playerInput.actions["Move"].Enable();
        speed = SPEED;
        thrust = THRUST;

    }

    // ease health bar
    IEnumerator healthBarEase(float oldHealth) {

        float duration = 0.2f;
        float displayValue = 0f; // value during animation
        float timer = 0f;

        // lerp fill amount of health bar
        while (timer < duration) {
            displayValue = Mathf.Lerp(oldHealth, health * 0.2f, timer / duration);
            healthBar.fillAmount = displayValue;
            timer += Time.deltaTime;

            yield return null;
        }


    }

    public IEnumerator FlickerRed(GameObject obj) {
        // get collider, renderer, and original color
        MeshRenderer ren = obj.GetComponent<MeshRenderer>();
        Collider coll = obj.GetComponent<Collider>();
        Color originalColor = ren.material.color;

        // disable collider temporarily
        coll.enabled = false;

        // flicker red
        ren.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        // set back to og color and renable collider
        ren.material.color = originalColor;
        coll.enabled = true;

    }

    // show points popup
    IEnumerator Points(bool negative) {

        // show negative points popup
        if (negative) {
            GameObject newPoints = Instantiate(pointsNegative, canvas.transform) as GameObject;

            yield return new WaitForSeconds(0.5f);

            Destroy(newPoints);

        } 
        
        // show positive points popup
        else {
            GameObject newPoints = Instantiate(pointsPositive, canvas.transform) as GameObject;

            yield return new WaitForSeconds(0.5f);

            Destroy(newPoints);
        }
    }


}
