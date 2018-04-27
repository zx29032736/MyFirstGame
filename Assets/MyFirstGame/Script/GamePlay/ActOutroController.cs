using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActOutroController : MonoBehaviour {

    public Button returnBtn;

    public Image GoldCollectedImage;
    public Text GoldCollectedText;

    public Image ItemsCollectedImage;
    public Text ItemsCollectedText;

    public Image ExpToLevelImage;
    public Text ExpText;

    public void Awake()
    {
        returnBtn.onClick.RemoveAllListeners();
        returnBtn.onClick.AddListener(() => { RetrunToProfile(); });
    }

    public void RetrunToProfile()
    {
        //update to playfab
        if (PF_GamePlay.QuestProgress.isQuestWon)
        {
            PF_GamePlay.SavePlayerData(PF_PlayerData.SavedTeam[0], PF_GamePlay.QuestProgress);
        }
        if (!PF_GamePlay.QuestProgress.areItemsAwarded)
        {
            PF_GamePlay.RetriveQuestItems();
        }

        PhotonNetwork.Disconnect();
        GameController.Instance.sceneController.RequestSceneChange(SceneController.GameScenes.Profile,1f);
    }

    public void UpdateQuestStats()
    {
        QuestTracker progress = PF_GamePlay.QuestProgress;

        progress.isQuestWon = true;

        progress.GoldCollected = Random.Range(0, 100);
        progress.XpCollected = Random.Range(0, 100);
        progress.ItemsFound.Add("MediumPileOGold");

        GoldCollectedText.text = progress.GoldCollected.ToString();
        ExpText.text = progress.XpCollected.ToString();
        ItemsCollectedText.text = progress.ItemsFound.Count.ToString();

        CheckedActivedLevelUp();

        if (PF_PlayerData.SavedTeam[0] != null)
        {
            Debug.Log("May be do level up");
        }
    }

    void CheckedActivedLevelUp()
    {
        string level = PF_PlayerData.SavedTeam[0].characterData.CharacterLevel.ToString();
        if (PF_GameData.CharacterLevelRamp[level] <= PF_PlayerData.SavedTeam[0].characterData.ExpThisLevel + PF_GamePlay.QuestProgress.XpCollected)
        {
            PF_PlayerData.SavedTeam[0].characterData.TotalExp = PF_PlayerData.SavedTeam[0].characterData.ExpThisLevel;
            PF_PlayerData.SavedTeam[0].PlayerVitals.levelIncrease++;
        }
    }
}
