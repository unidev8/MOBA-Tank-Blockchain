using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class Spec_06_TeamHealthUp : MonoBehaviour
{
    private float despawnDelay = SpecConsts.cSpec_Delay_Time[SpecConsts.cSpec_0601_Help];
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
    }

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }
    void OnDespawn()
    {
        if (spec_Manager)
        {
            spec_Manager.GetComponent<Spec_Manager>().cooldownStart = Time.time;
            spec_Manager.GetComponent<Spec_Manager>().curIdx = SpecConsts.cSpec_noSpec;
        }
    }
}
