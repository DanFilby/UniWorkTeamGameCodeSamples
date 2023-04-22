using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardrobeTrigger : MonoBehaviour
{

    [Header("Cameras")]
    public CinemachineVirtualCamera WardrobeCam;
    public CinemachineVirtualCamera PlayerCam;

    [Header("References")]
    public Wardrobe WardrobeScript;
    public ConstructionFade fading;
    public Transform playerStandPosition;
    public Transform designPenguinBody;
    public Transform camLookPositon;

    bool playerInWardrobe = false;
    bool playerInRange = false;
    PlayerManager cachedPlayerManager;
    Action cachedCamResetCallBack;

    private Quaternion cachedPlayerEnterRot;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerInRange && !playerInWardrobe) {
            if (!playerInWardrobe) {
                EnterWardrobe();
            }
        }
        else if (playerInWardrobe && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))) {
            ExitWardrobe();
        }

        if (playerInWardrobe) {
            designPenguinBody.Rotate(Vector3.up, Input.GetAxis("Horizontal") * Time.deltaTime * 80.0f);
        }

    }

    private IEnumerator EnterCinematic()
    {
        cachedPlayerManager.gameObject.SetActive(false);

        //fade screen to black, so can teleport player model out of view
        fading.FadeInUI();

        //check each frame if finshed fading
        WaitForEndOfFrame endOfFrameDelay = new WaitForEndOfFrame();
        while (fading.fadeIn) {
            yield return endOfFrameDelay;
        }

        designPenguinBody.gameObject.SetActive(true);
        cachedCamResetCallBack = cachedPlayerManager.SetActiveCamera(WardrobeCam);

        //check each frame if finshed fading
        fading.FadeOutUI();
        while (fading.fadeOut) {
            yield return endOfFrameDelay;
        }

        playerInWardrobe = true;
        WardrobeScript.SetupWardrobe();
    }


    private IEnumerator ExitCinematic()
    {
        designPenguinBody.gameObject.SetActive(false);
        playerInWardrobe = false;
        WardrobeScript.StopWardrobe();

        fading.FadeInUI();

        //check each frame if finshed fading
        WaitForEndOfFrame endOfFrameDelay = new WaitForEndOfFrame();
        while (fading.fadeIn) {
            yield return endOfFrameDelay;
        }

        cachedPlayerManager.gameObject.SetActive(true);
        cachedCamResetCallBack.Invoke();

        //check each frame if finshed fading
        fading.FadeOutUI();
        while (fading.fadeOut) {
            yield return endOfFrameDelay;
        }
    }

    private void EnterWardrobe()
    {        
        StartCoroutine(EnterCinematic());        
    }

    private void ExitWardrobe()
    {
        StartCoroutine(ExitCinematic());      
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerManager>(out PlayerManager player)) {
            player.GetPrompt().text = "E: Enter Player Wardrobe";
            player.GetPrompt().enabled = true;
            cachedPlayerManager = player;
            playerInRange = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerManager>(out PlayerManager player)) {
            playerInRange = false;
            player.GetPrompt().enabled = false;
            cachedPlayerManager = null;
        }
    }
}
