using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class Spec_02_IceCub : MonoBehaviour
{
    private float despawnDelay = SpecConsts.cSpec_Delay_Time[SpecConsts.cSpec_0201_IceShell]; // // depending on particle aniamtion  
    [HideInInspector]
    public GameObject player;
    [HideInInspector]
    public GameObject spec_Manager;

    void Update()
    {
        if (player) // only owner is set. (ex iceburg, exploration)
        {
            transform.position = player.transform.position;
            transform.rotation = player.transform.rotation;
        }
        else
        {            
        }

    }

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }
    void OnDespawn()
    {
        if (player)
            player.GetComponent<Player>().SpeedReturn();
        //player.GetComponent<PhotonView>().RPC("CmdSpeedReturn", RpcTarget.AllViaServer);
    }
}
