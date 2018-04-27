using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatDisplayer : MonoBehaviour {

    public UIManager uiManager;
    public RectTransform myTr;
    public Text nameText;
    public Text HpText;
    public Text AttackText;
    public Text SpAttackText;
    public Text DefenseText;
    public Text SpDefense;
    public Text SpeedText;
    public Text LevelText;

    public List<Text> StatusText;

    bool keepShowing = false;
    Unit selectedUnit = null;
    public void Init(Unit unit)
    {
        if (selectedUnit != unit)
            selectedUnit = unit;
        gameObject.SetActive(true);
        keepShowing = true;
        test();
    }

    private void test()
    {
        if (keepShowing)
        {
            nameText.text = selectedUnit.name;
            HpText.text = selectedUnit.savedCharacter.PlayerVitals.Health + " / " + selectedUnit.savedCharacter.PlayerVitals.MaxHealth;
            AttackText.text = BuffValueCalculator(selectedUnit.savedCharacter.PlayerVitals.MaxAttack, selectedUnit.savedCharacter.PlayerVitals.Attack);//selectedUnit.savedCharacter.PlayerVitals.Attack.ToString();
            SpAttackText.text = BuffValueCalculator(selectedUnit.savedCharacter.PlayerVitals.MaxSpAttack, selectedUnit.savedCharacter.PlayerVitals.SpAttack);//selectedUnit.savedCharacter.PlayerVitals.SpAttack.ToString();
            DefenseText.text = BuffValueCalculator(selectedUnit.savedCharacter.PlayerVitals.MaxDefense, selectedUnit.savedCharacter.PlayerVitals.Defense);//selectedUnit.savedCharacter.PlayerVitals.Defense.ToString();
            SpDefense.text =  BuffValueCalculator(selectedUnit.savedCharacter.PlayerVitals.MaxSpDefense, selectedUnit.savedCharacter.PlayerVitals.SpDefense);// selectedUnit.savedCharacter.PlayerVitals.SpDefense.ToString();
            SpeedText.text = BuffValueCalculator(selectedUnit.savedCharacter.PlayerVitals.MaxSpeed, selectedUnit.savedCharacter.PlayerVitals.Speed);//selectedUnit.savedCharacter.PlayerVitals.Speed.ToString();
            LevelText.text = selectedUnit.savedCharacter.characterData.CharacterLevel.ToString();

            for(int i = 0; i < StatusText.Count;i++)
            {
                if (i > selectedUnit.savedCharacter.PlayerVitals.ActiveStati.Count - 1)
                {
                    StatusText[i].gameObject.SetActive(false);
                }
                else
                {
                    if (selectedUnit.savedCharacter.PlayerVitals.ActiveStati[i].Target.Contains("Player"))
                        StatusText[i].text = "<color=green>" + selectedUnit.savedCharacter.PlayerVitals.ActiveStati[i].StatusName + "</color>";
                    else if (selectedUnit.savedCharacter.PlayerVitals.ActiveStati[i].Target.Contains("Enemy"))
                        StatusText[i].text = "<color=blue>" + selectedUnit.savedCharacter.PlayerVitals.ActiveStati[i].StatusName + "</color>";
                    StatusText[i].gameObject.SetActive(true);
                }
            }

            myTr.anchoredPosition = UIManager.WorldToUI(uiManager.myRectTransform, selectedUnit.transform.position);
        }
    }

    string BuffValueCalculator(int maxValue, int currentValue)
    {
        if (maxValue - currentValue > 0)
        {
            return string.Format("{0} (<color=blue> -{1} </color>)", currentValue, Mathf.Abs(maxValue - currentValue));
        }
        else if (maxValue - currentValue < 0)
        {
            return string.Format("{0} (<color=green>{1}</color>)", currentValue, Mathf.Abs(maxValue - currentValue));
        }
        else
            return currentValue.ToString();
    }

    public void Close()
    {
        keepShowing = false;
        gameObject.SetActive(false);
    }

}
