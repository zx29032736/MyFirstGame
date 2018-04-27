using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSlot : MonoBehaviour {

    public FG_LevelData levelData;
    public Text levelNameText;
    public SoloLevelPicker levelPicker;

    public Button myButton;

    public void Init(string levelName, FG_LevelData data, SoloLevelPicker Picker)
    {
        levelData = data;
        levelNameText.text = levelName;
        levelPicker = Picker;

        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => levelPicker.SelectLevel(this));

        gameObject.SetActive(true);
    }
}
