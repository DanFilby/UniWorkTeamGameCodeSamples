using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//mananges the game's pop-ups, add a pop-up using 'AddPopUP'. 
//it keeps track of all pop ups and shrinks them over time deletes after a while
public class PickupPopUp : MonoBehaviour
{
    [Header("References")]
    public GameObject PopUpPrefab;
    public Transform UIParent;
    public ItemSystem ItemSystem;

    [Header("UI Options")]
    public Vector2 PopUpStartPos;
    public float PopUpSpacing;
    public float PopUpTime;

    private List<PopUp> activePopUps;

    void Start()
    {
        activePopUps = new List<PopUp>();
    }

    private void Update()
    {
        if (activePopUps.Count > 0) {
            foreach (var p in activePopUps) {
                //find percentage of popup time the current popup has been active
                float transparency = (PopUpTime - (Time.time - p.startTime)) / PopUpTime;
                p.SetTransparency(transparency);
            }
            CheckOldPopUps();
        }
    }

    //checks through popups and removes the first one thats older than the set time
    private void CheckOldPopUps()
    {
        for (int i = 0; i < activePopUps.Count; i++) {
            if (Time.time - activePopUps[i].startTime > PopUpTime) {
                activePopUps.RemoveAt(i);
                break;
            }
        }
    }

    public void AddPopUp(PopUpInfo popup)
    {
        //move other pop-ups y pos down
        if(activePopUps.Count >= 1 ) {
            foreach(var p in activePopUps) {
                float currentYPos = p.UITransform.localPosition.y;
                p.UITransform.localPosition = new Vector2(0, currentYPos - PopUpSpacing);
            }
        }

        //create new popup ui object
        GameObject popUpNew = Instantiate(PopUpPrefab, UIParent.transform);
        PopUp pScript = popUpNew.GetComponent<PopUp>();
        pScript.startTime = Time.time;

        //position the popup
        pScript.UITransform = popUpNew.GetComponent<RectTransform>();
        pScript.UITransform.localPosition = new Vector2(0, PopUpStartPos.y);

        //fill popup content
        pScript.itemText.text = ItemSystem.GetName(popup.itemID);
        pScript.countText.text = popup.count.ToString() + "X";
        pScript.itemImage.sprite = ItemSystem.GetIcon(popup.itemID);

        activePopUps.Add(pScript);
    }
}

//struct sent from player containing what item he picked up and how many
public struct PopUpInfo {
    public PopUpInfo(int _id, int _count)
    {
        itemID = _id;
        count = _count;
    }

    public int itemID;
    public int count;
}
