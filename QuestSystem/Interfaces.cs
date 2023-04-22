using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IQuestGiver {

    public void EnableQuest(Action<QuestReturnState> callback);

    public void DisableQuest();

}


