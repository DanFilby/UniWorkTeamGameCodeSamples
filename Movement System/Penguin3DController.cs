using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Cinemachine;
using UnityEditor.Rendering;
using System;
using System.Linq.Expressions;

public class Penguin3DController : MonoBehaviour
{
    [Header("UI References")]
    public TMPro.TextMeshProUGUI sensitivityText;
    public TMPro.TextMeshProUGUI InvertControlsText;
    public TMPro.TextMeshProUGUI ControlMethodText;
    public Slider MouseSensitivitySlider;
    public Image DashIcon;
    
    [Header("References")]
    public Transform penguinBody;
    private Rigidbody rb;
    public Animator penguinAnimator;
    public AudioManager audioManager;
    public FishShaderController fishShaderController;

    [Header("Controls")]
    public int deafaultControlMethod = 0;
    private int maxControlMethods = 2;

    [HideInInspector]
    public int currentControlMethod = 0;

    private delegate Vector3 MovementInput(bool dashing);
    private MovementInput movementInput;

    [Header("Cameras")]
    public PlayerCameras cameras;
    private CinemachineVirtualCamera activeCamera;

    public float swimmingCamDistance = 5.0f;
    public float swimmingCamMaxHeight = 4.9f;
    private float currentSwimCamHeight = 0;

    private float cachedSwimCamXSpeed;

    [Header("Stats")]
    public float swimSpeed = 10.0f;
    public float walkSpeed = 1.0f;
    public float walkTurnSpeed = 5.0f;
    private float sensitivity = 1.0f;
    private float invertYInput = -1.0f;

    public float jumpCooldownTime = 0.1f;
    private float prevJumpTime = 0.0f;
    private float collideWaterTime = 0.0f;

    [Header("Swimming Sprint")]
    //increase penguins speed, gradually by holding shift
    public float maxSprintMultiplier;
    public float sprintAcceleration;
    private float currentSprintModifier = 0.0f;

    [Header("Swimming Dash")]
    public float swimDashForce;
    public float swimDashCooldowntime = 0.5f;
    private float lastSwimDashTime = 0.0f;

    [Header("Fish Targetting")]
    public Transform targetFishHitBox;
    private Transform currentTargetFish;
    private bool lookingAtTargetFish;

    public float targettingYMin = 2.0f;
    public float targettingOffsetMax;
    private Vector2 currentTargettingOffset;

    private Action ResetFishShaderCallback;

    [HideInInspector]
    public bool swimming = false;
    public bool walking = false;

    [HideInInspector]
    public bool MovementRestricted = false;

    [HideInInspector]
    //scripts can listen for when player enters or exits water
    public static UnityAction<bool> PlayerEnteredWater;

