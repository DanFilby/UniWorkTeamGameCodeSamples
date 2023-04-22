using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum WardrobeItemType {All = 0, Hat = 1, Shirts = 2 }

public enum CustomizationGift { None = 0, PinkTopHat = 1, QuestHat = 2, Crown = 3, Coral = 4, Fish = 5, Barrel = 6}

public class WardrobeInventory : MonoBehaviour
{
    [NonReorderable]
    public List<WardrobeItem> AllWardrobeItems;

    [HideInInspector]
    public List<WardrobeItem> playersWardrobe;

    private List<CustomisationItem> currentlyEquippedItems;

    public Transform playersCustomisationParent;

    [Header("UI References")]
    public Transform UIContentTransform;
    public GameObject WardrobeItemTilePrefab;


    void Start()
    {
        currentlyEquippedItems = new List<CustomisationItem>();

        //playersWardrobe.Add(GetWardrobeItem(CustomizationGift.PinkTopHat));
        //playersWardrobe.Add(GetWardrobeItem(CustomizationGift.Crown));
        //playersWardrobe.Add(GetWardrobeItem(CustomizationGift.Fish));
        //playersWardrobe.Add(GetWardrobeItem(CustomizationGift.Coral));
        //playersWardrobe.Add(GetWardrobeItem(CustomizationGift.QuestHat));
        playersWardrobe.Add(GetWardrobeItem(CustomizationGift.Barrel));
    }

    void Update()
    {
        
    }

    public void AddWardrobeItemToPlayer(CustomizationGift customisationItem)
    {
        playersWardrobe.Add(GetWardrobeItem(customisationItem));

    }

    public void ListPlayersWardrobe(WardrobeItemType customisationType)
    {
        //clean content before open
        foreach (Transform item in UIContentTransform) {
            Destroy(item.gameObject);
        }

        foreach (var item in playersWardrobe) {
            if(customisationType != WardrobeItemType.All && item.mType != customisationType) {
                continue;
            }

            GameObject obj = Instantiate(WardrobeItemTilePrefab, UIContentTransform);
            obj.GetComponentsInChildren<Image>()[1].sprite = item.TileIcon;
            obj.GetComponentInChildren<TextMeshProUGUI>().text = item.TileName;

            obj.GetComponent<CustomisationItem>().Item = item.mItem;
        }

    }

    public void ListPlayersWardrobe()
    {
        //clean content before open
        foreach (Transform item in UIContentTransform) {
            Destroy(item.gameObject);
        }

        foreach (var item in playersWardrobe) {
            GameObject obj = Instantiate(WardrobeItemTilePrefab, UIContentTransform);
            obj.GetComponentsInChildren<Image>()[1].sprite = item.TileIcon;
            obj.GetComponentInChildren<TextMeshProUGUI>().text = item.TileName;

            obj.GetComponent<CustomisationItem>().Item = item.mItem;

        }

    }

    public void EquipItem(CustomizationGift item)
    {
        ActivateWardrobeItem(item, playersCustomisationParent);
    }

    public void ActivateWardrobeItem(CustomizationGift item, Transform penguinTransform)
    {
        //ward item holds all detail needed to spawn and position item
        WardrobeItem wardItem = GetWardrobeItem(item);

        //check if there is already an item with the same type equipped
        CustomisationItem equipped = null;
        foreach (CustomisationItem equippedItem in currentlyEquippedItems) {
            if(equippedItem.ItemType == wardItem.mType) {
                equipped = equippedItem;
            }
        }
        //remove equipped item and destroy it in scene
        if(equipped != null) {
            currentlyEquippedItems.Remove(equipped);
            Destroy(equipped.gameObject);
        }

        //cust item is the actual in scene customisation item
        GameObject custItem =  Instantiate(wardItem.mPrefab, penguinTransform);
        CustomisationItem itemIdentifier = custItem.AddComponent<CustomisationItem>();

        //add item to equipped list
        itemIdentifier.ItemType = wardItem.mType;
        currentlyEquippedItems.Add(itemIdentifier);

        //set items pos, rot and scale
        custItem.transform.localPosition = wardItem.mPos;
        custItem.transform.localRotation = Quaternion.Euler(wardItem.mRot);
        custItem.transform.localScale = wardItem.mScale;
    }

    public WardrobeItem GetWardrobeItem(CustomizationGift targetItem)
    {
        foreach (WardrobeItem item in AllWardrobeItems) {
            if(item.ID == (int)targetItem) {
                return (WardrobeItem)item.Clone();
            }
        }
        return null;
    }



}

//contians all info for use in scene and other sorting info
[System.Serializable]
public class WardrobeItem : ICloneable{

    WardrobeItem() {}

    //clone item
    public object Clone()
    {
        WardrobeItem itemClone = (WardrobeItem) this.MemberwiseClone();
        return itemClone;
    }

    public int ID { get { return (int)mItem; } }

    [Header("ItemDetails")]
    public GameObject mPrefab;
    public CustomizationGift mItem;
    public WardrobeItemType mType;
    public int mCost;

    //where on the penguin the item should go
    [Header("Positioning")]
    public Vector3 mPos;
    public Vector3 mRot;
    public Vector3 mScale;

    [Header("UI")]
    public string TileName;
    public Sprite TileIcon;

    
}
