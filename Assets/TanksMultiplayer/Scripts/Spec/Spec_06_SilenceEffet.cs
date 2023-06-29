using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class Spec_06_SilenceEffet : MonoBehaviour
{
    private float despawnDelay = SpecConsts.cSpec_Delay_Time[SpecConsts.cSpec_0602_Silence]; //   
    [HideInInspector]
    public GameObject player;

    void Update()
    {
        if (player) // only owner is set. (ex iceburg, exploration)
        {
            transform.position = player.transform.position;
            transform.rotation = player.transform.rotation;
        }

    }

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }

    void OnDespawn()
    {
        if (player)
            player.GetComponent<Player>().isPossibleSpec = true;
    }
}
