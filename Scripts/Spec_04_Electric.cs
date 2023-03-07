using Knife.ScifiEffects;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEngine;

public class Spec_04_Electric : MonoBehaviour 
{
    
    public GameObject lightningObj;
    
    [HideInInspector]
    public float despawnDelay = 8f;
    

    /*private void OnParticleCollision(GameObject other)
    {
        Debug.Log("Electric ParticleCollison: other=" + other);
        Player player = other.GetComponent<Player>();
        PlayerBot playerBot = other.GetComponent<PlayerBot>();

        if (player != null)
        {
            if (IsFriendlyFire(owner.GetComponent<Player>(), player)) return;
            //electricHitObj = PoolManager.Spawn(hitFX, other.transform.position, Quaternion.identity);
            //electricHitObj.GetComponent<Spec_General>().owner = other;
        }
        if (playerBot != null)
        {
            electricHitObj = PoolManager.Spawn(hitFX, other.transform.position, other.GetComponent<Player>().turret.rotation);
            electricHitObj.GetComponent<Spec_General>().owner = other;

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

    private bool IsFriendlyFire(Player origin, Player target)
    {
        //do not trigger damage for colliding with our own bullet
        if (target.gameObject == owner || target.gameObject == null) return true;
        //perform the actual friendly fire check on both team indices and see if they match
        else if (!GameManager.GetInstance().friendlyFire && origin.GetView().GetTeam() == target.GetView().GetTeam()) return true;

        //friendly fire is off, this bullet should do damage
        return false;
    }
    */
    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }
    void OnDespawn()
    {
        this.lightningObj.GetComponent<Lightning>().owner.GetComponent<Player>().isElectricCanon = false;
    }
}
