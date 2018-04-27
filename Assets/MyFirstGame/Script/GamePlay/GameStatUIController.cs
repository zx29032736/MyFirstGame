using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStatUIController : MonoBehaviour {

    public UIManager uiManager;
    public Text currentTurnText;
    public Text currentStateText;
    public List<GameObject> TeammateStatObj;
    private void OnEnable()
    {
        GamePlayManager.OnGameplayEvent += OnGameplayEventReceived;
    }

    private void OnDisable()
    {
        GamePlayManager.OnGameplayEvent -= OnGameplayEventReceived;
    }

    private void OnGameplayEventReceived(string message, GamePlayManager.GameplayEvent type)
    {
        //currentStateText.text = type.ToString();
        currentTurnText.text = uiManager.playManager.currentTurn.ToString();

        if(uiManager.unitController.currentUnit != null && uiManager.unitController.currentUnit.tag.Contains("Player"))
        {
            currentStateText.text = "Player's Turn _ " + type.ToString();
        }
        else if(uiManager.unitController.currentUnit != null && !uiManager.unitController.currentUnit.tag.Contains("Player"))
        {
            currentStateText.text = "Enemy's Turn _ " + type.ToString();
        }
    }

    public void InitTeamHealthUI(Unit myUnit)
    {
        foreach(var obj in TeammateStatObj)
        {
            if (!obj.activeInHierarchy)
            {
                obj.GetComponentInChildren<CharacterHealthBar>().Init(myUnit, null, false);
                obj.GetComponent<Image>().overrideSprite = GameController.Instance.iconManager.GetIconById(myUnit.savedCharacter.baseClass.Icon);
                obj.SetActive(true);
                break;
            }
        }
    }

}
