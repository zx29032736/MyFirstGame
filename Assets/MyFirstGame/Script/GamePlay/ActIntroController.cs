using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ActIntroController : MonoBehaviour {

    public DirectionCanvasController directController;
    public Button readyBtn;
    public Text introDialog;

    private void Awake()
    {

        readyBtn.onClick.RemoveAllListeners();
        readyBtn.onClick.AddListener(() =>
        {
            if (GamePlayManager.IsMultiPlayer)
            {
                if (!PhotonNetwork.isMasterClient)
                {
                    PhotonNetwork.RaiseEvent(100, null, true, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient });
                }
                else if (PhotonNetwork.isMasterClient)
                {
                    directController.playManager.RefreshRoomProperties(GamePlayManager.GameplayEvent.StartQuest);
                }

            }
            else
            {
                GamePlayManager.RaiseGameplayEvent("", GamePlayManager.GameplayEvent.StartQuest);
            }
            PF_GamePlay.OutroPane(this.gameObject, 0);
        }
        );
    }

    public void ShowInfo()
    {
        Init();
        introDialog.text = PF_GamePlay.ActiveQuest.Acts.Values.ToList()[0].IntroMonolog;
        //ConfirmPanel.SetActive(true);
    }

    public void RefreshConfirmPanel()
    {
        //Button closeBtn = ConfirmPanel.GetComponentInChildren<Button>();
        readyBtn.GetComponentInChildren<Text>().text = "Start!!";
        readyBtn.interactable = true;
    }

    void Init()
    {
        readyBtn.interactable = false;

        if (GamePlayManager.IsMultiPlayer)
        {
            if (!PhotonNetwork.isMasterClient)
            {
                directController.playManager.StartCoroutine(GamePlayManager.Wait(() => { directController.playManager.PhotonInitialize(); readyBtn.interactable = true; }, 1));
            }
            else if (PhotonNetwork.isMasterClient)
            {
                System.Action action = () =>
                {
                    directController.playManager.PhotonInitialize();
                    readyBtn.GetComponentInChildren<Text>().text = "Waiting for player";
                    PhotonNetwork.RaiseEvent(100, null, true, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient });
                };

                directController.playManager.StartCoroutine(GamePlayManager.Wait(action, 1));
            }
        }
        else
        {
            RefreshConfirmPanel();
        }

    }

}
