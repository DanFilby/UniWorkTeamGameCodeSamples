using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingsQuests : MonoBehaviour, IQuestGiver
{
    [Header("References")]
    public bool readyToStartQuest = false;
    public QuestManager questManager;
    
    public AudioManager audioManager;

    public PlayerManager playerManager;
    private TMPro.TextMeshProUGUI textPromt;
    private QuestManager.DisableUICallBack currentQuestCallback;

    private bool playerInRange = false;
    private bool playerInUI = false;
    private bool playerOnQuest = false;

    public GameObject questMark;
    public GameObject questmarkAccepted;

    private Action<QuestReturnState> questFinishedCallback;

    [Header("Quests")]
    private int playersCurrentQuest = 1;

    public quest2 firstQuest;
    public quest2 secondQuest;
    public quest2 thirdQuest;

    private quest2 currentQuest;

    [HideInInspector]
    public bool startQuest = false;

    private int currentQuestIconId;

    void Start()
    {
        currentQuest = firstQuest;
        InventoryManager.ItemPickedUp += UpdateQuestProgress;
        audioManager = FindObjectOfType<AudioManager>();
    }

    //returns true if the player presses accept button or key
    private bool InputAccept()
    {
        if (startQuest) {
            startQuest = false;
            return true;
        }
        return Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Fire1") || questManager.CheckAccpetClicked();
    }

    //returns true if the player presses decline button or key
    private bool InputDecline()
    {
        return Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.Escape) || questManager.CheckDeclineClicked();
    }

    public void EnableQuest(Action<QuestReturnState> callback)
    {
        questFinishedCallback = callback;
        readyToStartQuest = true;
        startQuest = true;
    }

    public void DisableQuest()
    {
        readyToStartQuest = false;
        questFinishedCallback = null;
    }

    void Update()
    {
        if (InputAccept() && playerInRange && !playerInUI && readyToStartQuest) {
            bool playerCanCompleteQuest = CheckCanCompleteQuest(currentQuest);

            //player gets shown quest to accept it
            if (!playerOnQuest) {
                currentQuestCallback = questManager.SetQuestUI(currentQuest, QuestManager.QuestUIStatus.UnAccepted);
                playerInUI = true;
            }
            else if (playerCanCompleteQuest) {
                currentQuestCallback = questManager.SetQuestUI(currentQuest, QuestManager.QuestUIStatus.CanComplete);
                playerInUI = true;
            }
            //player gets shown quest to finish it
            else {
                currentQuestCallback = questManager.SetQuestUI(currentQuest, QuestManager.QuestUIStatus.InProgress);
                playerInUI = true;
            }
            //textPromt.enabled = false;

        }
        else if (playerInUI) {
            bool playerCancompleteQuest = CheckCanCompleteQuest(currentQuest);          
            ManageActiveUI(playerCancompleteQuest);
        }

        //Debug.Log($"Current quest: {playersCurrentQuest}");
    }

    //called each frame when the player is looking at the quest UI
    private void ManageActiveUI(bool canComplete)
    {
        //player out of range disable menu
        if (!playerInRange) {
            currentQuestCallback.Invoke();
            playerInUI = false;
        }

        //player completes quest
        if(InputAccept() && canComplete) {
            CompleteQuest();
        }

        //accept quest
        else if (InputAccept() && !playerOnQuest) {
            AcceptQuest();
        }

        //decline quest
        if (InputDecline()) {
            currentQuestCallback.Invoke();
            playerInUI = false;
            playerManager.inQuestScreen = false;

            //player declined the quest so will show dialogue again
            if (!playerOnQuest) {
                questFinishedCallback.Invoke(QuestReturnState.Declined);
            }
        }
    }

    private void AcceptQuest()
    {
        questManager.SpokeToTheKing();

        currentQuestCallback.Invoke();
        audioManager.Play("QuestAccept");
        questMark.SetActive(false);
        questmarkAccepted.SetActive(true);
        playerInUI = false;
        playerOnQuest = true;
        playerManager.inQuestScreen = false;

        currentQuestIconId = questManager.SetQuestIcon(IconInfo(currentQuest));

        questFinishedCallback.Invoke(QuestReturnState.Accepted);
    }

    private void CompleteQuest()
    {
        //diable quest UI screen
        currentQuestCallback.Invoke();
        questManager.RemoveQuestIcon(currentQuestIconId);
        audioManager.Play("QuestComplete");
        playerInUI = false;
        playerOnQuest = false;
        playerManager.inQuestScreen = false;

        //add quest reward
        playerManager.PenguinCash += currentQuest.CoinReward;

        //take the quest items away from inventory
        foreach (var goal in currentQuest.goals) {
            if(goal.goaltpye == GoalType.GatherFish) {
                playerManager.inventoryManager.RemoveFish(goal.fishType, goal.count);
            }
        }

        //reset dialogue manager so can talk to king again 
        questFinishedCallback.Invoke(QuestReturnState.Completed);

        //check if need to progress the level
        questManager.CheckLevelProgression(currentQuest.ProgressMainLevel, currentQuest.ProgressSideLevel);

        //move onto next quest, doesn't start untill player speaks with king again
        NextQuest();
    }

    private void NextQuest()
    {
        playersCurrentQuest++;

        //turn back on the quest indicator
        if(playersCurrentQuest != 4){
            questMark.SetActive(true);
            questmarkAccepted.SetActive(false);
        }

        if (playersCurrentQuest == 2) {
            currentQuest = secondQuest;         
        }
        if (playersCurrentQuest == 3) {
            currentQuest = thirdQuest;    
        }
    }

    //returns true if player can complete the quest
    private bool CheckCanCompleteQuest(quest2 quest)
    {
        int goalsCompleted = 0;

        Dictionary<FishType, int> playersCurrentFish = playerManager.FishCaught();

        foreach(Goal questGoal in quest.goals) {
            switch (questGoal.goaltpye) {

                case GoalType.GatherFish:
                    //player has more fish than needed
                    if(playersCurrentFish.TryGetValue(questGoal.fishType, out int count)) {
                        if(count >= questGoal.count) {
                            goalsCompleted++;
                        }
                    }
                    break;
                case GoalType.GatherItems:
                    if(playerManager.InventoryContains(questGoal.itemId, questGoal.count, out int _)) {
                        goalsCompleted++;
                    }             
                    break;
            }
        }

        return goalsCompleted >= quest.goals.Count;
    }

    private QuestIconInfo IconInfo(quest2 quest)
    {
        Dictionary<FishType, int> playersCurrentFish = playerManager.FishCaught();
        List<int> goalItemIds = new List<int>();
        List<int> goalItemProgress = new List<int>();
        List<int> goalItemMax = new List<int>();

        foreach (var goal in quest.goals) {
            switch (goal.goaltpye) {
                case GoalType.GatherFish:
                    goalItemIds.Add(ItemSystem.FishToId[goal.fishType]);
                    if (playersCurrentFish.TryGetValue(goal.fishType, out int count)) {
                        goalItemProgress.Add(count);
                    }
                    else { goalItemProgress.Add(0); }
                    break;
                case GoalType.GatherItems:
                    goalItemIds.Add(goal.itemId);
                    playerManager.InventoryContains(goal.itemId, goal.count, out int currentAmount);
                    goalItemProgress.Add(currentAmount);
                    break;
            }
            goalItemMax.Add(goal.count);

        }

        QuestIconInfo iconInfo = new QuestIconInfo(
            quest.QuestTitle,
            goalItemIds,
            goalItemProgress,
            goalItemMax
            );
        iconInfo.QuestIconID = currentQuestIconId;

        return iconInfo;

    }

    private void UpdateQuestProgress()
    {
        if (playerOnQuest) {
            questManager.UpdateQuestIcon(currentQuestIconId, IconInfo(currentQuest));
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent<PlayerManager>(out _) ) {
            if (readyToStartQuest) {
                textPromt = playerManager.GetPrompt();
                //textPromt.enabled = true;
                textPromt.text = "I have a Quest Young Penguin,\n Press E to view";
            }
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerManager>(out _)) {
            textPromt = playerManager.GetPrompt();
            textPromt.enabled = false;
            textPromt.text = "";

            playerInRange = false;
        }
    }

    
}
