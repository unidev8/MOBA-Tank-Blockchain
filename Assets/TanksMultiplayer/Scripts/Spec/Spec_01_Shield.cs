using System.Collections;
using System.Collections.Generic;
using TanksMP;
using Unity.Jobs;
using UnityEngine;

public class Spec_01_Shield : MonoBehaviour
{
    private float despawnDelay = SpecConsts.cSpec_Delay_Time[SpecConsts.cSpec_0102_Shield];
    [HideInInspector]
    public GameObject player;
    [HideInInspector]
    public GameObject spec_Manager;

    void Update()
    {
        if (player) // only owner is set. (ex iceburg, exploration)
        {
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 0.3f, player.transform.position.z);
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
        if (player)
            player.GetComponent<Player>().isSpecShield = false;
    }
}