    void Start()
    {
        //Get audio manager
        audioManager = FindObjectOfType<AudioManager>();
        //setup cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //setup cameras
        cameras.Setup();
        activeCamera = cameras.SetActiveCamera(0);
        currentTargettingOffset = new Vector2();
        cachedSwimCamXSpeed = cameras.SwimCam1_KeyboardMain.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.m_MaxSpeed;

        //setup rigidbody
        rb = GetComponent<Rigidbody>();   
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        //setup controls
        currentControlMethod = deafaultControlMethod;
        SetControls(false, deafaultControlMethod);

        //penguinAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!MovementRestricted) {
            if (swimming) {
                //animation and audio
                walking = false;
                audioManager.Stop("SnowCrunch");
                penguinAnimator.SetBool("Walking", true);

                //start targetting or stop when using right mouse button 
                if (Input.GetMouseButtonDown(1)) { StartTargetting(); }
                if (Input.GetMouseButtonUp(1)) { StopTargetting(); }
                
                Swim();            
            }
            else {
                Walk();
            }

            //jump and dash
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1")) {
                if (swimming && Time.time - lastSwimDashTime > swimDashCooldowntime) {
                    //dash in swim direction
                    StartCoroutine(SwimDash());
                    lastSwimDashTime = Time.time;
                }
                else if (!swimming && Time.time - prevJumpTime > jumpCooldownTime) {
                    //jump
                    rb.AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
                    prevJumpTime = Time.time;
                }
            }
        }


    }

    private void Walk()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        //when moving rotate penguin towards camera view direction
        if (input.magnitude > 0.2) {

            //if just started walking
            if (!walking) {
                walking = true;
                audioManager.Play("SnowCrunch");
                penguinAnimator.SetBool("Walking", true);
            }

            //calculate angle facing from camera direction
            Vector3 offset = transform.position - activeCamera.transform.position;
            float trig = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
            Vector3 newRot = new Vector3(0, trig, 0);
            penguinBody.rotation = Quaternion.Lerp(penguinBody.rotation, Quaternion.Euler(newRot), 0.2f);

        }
        else if(input.magnitude <= 0.2) {

            //if just stopped walking
            if (walking) {
                walking = false;
                audioManager.Stop("SnowCrunch");
                penguinAnimator.SetBool("Walking", false);
            }
        }

        //input & movement
        Vector3 walkMovement = penguinBody.forward * input.y + penguinBody.right * input.x;
        transform.position += walkMovement * walkSpeed * Time.deltaTime;

        //look up and down
        Vector3 camFollowOffset = activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset;
        camFollowOffset.y += -Input.GetAxis("Mouse Y") * Time.deltaTime * 5.0f;
        camFollowOffset.y = Mathf.Clamp(camFollowOffset.y, 0, 2);
        activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset = camFollowOffset;
    }


    private void Walk2()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        penguinAnimator.SetBool("Walking", (input.magnitude > 0) ? true : false);

        Vector3 walkMovement = penguinBody.forward * input.y;
        transform.position += walkMovement * walkSpeed * Time.deltaTime;

        penguinBody.Rotate(Vector3.up, input.x * Time.deltaTime * walkTurnSpeed * 100);

        //look up and down
        Vector3 camFollowOffset = activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset;
        camFollowOffset.y += -Input.GetAxis("Mouse Y") * Time.deltaTime * 5.0f;
        camFollowOffset.y = Mathf.Clamp(camFollowOffset.y, 0, 2);
        activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset = camFollowOffset;
    }


    //called each frame when swimming
    private void Swim()
    {
        Vector3 moveInput;

        if (lookingAtTargetFish) {
            //look towards target and swim towards target
            moveInput = SwimTowardsTarget();
        }
        else {
            //get regular swimming input
            moveInput = movementInput.Invoke(Input.GetKey(KeyCode.LeftShift));
        }

        //if holding shift add to the current speed multiplier otherwise slowly take it back to zero
        currentSprintModifier += (Input.GetKey(KeyCode.LeftShift)? sprintAcceleration : -1.0f) * Time.deltaTime;
        currentSprintModifier = Mathf.Clamp(currentSprintModifier, 0.0f, maxSprintMultiplier);

        rb.AddForce(moveInput * swimSpeed * 10.0f * Time.deltaTime);
    }

    private Vector3 SwimInputKeyboardMain(bool dashing)
    {
        //take input for moving the penguin
        Vector3 moveInput = penguinBody.up * Input.GetAxis("Vertical");
        moveInput += penguinBody.right * Input.GetAxis("Horizontal");
        moveInput.Normalize();

        //add dash speed
        moveInput += penguinBody.up * currentSprintModifier;

        //look towards camera x axis
        if (moveInput.magnitude > 0.1) {
            Vector3 offset = transform.position - activeCamera.transform.position;

            penguinBody.LookAt(transform.position + offset.normalized);
            penguinBody.Rotate(Vector3.right, 105, Space.Self);
        }

        //move camera up and down with mouse
        currentSwimCamHeight += Input.GetAxis("Mouse Y") * Time.deltaTime * 30.0f * sensitivity * invertYInput; 
        currentSwimCamHeight = Mathf.Clamp(currentSwimCamHeight, -swimmingCamMaxHeight, swimmingCamMaxHeight);

        //calculate camera offset vector 
        float camY = currentSwimCamHeight;
        float camZ = Mathf.Sqrt(swimmingCamDistance * swimmingCamDistance - camY * camY);
        Vector3 camFollowOffset = new Vector3(0, camY, camZ);

        activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset = camFollowOffset;

        return moveInput;
    }
    

    private Vector3 ControllerMain(bool dashing)
    {
        //take input for moving the penguin
        Vector3 moveInput = penguinBody.up * Input.GetAxis("Vertical") + penguinBody.right * Input.GetAxis("Horizontal");
        moveInput.Normalize();

        //add dash speed
        moveInput += penguinBody.up * currentSprintModifier;

        //look where the camera is pointing, and orientate the penguin to be flat
        if (moveInput.magnitude > 0.1) {
            Vector3 offset = transform.position - activeCamera.transform.position;

            penguinBody.LookAt(transform.position + offset.normalized);
            penguinBody.Rotate(Vector3.right, 90, Space.Self);
        }

        //move camera up and down with mouse
        currentSwimCamHeight += Input.GetAxis("RightStick Y") * Time.deltaTime * 12.0f * sensitivity * invertYInput;
        currentSwimCamHeight = Mathf.Clamp(currentSwimCamHeight, -swimmingCamMaxHeight, swimmingCamMaxHeight);

        //calculate camera pos with the hieght
        activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset = CalculateCameraOffset(currentSwimCamHeight);

        return moveInput;
    }

    private Vector3 SwimTowardsTarget()
    {
        //check if caught target
        if(currentTargetFish == null) {
            StopTargetting();
            return new Vector3();
        }

        //take input for moving the penguin. always move forward when targetting
        Vector3 moveInput = penguinBody.up + penguinBody.right * Input.GetAxis("Horizontal");
        moveInput.Normalize();

        //add dash speed
        moveInput += penguinBody.up * currentSprintModifier;

        //player can add slight offset from the target
        currentTargettingOffset.y += -Input.GetAxis("Mouse Y") * Time.deltaTime * 30.0f * sensitivity * invertYInput;
        currentTargettingOffset.x += Input.GetAxis("Mouse X") * Time.deltaTime * 30.0f * sensitivity;
        currentTargettingOffset.y = Mathf.Clamp(currentTargettingOffset.y, -targettingOffsetMax, targettingOffsetMax);
        currentTargettingOffset.x = Mathf.Clamp(currentTargettingOffset.x, -targettingOffsetMax, targettingOffsetMax);

        //look at target and rotate so penguin's flat
        penguinBody.LookAt(currentTargetFish);
        penguinBody.Rotate(Vector3.right, 90, Space.Self);

        //calculate camera angles
        Vector3 offsetToTarget = currentTargetFish.position - transform.position;
        float angleDifY = Mathf.Atan2(offsetToTarget.y, offsetToTarget.z) + Mathf.Deg2Rad * currentTargettingOffset.y;
        float angleDifX = Mathf.Atan2(offsetToTarget.x, offsetToTarget.z) + Mathf.Deg2Rad * currentTargettingOffset.x;

        //use a bit of trig to find the height of the camera
        float camY = Mathf.Clamp(Mathf.Sin(angleDifY) * -swimmingCamDistance, -2, swimmingCamMaxHeight);

        //manually set the cameras position
        activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset = CalculateCameraOffset(camY);
        activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.Value = Mathf.Rad2Deg * angleDifX ;

        return moveInput;
    }

    private Vector3 CalculateCameraOffset(float height)
    {
        float camZ = Mathf.Sqrt(swimmingCamDistance * swimmingCamDistance - height * height);
        return new Vector3(0, height, camZ);
    }

    private void StartTargetting()
    {
        //find all colliders in the hit box
        Collider[] hits = Physics.OverlapBox(targetFishHitBox.position, targetFishHitBox.lossyScale / 2, targetFishHitBox.rotation);

        float closestFishDist = 1000;
        GameObject closestFishObj = null;

        //find closest fish in the hitbox
        foreach (var collider in hits) {
            if(collider.TryGetComponent<Fish3D>(out _)){
                //if shrimp continue, buggy
                if(collider.GetComponent<ItemController>().item.id == 9) {
                    continue;
                }

                float dist = Vector3.Distance(transform.position,collider.transform.position);

                if(dist < closestFishDist) {
                    closestFishObj = collider.gameObject;
                    closestFishDist = dist;
                }
            }
        }
        
        if(closestFishObj != null) {
            currentTargetFish = closestFishObj.transform;
            lookingAtTargetFish = true;
            activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.m_MaxSpeed = 0;

            ResetFishShaderCallback = fishShaderController.SetFishTargetted(currentTargetFish.gameObject);
        }
    }

    private void StopTargetting()
    {
        currentTargetFish = null;
        lookingAtTargetFish = false;
        activeCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.m_MaxSpeed = cachedSwimCamXSpeed * sensitivity;
        currentTargettingOffset = new Vector2();

        ResetFishShaderCallback?.Invoke();
        ResetFishShaderCallback = null;
    }

    //dash in water
    private IEnumerator SwimDash()
    {
        //dash forward
        rb.AddForce(penguinBody.up * swimDashForce * 100.0f);
        audioManager.Play("Dash");
        DashIcon.fillAmount = 1;
        WaitForSeconds delay = new WaitForSeconds(swimDashCooldowntime / 100.0f);    
      
        for (int i = 0; i < 100; i++) 
        {
            
            DashIcon.fillAmount -= 0.01f;
            yield return delay;
        }

        DashIcon.fillAmount = 0;
    }

    //called from pause menu 
    public void ChangeControls()
    {
        //change to the next control method, looping around if reached max
        currentControlMethod = (currentControlMethod + 1) % (maxControlMethods + 1);
        if(currentControlMethod == 0) { currentControlMethod++; }

        SetControls(false, currentControlMethod);     
    }

    //called by pause menu button
    public void InvertControls()
    {
        invertYInput = invertYInput * -1.0f;
        InvertControlsText.text = (invertYInput < 0) ? "ON" : "OFF";
    }

    //called by slider on value change
    public void ChangeSensitivity()
    {
        sensitivity  = MouseSensitivitySlider.value;
        cameras.SwimCam1_KeyboardMain.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XAxis.m_MaxSpeed = cachedSwimCamXSpeed * sensitivity;
        sensitivityText.text = "Sensitivity: " + System.MathF.Round(MouseSensitivitySlider.value,2).ToString();
    }

    private void SetControls(bool changeCam, int controlIndex)
    {
        if (changeCam) {
            activeCamera = cameras.SetActiveCamera(controlIndex);
        }

        switch (controlIndex) {
            case 1:
                movementInput = SwimInputKeyboardMain;
                ControlMethodText.text = "Keyboard 1";
                break;
                default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "WaterTop" && Time.time - collideWaterTime > 1) {
            swimming = !swimming;
            rb.useGravity = !swimming;

            if (swimming) {
                PlayerEntersWater();
            }
            else {
                PlayerExitsWater();
            }

            collideWaterTime = Time.time;
        }
    }

    private void PlayerEntersWater()
    {
        //call events for other scripts listening that player enetered water
        PlayerEnteredWater?.Invoke(swimming);
        audioManager.Play("JumpingInWater");
        SetControls(true, currentControlMethod);

        //temporarily disable collider and shoot player downwards into the water
        StartCoroutine(TempDisableCollider());
        rb.AddForce(Vector3.down * 800);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void PlayerExitsWater()
    {
        //set walk cam active
        activeCamera = cameras.SetActiveCamera(0);

        //diable collider and shoot player upwards
        StartCoroutine(TempDisableCollider());
        rb.AddForce(Vector3.up * 600);

        // reset rotation
        Vector3 rot = new Vector3(0, 180, 0);
        transform.rotation = Quaternion.Euler(rot);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    //diables collider for a bit, so when triggering swimming penguin can go through the ice
    IEnumerator TempDisableCollider()
    {
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(0.3f);
        GetComponent<Collider>().enabled = true;
    }
}

[System.Serializable]
public struct PlayerCameras {
    public CinemachineVirtualCamera WalkCam;
    public CinemachineVirtualCamera SwimCam1_KeyboardMain;
    public CinemachineVirtualCamera SwimCam2_ControllerMain;

    private List<CinemachineVirtualCamera> virtualCameras;

    public void Setup()
    {
        virtualCameras = new List<CinemachineVirtualCamera>() { WalkCam, SwimCam1_KeyboardMain,
            SwimCam2_ControllerMain};
    }

    public CinemachineVirtualCamera SetActiveCamera(int index)
    {
        foreach (var cam in virtualCameras) {
            cam.gameObject.SetActive(false);
        }
        virtualCameras[index].gameObject.SetActive(true);
        return virtualCameras[index];
    }

}