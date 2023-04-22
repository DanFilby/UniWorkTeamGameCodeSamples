using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;
    public PauseMenu pauseMenu;
    public Canvas UICanvas;
    public TMPro.TextMeshProUGUI tutorialText;


    [Header("Tutorial Stuff")]
    public bool RunTutorial = false;
    [NonReorderable]
    public List<TutorialInfo> Tutorials;
    private int currentTutorial = 1;
    private int currentTutorialPart = 0;

    public Image TutorialUI;
    public Image QuestTrackerCover;


    private List<GameObject> ActiveTutorialUI;
    private bool doScaleUI;
    private float currentTracker;
    private Vector3 currentScale;

    private bool waterTutorialCompleted;
    public WaterSplash waterSplash;

    void Start()
    {
        waterSplash = FindObjectOfType<WaterSplash>();
        if (!RunTutorial) {
            return;
        }

        ActiveTutorialUI = new List<GameObject>();
        StartCoroutine(ScaleUI());


        QuestTrackerTutorial();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) {
            Next();
        }
        if(waterTutorialCompleted == false && waterSplash.playerFTIW == true) //FTIW - First Time in water
        {
            UnderwaterTutorial();
            Debug.Log("Started Water Tutorial");
        }
    }

    void Next()
    {
        currentTutorialPart++;
        if(currentTutorialPart >= Tutorials[currentTutorial].MaxParts()) {
            SetTutorialUIActive(false);
            return;
        }

        tutorialText.text = Tutorials[currentTutorial].NextTutorialPart(currentTutorialPart) + "\n\nT: To Continue";

    }


    private void QuestTrackerTutorial()
    {
        SetTutorialUIActive(true);
        tutorialText.text = Tutorials[currentTutorial].NextTutorialPart(0) + "\n\nT: To Continue";

        ActiveTutorialUI.Add(QuestTrackerCover.gameObject);
    }
    private void UnderwaterTutorial()
    {
        waterTutorialCompleted = true;
        SetTutorialUIActive(true);
        tutorialText.text = Tutorials[currentTutorial].NextTutorialPart(4) + "\n\nT: To Continue";
    }

    private void SetTutorialUIActive(bool state)
    {
        TutorialUI.gameObject.SetActive(state);
        Time.timeScale = (state)? 0 : 1;
        pauseMenu.playerIsInUI = state;
        pauseMenu.tutorialActive = state;
        doScaleUI = state;
    }

    IEnumerator ScaleUI()
    {
        while (true) {
            if (doScaleUI) {
                currentTracker += 0.01f;
                float scale = Mathf.Sin(currentTracker) * 0.03f;
                currentScale = Vector3.one + new Vector3(scale, scale, scale);

                foreach (var UIComp in ActiveTutorialUI) {
                    UIComp.GetComponent<RectTransform>().localScale = currentScale;
                }
            }
            yield return new WaitForEndOfFrame();
        }    
    }

}

[System.Serializable]
public struct TutorialInfo {
    public List<string> Sentences;
    public List<Image> CoverImages;

    public string NextTutorialPart(int index)
    {
        //enable the correct image
        foreach (var image in CoverImages) {
            image.gameObject.SetActive(false);
        }
        //set the cover active and return the prompt
        CoverImages[index].gameObject.SetActive(true);
        return Sentences[index];
    }

    public int MaxParts()
    {
        return Sentences.Count;
    }

}