using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player : MonoBehaviourPun
{
    [SerializeField]
    private float moveDelay = 0.8f;
    [SerializeField]
    SnakeBody snakeBodyP;
    [SerializeField]
    Direction currDirection = Direction.Right;
    List<SnakeBody> snakeBodys = new List<SnakeBody>();
    public SnakeBody headSnake;
    public bool hasEatenFood = false;
    PhotonView PV;
    public int LocalScore = 0;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    private void Start()
    {
        if (PlayerPrefs.GetString("GameMode") == "SinglePlayer" || PV.IsMine)
        {
            StartMoveSnake();
        }
    }


    public bool HasCollisionWithSnakeBody()
    {
 
        for (int i = 1; i < snakeBodys.Count;i++)
        {
            if (headSnake.transform.position == snakeBodys[i].transform.position) return true;
        }
        return false;
    }

    public void StartMoveSnake()
    {

        SnakeBody startSnakeBody;
        if(PlayerPrefs.GetString("GameMode") == "SinglePlayer")
        {
            startSnakeBody = Instantiate(snakeBodyP, transform.position, Quaternion.identity).GetComponent<SnakeBody>();
        }
        else
        {
            startSnakeBody = PhotonNetwork.Instantiate("SnakeBody", transform.position, Quaternion.identity).GetComponent<SnakeBody>();
        }
        headSnake = startSnakeBody;
        headSnake.thisPlayer = this;
        snakeBodys.Add(startSnakeBody);
        headSnake.lastdirection = currDirection;
        StartCoroutine(MoveSnake());
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (PlayerPrefs.GetString("GameMode") == "SinglePlayer")
            {
                GameManager.Instance.OnPlayerDied(-1);
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("OnPlayerDied", RpcTarget.All, PV.OwnerActorNr);

            }
        }



        if (collision.gameObject.CompareTag("Snake"))
        {

            if(PV.IsMine && collision.transform.TryGetComponent<SnakeBody>(out SnakeBody body))
            {
                if(body.thisPlayer != this)
                PV.RPC("OnPlayerDied", RpcTarget.All, PV.OwnerActorNr);
            }
        }

    }

    [PunRPC]
    public void SetHasEatenRPC(int Actorno)
    {
        if(PV.IsMine && Actorno == photonView.OwnerActorNr)
        hasEatenFood = true;
    }
    //Helper RPC
    [PunRPC]
    public void StartGameRPC()
    {
        GameManager.Instance.StartGameRPC();
    }
    //Helper RPC
    [PunRPC]
    public void OnTimeOver()
    {
        StopAllCoroutines();
        GameManager.Instance.OnTimeOver();
     
    }
    [PunRPC]
    public void OnPlayerDied(int playerActor)
    {
        StopAllCoroutines();
        GameManager.Instance.OnPlayerDied(playerActor);
    }

    IEnumerator MoveSnake()
    {
        yield return new WaitUntil(() => GameManager.Instance.isGameStarted == true);

        while(GameManager.Instance.isGameOver == false)
        {

            Vector3 currPos = getNextPos(transform.position, currDirection);

            for (int i = snakeBodys.Count - 1; i > 0; i--)
            {
                snakeBodys[i].transform.position = snakeBodys[i - 1].transform.position;
                snakeBodys[i].lastdirection = snakeBodys[i - 1].lastdirection;
            }
            transform.position = currPos;
            snakeBodys[0].transform.position = currPos;
            snakeBodys[0].lastdirection = currDirection;

            if (hasEatenFood)
            {
                
                hasEatenFood = false;

                SnakeBody newBlock;
                if (PlayerPrefs.GetString("GameMode") == "SinglePlayer")
                {
                     newBlock = Instantiate(snakeBodyP, GetNewBodySpawnLocation(snakeBodys[snakeBodys.Count - 1].transform.position, snakeBodys[snakeBodys.Count - 1].lastdirection), Quaternion.identity).GetComponent<SnakeBody>();
                }
                else
                {
                     newBlock = PhotonNetwork.Instantiate("SnakeBody", GetNewBodySpawnLocation(snakeBodys[snakeBodys.Count - 1].transform.position, snakeBodys[snakeBodys.Count - 1].lastdirection), Quaternion.identity).GetComponent<SnakeBody>();
                }
                newBlock.thisPlayer = this;
                newBlock.lastdirection = snakeBodys[snakeBodys.Count - 1].lastdirection;
                snakeBodys.Add(newBlock);
            }

            //CheckSelfCollision
            if(snakeBodys.Count > 2)
            {
                for (int i = 1; i < snakeBodys.Count; i++)
                {
                    if(currPos == snakeBodys[i].transform.position)
                    {

                        if (PlayerPrefs.GetString("GameMode") == "SinglePlayer")
                        {
  
                                GameManager.Instance.OnPlayerDied(-1);
                        }
                        else
                        {
                            PV.RPC("OnPlayerDied", RpcTarget.All, PV.OwnerActorNr);
                        }
                       
                    }
                }
            }


            yield return new WaitForSeconds(moveDelay);
        }


    }

    Vector3 GetNewBodySpawnLocation(Vector3 posLast,Direction direction)
    {
        switch (direction)
        {
            case Player.Direction.Down:
                return new Vector3(posLast.x, posLast.y + 1, 0);
            case Player.Direction.Up:
                return new Vector3(posLast.x, posLast.y - 1, 0);
            case Player.Direction.Left:
                return new Vector3(posLast.x + 1, posLast.y, 0);
            case Player.Direction.Right:
                return new Vector3(posLast.x - 1, posLast.y, 0);
        }
        return Vector3.zero;
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    void TakeInput()
    {

        if (snakeBodys.Count == 1)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                currDirection = Direction.Up;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                currDirection = Direction.Down;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                currDirection = Direction.Left;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                currDirection = Direction.Right;
            }
        }
        else
        {

            if (Input.GetKeyDown(KeyCode.W) && currDirection != Direction.Down)
            {
                currDirection = Direction.Up;
            }
            else if (Input.GetKeyDown(KeyCode.S) && currDirection != Direction.Up)
            {
                currDirection = Direction.Down;
            }
            else if (Input.GetKeyDown(KeyCode.A) && currDirection != Direction.Right)
            {
                currDirection = Direction.Left;
            }
            else if (Input.GetKeyDown(KeyCode.D) && currDirection != Direction.Left)
            {
                currDirection = Direction.Right;
            }
        }
    }
    private void Update()
    {
        if(PlayerPrefs.GetString("GameMode") == "SinglePlayer" && GameManager.Instance.isGameOver == false)
        {
            TakeInput();
        }
        else if (PV != null && GameManager.Instance != null && PV.IsMine && GameManager.Instance.isGameOver == false)
        {
            TakeInput();
        }


  
    }

    public void MobileControls(int dir)
    {
        if (snakeBodys.Count == 1)
        {
            if (dir == 1)
            {
                currDirection = Direction.Up;
            }
            else if (dir == -1)
            {
                currDirection = Direction.Down;
            }
            else if (dir == 2)
            {
                currDirection = Direction.Left;
            }
            else if (dir == -2)
            {
                currDirection = Direction.Right;
            }
        }
        else
        {

            if (dir == 1 && currDirection != Direction.Down)
            {
                currDirection = Direction.Up;
            }
            else if (dir == -1 && currDirection != Direction.Up)
            {
                currDirection = Direction.Down;
            }
            else if (dir == 2 && currDirection != Direction.Right)
            {
                currDirection = Direction.Left;
            }
            else if (dir == -2 && currDirection != Direction.Left)
            {
                currDirection = Direction.Right;
            }
        }
    }


    Vector3 getNextPos(Vector3 currPos,Direction currDirection)
    {
        switch (currDirection)
        {
            case Player.Direction.Down:
                return new Vector3(currPos.x, currPos.y-1, 0);               
            case Player.Direction.Up:
                return new Vector3(currPos.x, currPos.y + 1, 0);
            case Player.Direction.Left:
                return new Vector3(currPos.x-1, currPos.y, 0);
            case Player.Direction.Right:
                return new Vector3(currPos.x+1, currPos.y, 0);
        }
        return Vector3.zero;

    }


}
