using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestIconUI : MonoBehaviour
{
    [Header("References")]
    public GameObject IconPrefab;
    public GameObject GoalPrefab;
    public GameObject UIParent;
    private RectTransform UIParentTransform;

    public ItemSystem ItemSystem;

    [Header("UI Options")]
    public Vector2 IconStartPos;
    public float IconSpacing;

    public List<QuestIconInfo> QuestIcons;


    private void Start()
    {
        QuestIcons = new List<QuestIconInfo>();

        GameObject g = new GameObject("Quest-Icons-Master");
        g.AddComponent<RectTransform>();
        g.transform.parent = UIParent.transform;
        g.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        g.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        UIParentTransform = g.GetComponent<RectTransform>();

        Debug.Log($"start {UIParentTransform != null}");

        SetUI();
    }

    public void AddQuest(QuestIconInfo quest)
    {
        QuestIcons.Add(quest);
        SetUI();
    }

    public void RemoveQuest(int questIconId)
    {
        for (int i = 0; i < QuestIcons.Count; i++) {
            if (QuestIcons[i].QuestIconID == questIconId) {
                QuestIcons.RemoveAt(i);
                break;
            }
        }
        SetUI();
    }

    private void SetUI()
    {
        //after scene restart this reference goes so find the obj again
        if(UIParentTransform == null) {
            UIParentTransform = GameObject.Find("Quest-Icons-Master").GetComponent<RectTransform>();
        }

        int childs = UIParentTransform.childCount;
        for (int i = childs - 1; i >= 0; i--) {
            Debug.Log(UIParentTransform.GetChild(i).gameObject.name);
            Destroy(UIParentTransform.GetChild(i).gameObject);
        }

        float heightTracker = IconStartPos.y;

        //loops each quest creating a parent object which has the quest name
        //then children objects for the goals and icon
        foreach(QuestIconInfo questIcon in QuestIcons) {
            GameObject IconMain = Instantiate(IconPrefab, UIParentTransform);
            IconMain.GetComponent<RectTransform>().localPosition = new Vector2(IconStartPos.x, heightTracker);
            IconMain.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = questIcon.Name;

            //loops each goal in the quest
            for (int i = 0; i < questIcon.goalsItemIds.Count; i++) {
                GameObject IconGoal = Instantiate(GoalPrefab, IconMain.transform);
                IconGoal.GetComponent<RectTransform>().localPosition = new Vector2(0, 50 - IconSpacing * (i + 1));
                
                //format for the goal text = item name (item count progress) / (item goal max)
                string goal = ItemSystem.GetName(questIcon.goalsItemIds[i]) + $" {questIcon.goalsItemProgress[i]} / {questIcon.goalsItemMax[i]}";
                
                IconGoal.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = goal;
                IconGoal.GetComponentInChildren<Image>().sprite = ItemSystem.GetIcon(questIcon.goalsItemIds[i]);
            }

            heightTracker -= 50 + IconSpacing * questIcon.goalsItemIds.Count;
        }

    }

}
