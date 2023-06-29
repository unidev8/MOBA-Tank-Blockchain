using Photon.Pun;
using TanksMP;
using UnityEngine;

public class Spec_04_Electric_ConfusionHit : MonoBehaviour
{
    private float despawnDelay = SpecConsts.cSpec_Delay_Time[SpecConsts.cSpec_0401_Confusion]; // // depending on particle aniamtion  
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
            player.GetComponent<Player>().SpeedReturn();// ", RpcTarget.AllViaServer);
    }
}
