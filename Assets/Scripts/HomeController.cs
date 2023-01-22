using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class HomeController : MonoBehaviourPunCallbacks
{

    [SerializeField]
    GameObject PlayButtonPanel;
    [SerializeField]
    TMP_Text PlayButtonPanelText;

    [SerializeField]
    GameObject WaitingPanel;
    public void LoadSinglePlayerScene()
    {
        PlayerPrefs.SetString("GameMode", "SinglePlayer");
        SceneManager.LoadScene("Game");
    }

    public void LoadMultiPlayerScene()
    {
        PlayerPrefs.SetString("GameMode", "MultiPlayer");
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("On Connected To Master");

        ExitGames.Client.Photon.Hashtable setPlayerProperties = new ExitGames.Client.Photon.Hashtable();
        setPlayerProperties.Add("SceneLoaded", (bool)false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(setPlayerProperties);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        byte maxPlayers = 2;
        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000 };
        PhotonNetwork.CreateRoom(null, options);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayButtonPanel.SetActive(true);
        }
        else
        {
            WaitingPanel.SetActive(true);
        }
       
    }


    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PlayButtonPanelText.text = "Someone joined the room. \n You can start the game now.";
        }
    }

    public void OnStartGameMasterClient()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {

            PlayButtonPanelText.text = "Please wait for someone to join.";
            return;
        }

        PhotonNetwork.LoadLevel("Game");
    }


}
