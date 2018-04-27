using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomController : Photon.PunBehaviour {

    public GameObject[] playerSlot;

    List<PhotonPlayer> photonPlayer = new List<PhotonPlayer>();

	public void Init()
    {
        Refresh();
        gameObject.SetActive(true);
    }

    public void Refresh()
    {
        DisableSlot();
        photonPlayer.Clear();
        foreach (var player in PhotonNetwork.playerList)
        {
            photonPlayer.Add(player);

            if (photonPlayer.Count > 3)
                break;
        }


        for (int i = 0; i < photonPlayer.Count; i++)
        {
            playerSlot[i].SetActive(true);
            playerSlot[i].GetComponentInChildren<Text>().text = photonPlayer[i].UserId;
            playerSlot[i].GetComponentInChildren<Image>().overrideSprite = GameController.Instance.iconManager.GetIconById("Middle Health Potion");
            if (photonPlayer[i] == PhotonNetwork.player)
                playerSlot[i].GetComponentInChildren<Text>().text = "<color=blue>" + PhotonNetwork.player.UserId + "</color>";
        }

    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        gameObject.SetActive(false);
    }

    void DisableSlot()
    {
        foreach(var go in playerSlot)
        {
            go.SetActive(false);
        }
    }

    public void EnterGameScene()
    {
        string[] playerList = new string[PhotonNetwork.playerList.Length];
        for(int i = 0; i < PhotonNetwork.playerList.Length;i++)
        {
            playerList[i] = PhotonNetwork.playerList[i].UserId;
        }


        PhotonNetwork.room.SetCustomProperties(new Hashtable() { { "Players", playerList }, { "Level", "The Cave" } });
    }

    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        PhotonNetwork.LoadLevel("GamePlay");
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Refresh();
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Refresh();
    }
}
