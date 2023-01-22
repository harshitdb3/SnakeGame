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
            if(GameManager.Instance.isSinglePlayerMode)
            {
                GameManager.Instance.MyPlayer.SetHasEatenLocal();
                GameManager.Instance.MyPlayer.LocalScore++;
                Destroy(gameObject);
            }
            else if(PhotonNetwork.IsMasterClient == true)
            {
                var pv = collision.GetComponent<PhotonView>();
                pv.RPC("SetHasEatenRPC", RpcTarget.All, pv.OwnerActorNr);
                GameManager.Instance.UpdatePlayerScore(pv.Owner, (int)pv.Owner.CustomProperties["PlayerScore"] + 1);
                PhotonNetwork.Destroy(this.gameObject);
            }


        }
    }
    // Start is called before the first frame update

}
