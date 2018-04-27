using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoloLevelPicker : MonoBehaviour {


    public Text levelDescription;

    public List<LevelSlot> levelSlots;
    private LevelSlot selectedSlot;



    public void Init()
    {
        int count = 0;

        foreach(var go in PF_GameData.Levels)
        {
            levelSlots[count].Init(go.Key, go.Value, this);
            count++;
        }

        gameObject.SetActive(true);
    }

    public void SelectLevel(LevelSlot selected)
    {
        if (selectedSlot == selected)
            return;
        //deselect slots
        DeselectedLevel();

        selectedSlot = selected;
        selectedSlot.GetComponent<Image>().color = Color.red;
        levelDescription.text = selectedSlot.levelData.Description;
    }

    public void DeselectedLevel()
    {
        foreach(var go in levelSlots)
        {
            go.GetComponent<Image>().color = Color.white;
        }
    }

    public void HitPlay()
    {
        PF_GamePlay.ActiveQuest = selectedSlot.levelData;
        SceneController.Instance.RequestSceneChange(SceneController.GameScenes.GamePlay);
        //load scence
    }

    public void Leave()
    {
        foreach (var go in levelSlots)
        {
            go.GetComponent<Image>().color = Color.white;
            go.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
