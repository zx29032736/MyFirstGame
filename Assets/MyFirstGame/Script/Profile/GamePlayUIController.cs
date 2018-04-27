using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class GamePlayUIController : Photon.PunBehaviour {

    public Text playerName;
    public Text playerID;
    public Text connStates;
    bool isconnected = false;

    public InputField roomInput;

    public GameObject modeSelection;
    public GameObject roomSelection;
    public RoomController roomController;
    public SoloLevelPicker soloLevelPicker;

    public void Init()
    {
        modeSelection.SetActive(true);
        roomSelection.SetActive(false);
        gameObject.SetActive(true);
        isconnected = false;

        PhotonNetwork.automaticallySyncScene = true;

        if (PhotonNetwork.connected)
            PhotonNetwork.Disconnect();

    }

    private void Update()
    {
        if (isconnected)
            connStates.text = PhotonNetwork.connectionState.ToString();
    }

    public void SoloPlay()
    {
        soloLevelPicker.Init();
    }

    public void MultiPlay()
    {
        modeSelection.SetActive(false);
        roomSelection.SetActive(true);

        if (!PhotonNetwork.connected)
        {
            isconnected = true;
            PhotonNetwork.ConnectUsingSettings("0.8");
        }

        DialogCanvasController.RequestLoadingPrompt(PlayFabAPIMethods.Generic);
    }

    public void LeaveSelection()
    {
        gameObject.SetActive(false);
    }

    public void CreateRoom()
    {
        if (isTextValid(roomInput.text))
            PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions() { MaxPlayers = 3 , PublishUserId = true}, null);
    }

    public void JoinRoom()
    {
        if (isTextValid(roomInput.text))
            PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions() { MaxPlayers = 3 }, null);
    }

    bool isTextValid(string t)
    {
        if (string.IsNullOrEmpty(t))
            return false;
        else
            return true;
    }

    #region Photon behavior

    private void OnConnectedToServer()
    {
        Debug.Log("PhotonNetwork.connected123");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("As OnConnectedToMaster() got called, the PhotonServerSetting.AutoJoinLobby must be off. Joining lobby by calling PhotonNetwork.JoinLobby().");
        playerName.text = PhotonNetwork.playerName;
        //playerID.text = PhotonNetwork.player.UserId;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if (PF_PlayerData.SavedTeam == null)
        {
            Debug.LogError("PF_PlayerData.SavedTeam == null");
            return;
        }
       
        string s = PlayFab.Json.PlayFabSimpleJson.SerializeObject(PF_PlayerData.SavedTeam[0]);
        PhotonNetwork.player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "SavedData",s} });
        Debug.Log("Joined lobby");
    }

    public override void OnJoinedRoom()
    {
        roomController.Init();
    }

    public override void OnPhotonJoinRoomFailed(object[] cause)
    {
       
        Debug.Log("Error: Can't join room (full or unknown room name). " + cause[1]);
    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        var hash = new ExitGames.Client.Photon.Hashtable();
        hash = (ExitGames.Client.Photon.Hashtable)playerAndUpdatedProps[1];
        FG_SavedCharacter cha = PlayFab.Json.PlayFabSimpleJson.DeserializeObject<FG_SavedCharacter>((string)hash["SavedData"]);

        DialogCanvasController.Instance.CloseLoadingPrompt(PlayFabAPIMethods.Generic);

    }
#endregion
}
