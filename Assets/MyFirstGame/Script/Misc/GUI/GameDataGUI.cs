using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameDataGUI : MonoBehaviour {

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PF_PlayerData.CreateNewCharacter(PF_GameData.Classes["Ivysaur"].CatalogCode, PF_GameData.Classes["Ivysaur"]);
        }
    }

    private void OnGUI()
    {
        for(int i = 0; i < PF_GameData.Classes.Count; i++)
        {
            GUI.Button(new Rect(100 * i, 0, 100, 50), PF_GameData.Classes.Values.ToList()[i].CatalogCode);
            GUI.Button(new Rect(100 * i, 50 , 100, 50),"HP:" + PF_GameData.Classes.Values.ToList()[i].BaseHP.ToString());
            GUI.Button(new Rect(100 * i, 100, 100, 50), "Atk:" + PF_GameData.Classes.Values.ToList()[i].BaseAttack.ToString());
            GUI.Button(new Rect(100 * i, 150, 100, 50), "SpA:" + PF_GameData.Classes.Values.ToList()[i].BaseSpAttack.ToString());
            GUI.Button(new Rect(100 * i, 200, 100, 50), "Def:" + PF_GameData.Classes.Values.ToList()[i].BaseDefense.ToString());
            GUI.Button(new Rect(100 * i, 250, 100, 50), "SpD:" + PF_GameData.Classes.Values.ToList()[i].BaseSpDefense.ToString());
            GUI.Button(new Rect(100 * i, 300, 100, 50), "Spd:" + PF_GameData.Classes.Values.ToList()[i].BaseSpeed.ToString());
        }

        for(int i = 0; i < PF_GameData.Spells.Count; i++)
        {
            GUI.Button(new Rect(200 + 100 * i, 0, 100, 50), PF_GameData.Spells.Values.ToList()[i].Icon);
        }
    }
}
