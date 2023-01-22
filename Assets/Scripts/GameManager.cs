using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks,IInRoomCallbacks
{

    int MyScore = 0;
    int OtherScore = 0;

    public int boardSize = 26;
    [SerializeField]
    GameObject wallPrefab;
    [SerializeField]
    GameObject StaticfoodPrefab;
    [SerializeField]
    GameObject DynamicfoodPrefab;
    [SerializeField]
    Player playerPrefab;
    [SerializeField]
    Vector2 playerSpawnPos;
    [SerializeField]
    int gameplayTimeinMultiplayer = 90;
    [SerializeField]
    GameObject gameOverPanel;
    [SerializeField]
    TMP_Text gameOverPanelText;
    [SerializeField]
    TMP_Text TimerText;
    [SerializeField]
    GameObject SinglePlayerUI;
    [SerializeField]
    GameObject MultiPlayerUI;
    public Player MyPlayer = null;

    public Vector2 FoodSpawnRange;

    public bool isGameOver = false;
    bool shouldSpawnStaticFood = true;
    public Food currentFood = null;

    public Transform MasterClientSpawnPos;
    public Transform JoinerSpawnPos;

    public static GameManager Instance;
    public bool isGameStarted = false;
    public List<TMP_Text> Scores;
    public bool isSinglePlayerMode;
    public TMP_Text SinglePlayerScore;

    private void Awake()
    {
        Instance = this;
        if (PlayerPrefs.GetString("GameMode", "SinglePlayer") == "SinglePlayer")
        {
            isSinglePlayerMode = true;
        }
        else isSinglePlayerMode = false;
    }
    public void Start()
    {
        if (isSinglePlayerMode)
        {
            SinglePlayerUI.SetActive(true);
            MyPlayer = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity).GetComponent<Player>();
            isGameStarted = true;
            StartCoroutine(ServeFood());
        }
        else
        {
            MultiPlayerUI.SetActive(true);
            ExitGames.Client.Photon.Hashtable setPlayerProperties = new ExitGames.Client.Photon.Hashtable();
            setPlayerProperties.Add("SceneLoaded", (bool)true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(setPlayerProperties);

            StartCoroutine(CheckAndInstantiatePlayer());
        }



    }

    IEnumerator CheckAndInstantiatePlayer()
    {
        yield return new WaitUntil(() => ArePlayersInGameScene() == true);

        if (PhotonNetwork.IsMasterClient)
        {
            ResetPlayerScore();
            MyPlayer = PhotonNetwork.Instantiate("Player", MasterClientSpawnPos.position, Quaternion.identity).GetComponent<Player>();
            StartCoroutine(ServeFoodNetwork());
            MyPlayer.photonView.RPC("StartGameRPC", RpcTarget.All);

        }
        else
        {

            MyPlayer = PhotonNetwork.Instantiate("Player", JoinerSpawnPos.position, Quaternion.identity).GetComponent<Player>();

        }
    }

    bool ArePlayersInGameScene()
    {
        if (PhotonNetwork.PlayerList.Length < 2) return false;
        foreach(var player in PhotonNetwork.PlayerList)
        {
            if(player.CustomProperties.TryGetValue("SceneLoaded",out object flag))
            {
                if ((bool)flag == false) return false;
            }
        }
        return true;
    }
    void ResetPlayerScore()
    {
        foreach (var e in PhotonNetwork.PlayerList)
        {
            UpdatePlayerScore(e, 0);
        }
    }

    public void StartGameRPC()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(Timer());
 
        }

        isGameStarted = true;

    }
    public void UpdatePlayerScore(Photon.Realtime.Player player, int Score)
    {
        if (PhotonNetwork.IsConnected  && PhotonNetwork.IsMasterClient )
        {
            ExitGames.Client.Photon.Hashtable setPlayerProperties = new ExitGames.Client.Photon.Hashtable();
            setPlayerProperties.Add("PlayerScore", (int)Score);
            player.SetCustomProperties(setPlayerProperties);
          
        }

    }

    public void SetGameEnd()
    {
        isGameOver = true;
    }

    public IEnumerator Timer()
    {
        while(gameplayTimeinMultiplayer > 0 && isGameOver == false)
        {
            ExitGames.Client.Photon.Hashtable setRoomProperties = new ExitGames.Client.Photon.Hashtable();
            setRoomProperties.Add("Timer", (int)gameplayTimeinMultiplayer);
            PhotonNetwork.CurrentRoom.SetCustomProperties(setRoomProperties);
            gameplayTimeinMultiplayer--;
            yield return new WaitForSeconds(1f);
        }

        if(isGameOver == false)
        {
            MyPlayer.photonView.RPC("OnTimeOver", RpcTarget.All);

        }
    }

    public void OnTimeOver()
    {
        if (isGameOver) return;
        gameOverPanel.SetActive(true);
        SetGameEnd();
        if (MyScore > OtherScore)
        {
            gameOverPanelText.text = "Time Over,You Won.!\n Your score - " + MyScore;
#if UNITY_ANDROID && !UNITY_EDITOR
        AlertDialog4Unity.Show("Time Over,You Won.!\n Your score - " + MyScore, "OK", null, null, null);

#endif
        }
        else if(MyScore < OtherScore)
        {
            gameOverPanelText.text = "Time Over,Other plyer Won.!\n Your score - " + MyScore;
#if UNITY_ANDROID && !UNITY_EDITOR
        AlertDialog4Unity.Show("Time Over,Other plyer Won.!\n Your score - " + MyScore, "OK", null, null, null);

#endif
        }
        else
        {
            gameOverPanelText.text = "Time Over,Game Tie!\n Your score - " + MyScore;
#if UNITY_ANDROID && !UNITY_EDITOR
        AlertDialog4Unity.Show("Time Over,Game Tie!\n Your score - " + MyScore, "OK", null, null, null);

#endif
        }




    }
    private IEnumerator ServeFood()
    {

        while (!isGameOver)
        {


            yield return new WaitUntil(() => currentFood == null);

            int xPos = UnityEngine.Random.Range((int)-FoodSpawnRange.x + 1, (int)FoodSpawnRange.x);
            int yPos = UnityEngine.Random.Range((int)-FoodSpawnRange.y + 1, (int)FoodSpawnRange.y);

            if (shouldSpawnStaticFood)
            {
                GameObject cFood;
                    cFood = Instantiate(StaticfoodPrefab, new Vector3(xPos, yPos), Quaternion.identity);

                currentFood = (Food)cFood.GetComponent<StaticFood>();
            }
            else
            {
                GameObject cFood;
                    cFood = Instantiate(DynamicfoodPrefab, new Vector3(xPos, yPos), Quaternion.identity);

                currentFood = (Food)cFood.GetComponent<DynamicFood>();
            }
            shouldSpawnStaticFood = !shouldSpawnStaticFood;
        }
    }


    private IEnumerator ServeFoodNetwork()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient == false) yield break;

        while (!isGameOver)
        {


            yield return new WaitUntil(()=>currentFood == null);

            int xPos = UnityEngine.Random.Range((int)-FoodSpawnRange.x + 1, (int)FoodSpawnRange.x);
            int yPos = UnityEngine.Random.Range((int)-FoodSpawnRange.y + 1, (int)FoodSpawnRange.y);

            if (shouldSpawnStaticFood)
            {
                GameObject cFood;
                if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
                {
                    cFood = PhotonNetwork.Instantiate("Static", new Vector3(xPos, yPos), Quaternion.identity);
                }
                else 
                cFood = Instantiate(StaticfoodPrefab, new Vector3(xPos, yPos), Quaternion.identity);

                currentFood = (Food)cFood.GetComponent<StaticFood>();
            }
            else
            {
                GameObject cFood;
                if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
                {
                    cFood = PhotonNetwork.Instantiate("Dynamic", new Vector3(xPos, yPos), Quaternion.identity);
                }
                else
                    cFood = Instantiate(DynamicfoodPrefab, new Vector3(xPos, yPos), Quaternion.identity);

                currentFood = (Food)cFood.GetComponent<DynamicFood>();
            } 
            shouldSpawnStaticFood = !shouldSpawnStaticFood;
        }
    }


    public void OnPlayerDied(int playerActor)
    {
        if (isGameOver) return;

        if(playerActor == -1)
        {
            gameOverPanel.SetActive(true);
            gameOverPanelText.text = "You died. \n Your score - " + MyPlayer.LocalScore;
            SetGameEnd();
#if UNITY_ANDROID && !UNITY_EDITOR
        AlertDialog4Unity.Show("You died.", "OK", null, null, null);

#endif
        }
        else
        {
            gameOverPanel.SetActive(true);
            StopAllCoroutines();
            SetGameEnd();
            if (playerActor != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                gameOverPanelText.text = "Other player died,you won \n Your score - " + MyScore;
#if UNITY_ANDROID && !UNITY_EDITOR
        AlertDialog4Unity.Show(gameOverPanelText.text, "OK", null, null, null);

#endif
            }

            else
            {
                gameOverPanelText.text = "You died,other player won \n Your score - " + MyScore;
#if UNITY_ANDROID && !UNITY_EDITOR
        AlertDialog4Unity.Show(gameOverPanelText.text, "OK", null, null, null);

#endif
            }

        }

    }
    public void Restart()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }


    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        SceneManager.LoadScene("Menu");
    }

    private void Update()
    {
        if (isSinglePlayerMode)
        {
            SinglePlayerScore.text = MyPlayer.LocalScore.ToString();
            return;
        }
            //Update Scores an UI
        if (PhotonNetwork.IsConnected == false && PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length < 2 || MyPlayer == null) return;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {

            if(PhotonNetwork.PlayerList[i] != null && PhotonNetwork.PlayerList[i].CustomProperties.TryGetValue("PlayerScore",out object score))
            {
                if (PhotonNetwork.PlayerList[i].ActorNumber == MyPlayer.photonView.OwnerActorNr) MyScore = (int)score;
                else OtherScore = (int)score;
                Scores[i].text = ("Player# " + PhotonNetwork.PlayerList[i].ActorNumber.ToString() + " Score - " + (int)score).ToString();
            }

        }
        if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Timer",out object Time))
        {
            TimerText.text = "Time - " + Time.ToString();
        }



    }

    public void OnMobileInput(int dir)
    {
        if (isSinglePlayerMode)
        {
            if(MyPlayer != null)
            {
                MyPlayer.MobileControls(dir);
            }

        }
        else if (MyPlayer != null && MyPlayer.photonView.IsMine)
        {
            MyPlayer.MobileControls(dir);
        }
    }



}
