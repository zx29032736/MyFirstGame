using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderBarController : MonoBehaviour {

    public CharacterUnitController unitController;
    public List<Image> orderImage = new List<Image>();

    public void Init(CharacterUnitController uController)
    {
        unitController = uController;

        RefreshBar();
        foreach (var go in orderImage)
        {
            go.gameObject.SetActive(true);
        }
    }

    int cnt = 0;
    public void RefreshBar()
    {
        cnt = unitController.currentId;
        for (int i = 0; i < orderImage.Count; i++)
        {
            if (cnt >= unitController.allCharacterUnit.Count)
                cnt = 0;

            orderImage[i].overrideSprite = GameController.Instance.iconManager.GetIconById(unitController.allCharacterUnit[cnt].savedCharacter.baseClass.Icon);
            cnt++;
        }
    }
}
