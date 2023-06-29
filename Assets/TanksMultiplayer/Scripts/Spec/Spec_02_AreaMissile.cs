using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class Spec_02_AreaMissile : MonoBehaviour
{
    private float despawnDelay = 5f; // No meaning - depend on bomb animatoin
    [HideInInspector]
    public GameObject player;

    void Update()
    {

    }

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }
    void OnDespawn()
    {
    }
}
