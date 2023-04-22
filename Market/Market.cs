using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class Market : MonoBehaviour
{
    enum MarketState { Buying, Selling, Browsing };

    [Header("References")]
    public PlayerManager playerManager;
    public InventoryManager inventoryManager;
    public Inventoryreset inventoryreset;
    public PauseMenu pauseMenuManager;
    public GameObject playerUI;
    public WardrobeInventory wardrobeInventory;

    [Header("UI References")]
    public GameObject CanvasObj;
    public GameObject MarketUI;
    public Transform ItemContentList;
    public GameObject ItemIconPrefab;
    public TextMeshProUGUI playersCashText;

    [Header("Selling")]
    public GameObject SellPanel;
    public TMPro.TextMeshProUGUI SellAmountText;
    public TMPro.TextMeshProUGUI CashAmountText;
    private int sellamount;

    [Header("Buying")]
    public GameObject BuyPanel;
    public TMPro.TextMeshProUGUI BuyCostText;
    private List<WardrobeItem> marketWardrobeItems;

    MarketItem CurrentItem;

    //raycasting to canvas
    GraphicRaycaster UIRaycaster;
    PointerEventData CursorEventData;
    EventSystem CanvasEventSystem;

    private MarketState currentShopState = MarketState.Browsing;
    private bool marketEnabled = false;

    void Start()
    {
        UIRaycaster = CanvasObj.GetComponent<GraphicRaycaster>();
        CanvasEventSystem = CanvasObj.GetComponent<EventSystem>();
        SetupSellersInventory();
    }

    void Update()
    {
        if (marketEnabled) {

            //on click find the ui item obj 
            if (Input.GetMouseButtonDown(0) && GetItemAtCursor(out MarketItem item)) {

                ClickedOnItem(item);
            }

            playersCashText.text = playerManager.PenguinCash.ToString();
        }
    }

    private void ClickedOnItem(MarketItem item)
    {
        CurrentItem = item;
        if (item.owner == MarketItem.ItemOwner.Player) {
            currentShopState = MarketState.Selling;

            //setup selling panel
            SellPanel.SetActive(true);
            SellPanel.transform.Find("Item-Icon").GetComponent<Image>().sprite = item.itemIcon;
            SellPanel.transform.Find("Item-Icon").transform.Find("Item-Amount").GetComponent<TextMeshProUGUI>().text = item.itemAmount.ToString();

            //reset amount and text
            sellamount = 0;
            SellAmountText.text = sellamount.ToString();
            CashAmountText.text = (sellamount * CurrentItem.itemCost).ToString();

        }
        else if (item.owner == MarketItem.ItemOwner.MarketSeller) {
            currentShopState = MarketState.Buying;

            //setup buying panel
            BuyPanel.SetActive(true);
            BuyPanel.transform.Find("Item-Icon").GetComponent<Image>().sprite = item.itemIcon;
            BuyPanel.transform.Find("Item-Name").GetComponent<TextMeshProUGUI>().text = item.itemName;
            BuyPanel.transform.Find("Item-Cost").GetComponent<TextMeshProUGUI>().text = item.itemCost.ToString();

            BuyCostText.text = CurrentItem.itemCost.ToString();

        }

    }

    public void Button_IncreaseSellAmount()
    {
        sellamount = Mathf.Clamp(++sellamount, 0, CurrentItem.itemAmount);
        SellAmountText.text = sellamount.ToString();
        CashAmountText.text = (sellamount * CurrentItem.itemCost).ToString();
    }
    public void Button_DecreaseSellAmount()
    {
        sellamount = Mathf.Clamp(--sellamount, 0, CurrentItem.itemAmount);
        SellAmountText.text = sellamount.ToString();
        CashAmountText.text = (sellamount * CurrentItem.itemCost).ToString();
    }

    public void Button_MaxSell()
    {
        sellamount = CurrentItem.itemAmount;
        SellAmountText.text = sellamount.ToString();
        CashAmountText.text = (sellamount * CurrentItem.itemCost).ToString();
    }

    public void Button_CancelSell()
    {
        SellPanel.SetActive(false);
        currentShopState = MarketState.Browsing;
    }

    public void Button_Sell() 
    {
        //dont sell if at 0
        if(sellamount <= 0) {
            return;
        }

        //actually sell item and add cash to player
        playerManager.PenguinCash += sellamount * CurrentItem.itemCost;
        inventoryManager.RemoveItems(CurrentItem.itemId, sellamount);
        
        //reset inventory
        ShowPlayersItems();
        CurrentItem = null;

        //reset ui
        currentShopState = MarketState.Browsing;
        SellPanel.SetActive(false);
    }

    public void Button_CancelBuy()
    {
        BuyPanel.SetActive(false);
        currentShopState = MarketState.Browsing;
    }

    public void Button_Buy()
    {
        //have enough money to buy
        if (playerManager.PenguinCash >= CurrentItem.itemCost) {
            //deduct cash and add items to inventory
            playerManager.PenguinCash -= CurrentItem.itemCost;

            RemoveSellersItem(CurrentItem);

            wardrobeInventory.AddWardrobeItemToPlayer(CurrentItem.customisationItemType);

        }
        //reset sellers inventory
        ShowMarketItems();
        CurrentItem = null;

        //reset ui
        currentShopState = MarketState.Browsing;
        BuyPanel.SetActive(false);
    }


    public void SetupMarket()
    {
        marketEnabled = true;
        playerUI.SetActive(false);
        MarketUI.SetActive(true);
        pauseMenuManager.playerIsInUI = true;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        ShowPlayersItems();
        currentShopState = MarketState.Browsing;
    }

    public void StopMarket()
    {
        marketEnabled = false;
        playerUI.SetActive(true);
        MarketUI.SetActive(false);
        SellPanel.SetActive(false);
        BuyPanel.SetActive(false);
        pauseMenuManager.playerIsInUI = false;
    }


    public void ShowPlayersItems()
    {
        //clean content before open
        foreach (Transform item in ItemContentList) {
            Destroy(item.gameObject);
        }

        //add items into inventory
        foreach (var items in inventoryManager.ItemsList) {
            GameObject UIItemTile = Instantiate(ItemIconPrefab, ItemContentList);
            UIItemTile.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = items.itemName;
            UIItemTile.transform.Find("ItemIcon").GetComponent<Image>().sprite = items.icon;
            UIItemTile.transform.Find("Amount").GetComponent<TextMeshProUGUI>().text = items.amount.ToString();
            UIItemTile.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = items.value.ToString();

            //class on each ui item obj that the makret finds
            MarketItem marketItem = UIItemTile.GetComponent<MarketItem>();
            marketItem.Setup(items.id, items.itemName, items.amount, items.value, items.icon, MarketItem.ItemOwner.Player);
        }

    }

    public void ShowMarketItems()
    {
        //clean content before open
        foreach (Transform item in ItemContentList) {
            Destroy(item.gameObject);
        }

        foreach (WardrobeItem customisationItem in marketWardrobeItems) {
            GameObject UIItemTile = Instantiate(ItemIconPrefab, ItemContentList);
            UIItemTile.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = customisationItem.TileName;
            UIItemTile.transform.Find("ItemIcon").GetComponent<Image>().sprite = customisationItem.TileIcon;
            UIItemTile.transform.Find("Amount").GetComponent<TextMeshProUGUI>().enabled = false;
            UIItemTile.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = customisationItem.mCost.ToString();

            MarketItem marketItem = UIItemTile.GetComponent<MarketItem>();
            marketItem.Setup(0, customisationItem.TileName, 1, customisationItem.mCost, customisationItem.TileIcon, MarketItem.ItemOwner.MarketSeller);
            marketItem.customisationItemType = customisationItem.mItem;
        }


    }

    private void SetupSellersInventory()
    {
        marketWardrobeItems = new List<WardrobeItem>();

        //skip first item as it's null
        for (int i = 1; i < wardrobeInventory.AllWardrobeItems.Count; i++) {
            marketWardrobeItems.Add((WardrobeItem)wardrobeInventory.AllWardrobeItems[i].Clone());
        }

    }

    private void RemoveSellersItem(MarketItem item)
    {
        //remove customisation items
        int removeIndex = -1;
        for (int i = 0; i < marketWardrobeItems.Count; i++) {
            if (marketWardrobeItems[i].mItem == item.customisationItemType) {
                removeIndex = i;
            }
        }

        if(removeIndex != -1) {
            marketWardrobeItems.RemoveAt(removeIndex);
        }
    }

    private bool GetItemAtCursor(out MarketItem item)
    {
        foreach (RaycastResult result in GetUIAtCursor()) {
            if (result.gameObject.TryGetComponent<MarketItem>(out item)) {
                return true;
            }
        }

        item = null;
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
}
