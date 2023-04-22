using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketItem : MonoBehaviour
{
    public enum ItemOwner { Player, MarketSeller };

    public int itemId;
    public string itemName;
    public int itemAmount;
    public int itemCost;

    public Sprite itemIcon;

    public ItemOwner owner;

    public CustomizationGift customisationItemType;

    public void Setup(int id, string name, int amount, int cost, Sprite icon, ItemOwner itemOwner)
    {
        itemId = id;
        itemName = name;
        itemAmount = amount;
        itemCost = cost;
        itemIcon = icon;
        owner = itemOwner;
    }
    
}
