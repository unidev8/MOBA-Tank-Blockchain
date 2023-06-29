using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class Spec_05_AreaStopEffect : MonoBehaviour
{
    private float despawnDelay = SpecConsts.cSpec_Delay_Time[SpecConsts.cSpec_0502_Confusion];
    [HideInInspector]
    public GameObject player;

    void Update()
    {
        if (player) // only owner is set. (ex iceburg, exploration)
        {
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 2f, player.transform.position.z);
            transform.rotation = player.transform.rotation;
        }

    }

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }
    void OnDespawn()
    {
        if(player)
        {
            player.GetComponent<Player>().SpeedReturn();
            player.GetComponent<Player>().isFire = true;
        }
        
            
    }
}
