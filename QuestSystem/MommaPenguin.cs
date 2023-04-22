using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MommaPenguin : MonoBehaviour, IQuestGiver
{
    [Header("References")]
    public PlayerManager playerManager;
    public List<ChickFollow> lostChicks;

    public AudioManager audioManager;

    public QuestManager questManager;
    private QuestManager.DisableUICallBack currentQuestCallback;
    private int currentQuestIconId;

    [Header("Quests")]
    public quest2 Quest;

    //callback to the dialogue trigger, so can start next dialogue
    private Action<QuestReturnState> questFinishedCallback;

    private bool playerInRange;
    private bool playerInUI;
    private bool readyToStartQuest;
    private bool playerOnQuest;

    private quest2 currentQuest;

    private bool InputAccept()
    {
        if (playerOnQuest) {
            return Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Fire1");
        }

        return Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Fire1") || questManager.CheckAccpetClicked();
    }

    //returns true if the player presses decline button or key
    private bool InputDecline()
    {
        return Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.Escape) || questManager.CheckDeclineClicked();
    }


    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        foreach (var chick in lostChicks) {
            chick.gameObject.SetActive(false);
        }

    }

    private void StartQuest()
    {
        foreach (var chick in lostChicks) {
            chick.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        //questManager.UpdateQuestIcon(currentQuestIconId, IconInfo());
        if (InputAccept()
            && playerInRange && !playerInUI && readyToStartQuest ){

            bool playerCanCompleteQuest = CheckCanCompleteQuest();

            //player gets shown quest to accept it
            if (!playerOnQuest) {
                currentQuestCallback = questManager.SetQuestUI(Quest, QuestManager.QuestUIStatus.UnAccepted);
                playerInUI = true;
            }
            else if (playerCanCompleteQuest) {
                currentQuestCallback = questManager.SetQuestUI(Quest, QuestManager.QuestUIStatus.CanComplete);
                playerInUI = true;
            }
            //player gets shown quest to finish it
            else if ( !playerManager.PlayerHoldingChick()) {
                currentQuestCallback = questManager.SetQuestUI(Quest, QuestManager.QuestUIStatus.InProgress);
                playerInUI = true;
            }
            //textPromt.enabled = false;

        }
        else if (playerInUI) {
            bool playerCancompleteQuest = CheckCanCompleteQuest();
            ManageActiveUI(playerCancompleteQuest);
        }

        if (playerOnQuest) {
            questManager.UpdateQuestIcon(currentQuestIconId, IconInfo());
        }
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
        if (InputAccept() && canComplete) {
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

    private bool CheckCanCompleteQuest()
    {
        bool gotAll = true;
        foreach (var chick in lostChicks) {
            //if one chick is still lost
            if (chick.home == false) {
                gotAll = false;
            }
        }

        return gotAll;
    }

    private void AcceptQuest()
    {
        StartQuest();

        audioManager.Play("QuestAccept");
        currentQuestCallback.Invoke();

        playerInUI = false;
        playerOnQuest = true;
        playerManager.inQuestScreen = false;

        currentQuestIconId = questManager.SetQuestIcon(IconInfo());

        questFinishedCallback.Invoke(QuestReturnState.Accepted);
    }

    private void CompleteQuest()
    {
        audioManager.Play("QuestComplete");
        currentQuestCallback.Invoke();
        playerInUI = false;
        playerOnQuest = false;
        playerManager.inQuestScreen = false;

        playerManager.PenguinCash += Quest.CoinReward;
        questManager.RemoveQuestIcon(currentQuestIconId);

        readyToStartQuest = false;

        questFinishedCallback.Invoke(QuestReturnState.Completed);
        //where to move onto next quest

        playerManager.ToggleHat(true);
    }

    private QuestIconInfo IconInfo()
    {
        List<int> itemIds = new List<int>() { 99 };

        int count = 0;
        foreach (var chick in lostChicks) {
            if (chick.home == true) {
                count++;
            }
        }

        List<int> caughtCount = new List<int>() { count };
        List<int> maxCount = new List<int>() { lostChicks.Count };

        QuestIconInfo iconInfo = new QuestIconInfo("Bring Lost Chicks Back to Momma", itemIds,caughtCount,maxCount);

        return iconInfo;
    }
    
    public void EnableQuest(Action<QuestReturnState> callback)
    {
        questFinishedCallback = callback;
        readyToStartQuest = true;
    }

    public void DisableQuest()
    {
        readyToStartQuest = false;

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerManager>(out _)) {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerManager>(out _)) {
            playerInRange = false;

        }
    }

    
}
