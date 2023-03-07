using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class Spec_General : MonoBehaviour
{
    [HideInInspector]
    public float despawnDelay = 5f;
    [HideInInspector]
    public GameObject owner;
       
    
    void Update()
    {
        if (owner) // only owner is set. (ex iceburg, exploration)
        {
            transform.position = owner.transform.position;
            transform.rotation = owner.transform.rotation;
        }

    }

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }
}
