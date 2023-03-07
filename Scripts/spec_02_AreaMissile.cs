using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class spec_02_AreaMissile : MonoBehaviour 
{    
    public GameObject indicatorObj;
    [HideInInspector]
    public float despawnDelay = 5f;
    [HideInInspector]
    public GameObject owner;

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }
    void OnDespawn()
    {
        owner.GetComponent<Player>().isAreaMissil = false;
    }
}
