using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomisationZone : MonoBehaviour
{
    public WardrobeItemType ZoneType;

    [HideInInspector]
    public CustomisationItem CurrentItem = null;
}
