using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Wardrobe : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;
    public Transform designPenguinCustomisationTransform;
    private WardrobeInventory wardrobeInventory;
    public PauseMenu pauseMenuManager;
    public GameObject CanvasObj;

    [Header("UI References")]
    public GameObject WardrobeUI; 

    //raycasting to canvas
    GraphicRaycaster UIRaycaster;
    PointerEventData CursorEventData;
    EventSystem CanvasEventSystem;

    //picking up items, ui item obj
    private CustomisationItem currentUIItem;
    //holds all info for this item
    private WardrobeItem currentWardrobeItem;

    private Transform cachedItemParent;

    private List<WardrobeItem> EquippedItems;

    //flags
    private bool WardrobeEnabled = false;

    float prevSellTime = 0.0f;

    void Start()
    {
        UIRaycaster = CanvasObj.GetComponent<GraphicRaycaster>();
        CanvasEventSystem = CanvasObj.GetComponent<EventSystem>();
        wardrobeInventory = GetComponent<WardrobeInventory>();
        EquippedItems = new List<WardrobeItem>();
    }

    void Update()
    {
        if (WardrobeEnabled) {
            //if clicking on currently equipped item 
            if (Input.GetMouseButtonDown(0) && GetItemAtCursor(out CustomisationItem equippedItem)
                && CheckCustomisationZone(out CustomisationZone zone) ) {
                if(zone.CurrentItem == null ) { return; }

                currentUIItem = zone.CurrentItem;
                cachedItemParent = wardrobeInventory.UIContentTransform;
                currentUIItem.gameObject.GetComponent<RectTransform>().SetParent(WardrobeUI.transform);

                currentWardrobeItem = wardrobeInventory.GetWardrobeItem(equippedItem.Item);
            }

            //on click find the ui item obj from inventory
            else if (Input.GetMouseButtonDown(0) && GetItemAtCursor(out CustomisationItem item)) {
                currentUIItem = item;
                cachedItemParent = item.transform.parent;
                currentUIItem.gameObject.GetComponent<RectTransform>().SetParent(WardrobeUI.transform);
                currentUIItem.gameObject.GetComponent<Image>().enabled = false;
                currentUIItem.gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = false;

                currentWardrobeItem = wardrobeInventory.GetWardrobeItem(item.Item);
            }

            //if holding left click move the ui item obj to the cursor's position
            if (Input.GetMouseButton(0) && currentUIItem != null) {
                currentUIItem.gameObject.GetComponent<RectTransform>().position = Input.mousePosition;
            }

            //let go out mouse, will check if the item fits the penguin or just puts back into inv
            if (Input.GetMouseButtonUp(0) && Time.time - prevSellTime >= 0.2f && currentUIItem != null) {

                //check if over one of customisation zone, IE hats, shirts etc
                bool overCustomiseZone = CheckCustomisationZone(out CustomisationZone customisationZone);

                //if not over a customisezone or it doesn't fit put back into inv
                if (!overCustomiseZone || customisationZone.ZoneType != currentWardrobeItem.mType) {
                    //reset item back into inv
                    currentUIItem.gameObject.GetComponent<RectTransform>().SetParent(cachedItemParent);
                    currentUIItem.gameObject.GetComponent<Image>().enabled = true;
                    currentUIItem.gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = true;

                    //reset vars
                    currentUIItem = null;
                    currentWardrobeItem = null;
                    return;
                }

                //replace exsiting customisation item
                if(customisationZone.CurrentItem != null) {
                    CustomisationItem oldItem = customisationZone.CurrentItem;
                    oldItem.gameObject.GetComponent<RectTransform>().SetParent(cachedItemParent);
                    oldItem.gameObject.GetComponent<Image>().enabled = true;
                    oldItem.gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                }


                currentUIItem.gameObject.GetComponent<RectTransform>().SetParent(customisationZone.transform);
                currentUIItem.gameObject.GetComponent<RectTransform>().position = customisationZone.GetComponent<RectTransform>().position + new Vector3(0, 0, 0);

                customisationZone.CurrentItem = currentUIItem;

                EquipTempClothing(currentWardrobeItem.mItem);

                currentUIItem = null;
                currentWardrobeItem = null;
            }
        }       
    }

    //equips clothes to the wardrobe penguin and keeps list to give the player afterwards
    private void EquipTempClothing(CustomizationGift _item)
    {
        wardrobeInventory.ActivateWardrobeItem(_item, designPenguinCustomisationTransform);

        WardrobeItem wItem =  wardrobeInventory.GetWardrobeItem(_item);

        //add item to the currently equipped list, remove items of the same type
        EquippedItems.RemoveAll(x => x.mType == wItem.mType);
        EquippedItems.Add(wItem);
    }

    //returns the item at the cursor's position
    private bool GetItemAtCursor(out CustomisationItem item)
    {
        foreach (RaycastResult result in GetUIAtCursor()) {
            if (result.gameObject.TryGetComponent<CustomisationItem>(out item)) {
                return true;
            }
        }

        item = null;
        return false;
    }

    //checks if the cursor is above the sell zone
    private bool CheckCustomisationZone(out CustomisationZone customisationZone)
    {
        foreach (RaycastResult result in GetUIAtCursor()) {
            if (result.gameObject.TryGetComponent<CustomisationZone>(out customisationZone)) {
                return true;
            }
        }

        customisationZone = null;
        return false;
    }

    //returns all ui components at the cursors pos
    private List<RaycastResult> GetUIAtCursor()
    {
        CursorEventData = new PointerEventData(CanvasEventSystem);
        CursorEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();

        UIRaycaster.Raycast(CursorEventData, results);

        return results;
    }

    public void SetupWardrobe()
    {
        WardrobeEnabled = true;

        WardrobeUI.SetActive(true);
        pauseMenuManager.playerIsInUI = true;
        wardrobeInventory.ListPlayersWardrobe(WardrobeItemType.All);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void StopWardrobe()
    {
        WardrobeEnabled = false;

        WardrobeUI.SetActive(false);
        pauseMenuManager.playerIsInUI = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void CustomisationButtonClick(int cutomisationType)
    {
        Debug.Log((WardrobeItemType)cutomisationType);
        wardrobeInventory.ListPlayersWardrobe((WardrobeItemType)cutomisationType);

    }

}
