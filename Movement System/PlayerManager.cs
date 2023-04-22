using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Reflection;
using System;
using UnityEngine.Rendering;

public class PlayerManager : MonoBehaviour
{
    [Header("UI References")]
    public PauseMenu PM;
    public PickupPopUp PopUpManager;
    public TextMeshProUGUI currencyUI;
    public TextMeshProUGUI PlayerPrompt;

    [Header("References")]
    public Cinemachine.CinemachineVirtualCamera playerCamera;
    public UnderwaterBreathin underwaterBreathing;

    [Header("Inventory")]
    public InventoryManager inventoryManager;
    public int PenguinCash;

    [Header("Quest's & Dialogue")]
    [HideInInspector]public bool inDialogue = false;
    [HideInInspector]public bool inQuestScreen = false;
    [HideInInspector]public bool onQuest = false;
    public PickupChick pickupScript;

    private bool finsihsedQuest = false;
    private bool hatState = false;
    public GameObject hatReward;

    private Penguin3DController playerConrtoller;

    private Cinemachine.CinemachineVirtualCamera cachedCam;

    private void Start()
    {
        hatReward.SetActive(false);
        playerConrtoller = gameObject.GetComponent<Penguin3DController>();
    }

    void Update()
    {
        currencyUI.text = PenguinCash.ToString();
        
        if(PenguinCash == 100000)
        {
            PM.YouWinScreen();
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            ToggleHat(false);
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            PopUpInfo info = new PopUpInfo();
            info.count = 1;
            info.itemID = 2;
            PopUpManager.AddPopUp(info);
        }

        CheatCodes();
    }

    public void RestricPlayer()
    {
        playerConrtoller.MovementRestricted = true;
    }

    public void UnRestrictPlayer()
    {
        playerConrtoller.MovementRestricted = false;
    }

    public void ToggleUnderWaterBreathMeter(bool state)
    {
        underwaterBreathing.breathingEnabled = state;
    }

    //returns dict of fish caught used to check quest completion
    public Dictionary<FishType,int> FishCaught()
    {
        Dictionary<FishType, int> fishCaught = new Dictionary<FishType, int>();

        List<Items> playersInventory = inventoryManager.ItemsList;

        foreach (var item in playersInventory) {
            //if item is a fish
            if(item.itemType == Items.ItemType.Fish) {
                //get the fish type from the id map and add the ammount
                fishCaught.Add(InventoryManager.IdFishMap[item.id], item.amount);
            }
        }

        return fishCaught;
    }

    public bool InventoryContains(int itemId, int count, out int currentAmount)
    {
        List<Items> playersInventory = inventoryManager.ItemsList;

        currentAmount = 0;

        foreach (var item in playersInventory) {
            if(item.id == itemId ) {
                currentAmount = item.amount;

                if(currentAmount >= count) {
                    return true;
                }
            }
        }

        return false;
    }

    public void ToggleHat(bool firstTime)
    {
        if (firstTime) {
            finsihsedQuest = true;
        }
        if (finsihsedQuest) {
            hatState = !hatState;
            hatReward.SetActive(hatState);
        }    
    }

    private void CheatCodes()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                PenguinCash += 550;
            }
        }

    }

    public bool PlayerHoldingChick()
    {
        return pickupScript.holdingChick;
    }

    public TextMeshProUGUI GetPrompt()
    {
        return PlayerPrompt;
    }

    public Action SetActiveCamera(Cinemachine.CinemachineVirtualCamera cam)
    {
        RestricPlayer();

        cam.enabled = true;
        playerCamera.enabled = false;

        cachedCam = cam;

        return ResetCamera;
    }

    public void ResetCamera()
    {
        UnRestrictPlayer();

        cachedCam.enabled = false;
        playerCamera.enabled = true;
    }
}


