using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon;
//using ExitGames = ExitGames.Client.Photon.Hashtable;

public class TurnController : PunBehaviour, IPunTurnManagerCallbacks
{
    public PunTurnManager turnManager;

    public void Init()
    {
        turnManager = gameObject.AddComponent<PunTurnManager>();
        turnManager.TurnManagerListener = this;
        turnManager.TurnDuration = 60f;
    }

    public void StartTurn()
    {
        if (PhotonNetwork.isMasterClient)
        {
            this.turnManager.BeginTurn();
        }
    }


    public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        //Debug.LogError("changed !!");
    }

    #region PunTurnManagerCallbacks
    public void OnPlayerFinished(PhotonPlayer player, int turn, object move)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerMove(PhotonPlayer player, int turn, object move)
    {
        throw new NotImplementedException();
    }

    public void OnTurnBegins(int turn)
    {
        throw new NotImplementedException();
    }

    public void OnTurnCompleted(int turn)
    {
        throw new NotImplementedException();
    }

    public void OnTurnTimeEnds(int turn)
    {
        throw new NotImplementedException();
    }
    #endregion
}
