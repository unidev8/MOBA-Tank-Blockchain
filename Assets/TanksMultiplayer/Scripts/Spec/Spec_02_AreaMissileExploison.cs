using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class Spec_02_AreaMissileExploison : MonoBehaviour
{
    private float despawnDelay = 5f;// SpecConsts.cSpec_Delay_Time[SpecConsts.cSpec_0102_Shield]; //depending on particle animation
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

    }
}
