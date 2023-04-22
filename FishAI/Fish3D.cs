using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish3D : MonoBehaviour
{
    [Header("References")]
    public GameObject Player;

    [Header("Fish Stats")]
    public FishType fishType;
    public float cruiseSpeed;
    public float avoidPlayerSpeed;
    public float avoidPlayerDist = 5.0f;

    Vector3 direction;
    Vector3 swimDirection;
    Vector3 toKeepInbounds;
    Vector3 avoidPlayerDir;

    //fish stay inside box
    public Transform BoundingTopLeftFront;
    public Transform BoundingBottomRightBack;

    private bool avoidingFromPlayer = false;

    //when true fish seek player
    private bool FishDebuging = false;

    public bool debugLog = false;

    void Start()
    {
        direction = new Vector3(1 - 2 * Random.value, 1 - 2 * Random.value, 1 - 2 * Random.value);
        direction.Normalize();

        swimDirection = direction;

        toKeepInbounds = new Vector3(0, 0);
    }

    void Update()
    { 
        KeepInBounds();
        AvoidPlayer();

        CalculateSwimDirection();

        transform.Translate(swimDirection * (avoidingFromPlayer? avoidPlayerSpeed : cruiseSpeed) * Time.deltaTime, Space.World);

        //rotate fish to look in the direction it swims
        ChangeSwimDirection();


        //cheat - fish seek player
        if (Input.GetKey(KeyCode.Z) && Input.GetKeyDown(KeyCode.Alpha2)) { FishDebuging = !FishDebuging; }
    }

    private void CalculateSwimDirection()
    {
        Vector3 wantedDirection = direction;

        if (toKeepInbounds.y != 0) {
            wantedDirection.y = swimDirection.y * Mathf.Sign(swimDirection.y) * toKeepInbounds.y;
        }
        if (toKeepInbounds.x != 0) {
            wantedDirection.x = swimDirection.x * Mathf.Sign(swimDirection.x) * toKeepInbounds.x;
        }
        if (toKeepInbounds.z != 0) {
            wantedDirection.z = swimDirection.z * Mathf.Sign(swimDirection.z) * toKeepInbounds.z;
        }

        if (avoidingFromPlayer) {
            swimDirection = swimDirection = Vector3.Lerp(swimDirection, (avoidPlayerDir * 0.5f) + (wantedDirection * 0.5f), 0.2f);
        }
        else {
            swimDirection = Vector3.Lerp(swimDirection, wantedDirection, 0.6f);
        }

        swimDirection.Normalize();
        direction = swimDirection;
    }

    private void AvoidPlayer()
    {
        if (Player != null) {
            //if player is within detection range
            Vector3 playerOffset = transform.position - Player.transform.position;
            if (playerOffset.magnitude < avoidPlayerDist * (FishDebuging? 5.0f : 1.0f)) {

                //vector away from the player normalised
                avoidPlayerDir = playerOffset.normalized * (FishDebuging ? -1.0f : 1.0f);

                avoidingFromPlayer = true;
            }
            else {
                avoidingFromPlayer = false;
            }
        }
    }

    //rotate to look in swim direction 
    private void ChangeSwimDirection()
    {
        transform.LookAt(transform.position + swimDirection);

        transform.Rotate(new Vector3(0, 1, 0), 90, Space.Self);
        transform.Rotate(new Vector3(1, 0, 0), -90, Space.Self);
    }


    //adds the direction to swim away from boundaries to tokeepinbounds vector
    private void KeepInBounds()
    {
        Vector3 pos = transform.position;

        if (pos.y > BoundingTopLeftFront.position.y) {
            toKeepInbounds.y = -1;
        }
        else if (pos.y < BoundingBottomRightBack.position.y) {
            toKeepInbounds.y = 1;
        }

        if (pos.x < BoundingTopLeftFront.position.x) {
            toKeepInbounds.x = 1;
        }
        else if (pos.x > BoundingBottomRightBack.position.x) {
            toKeepInbounds.x = -1;
        }

        if (pos.z > BoundingTopLeftFront.position.z) {
            toKeepInbounds.z = -1;
        }
        else if (pos.z < BoundingBottomRightBack.position.z) {
            toKeepInbounds.z = 1;
        }  
    }
}
