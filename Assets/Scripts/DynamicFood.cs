using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFood : Food
{
    [SerializeField]
    float speedDelay = 0.5f;
    public Food.FoodType foodType = FoodType.Dynamic;
    MovementType moveType;
    int MoveDirection = 1;
    enum MovementType
    {
        Vertical,
        Horizontal
    }
    private void Start()
    {
        if(GameManager.Instance.isSinglePlayerMode || PhotonNetwork.IsMasterClient)
        {

            if (Random.value > 0.5)
            {
                moveType = MovementType.Vertical;
            }
            else moveType = MovementType.Horizontal;

            StartCoroutine(MoveFood());
        }

    }

    IEnumerator MoveFood()
    {
        while(true)
        {
            if (HasReachedEdge())
                MoveDirection *= -1;
            if (moveType == MovementType.Vertical)
            {
                transform.position += new Vector3(0,MoveDirection, 0);
            }
            else
            {
                transform.position += new Vector3(MoveDirection, 0, 0);
            }
            yield return new WaitForSeconds(speedDelay);
        }


    }

    bool HasReachedEdge()
    {
        if (transform.position.x >= GameManager.Instance.FoodSpawnRange.x || transform.position.x <= -GameManager.Instance.FoodSpawnRange.x ||
            transform.position.y >= GameManager.Instance.FoodSpawnRange.y || transform.position.y <= -GameManager.Instance.FoodSpawnRange.y) return true;
        else return false;
    }
}
