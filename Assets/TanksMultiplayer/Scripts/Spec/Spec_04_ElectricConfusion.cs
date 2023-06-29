using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

using TanksMP;

public class Spec_04_ElectricConfusion : MonoBehaviour
{
    private float despawnDelay = 2f; // this is bomb
    [HideInInspector]
    public GameObject owner;

    [SerializeField]
    private GameObject hitPrefab;
    private GameObject hitObj;
    private Player player;

    private void OnParticleCollision(GameObject other)
    {
        player = other.GetComponent<Player>();

        if (player != null)
        {
            if (IsFriendlyFire(owner.GetComponent<Player>(), player)) return;
            hitObj = PoolManager.Spawn(hitPrefab, other.transform.position, other.GetComponent<Player>().turret.rotation);
            hitObj.GetComponent<Spec_04_Electric_ConfusionHit>().player = other;
            //player.photonView.RPC("CmdSpeedDown", RpcTarget.AllViaServer, 0f);
            player.SpeedDown(0);
            player.isFire = false;
            player.TakeDamage(10, 0);
            //if (hitClip) AudioManager.Play3D(hitClip, transform.position);
        }

        //despawn gameobject
        //PoolManager.Despawn(gameObject);
        Player ownerPlayer = owner.GetComponent<Player>();

        //the previous code is not synced to clients at all, because all that clients need is the
        //initial position and direction of the bullet to calculate the exact same behavior on their end.
        //at this point, continue with the critical game aspects only on the server
        if (!PhotonNetwork.IsMasterClient) return;

        //create list for affected players by this bullet and add the collided player immediately,
        //we have done validation & friendly fire checks above already
        List<Player> targets = new List<Player>();
        if (player != null) targets.Add(player);

        //apply bullet damage to the collided players
        for (int i = 0; i < targets.Count; i++)
        {
            //targets[i].TakeDamage_Speedy(this);

            //if(ownerPlayer.socketID != ""){
            //SocketGameManager.instance.AddExperience(ownerPlayer.socketID, targets[i].level);
            //}
        }

    }

    private void Update() // *********Dont delete!!!***********
    {
        if (owner) // only owner is set. (ex iceburg, exploration)
        {
            transform.position = owner.GetComponent<Player>().shotPos.position;
            transform.rotation = owner.GetComponent<Player>().turret.rotation;
        }
    }

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
        if (player)
            player.isFire = true;
    }

    void OnDespawn()
    {       
            
    }

    public bool IsFriendlyFire(Player origin, Player target)
    {
        //do not trigger damage for colliding with our own bullet
        if (target.gameObject == owner || target.gameObject == null) return true;
        //perform the actual friendly fire check on both team indices and see if they match
        else if (!GameManager.GetInstance().friendlyFire && origin.GetView().GetTeam() == target.GetView().GetTeam()) return true;

        //friendly fire is off, this bullet should do damage
        return false;
    }
}
