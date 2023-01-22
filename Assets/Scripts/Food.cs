using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public enum FoodType
    {
        Static,
        Dynamic
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       

        if (collision.gameObject.CompareTag("Snake"))
        {
            if(PlayerPrefs.GetString("GameMode") == "SinglePlayer")
            {
                GameManager.Instance.MyPlayer.hasEatenFood = true;
                GameManager.Instance.MyPlayer.LocalScore++;
                Destroy(gameObject);
            }
            else if(PhotonNetwork.IsMasterClient == true)
            {
                var pv = collision.GetComponent<PhotonView>();
                GameManager.Instance.UpdatePlayerScore(pv.Owner, (int)pv.Owner.CustomProperties["PlayerScore"] + 1);
                pv.RPC("SetHasEatenRPC", pv.Owner, pv.OwnerActorNr);
                PhotonNetwork.Destroy(this.gameObject);
            }


        }
    }
    // Start is called before the first frame update

}
