using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomisationItem : MonoBehaviour
{
    //used for ui, 
    public CustomizationGift Item;

    //used for inv manager 
    [HideInInspector]
    public WardrobeItemType ItemType;
}
