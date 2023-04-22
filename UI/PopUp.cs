using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    //this class just holds references to the UI components and start info

    public TextMeshProUGUI countText;
    public TextMeshProUGUI itemText;

    public Image itemImage;

    public float startTime;

    [HideInInspector]
    public RectTransform UITransform;


    //set trasparency of all ui components
    public void SetTransparency(float amount)
    {
        //create transparent colour
        Color colour = Color.white;
        colour.a = amount;

        countText.color = colour;
        itemText.color = colour;
        itemImage.color = colour;
    }

}
