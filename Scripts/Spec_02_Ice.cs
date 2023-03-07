using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TanksMP;
using UnityEditor.TextCore.Text;
using UnityEngine;

public class Spec_02_Ice : MonoBehaviour
{
    private float despawnDelay = 6f;
    public GameObject hitFX;

    [HideInInspector]
    public GameObject owner;
    private GameObject iceBoxObj;

    //public AudioClip hitClip;
    //public AudioClip explosionClip;    

    private void OnParticleCollision(GameObject other)
    {
        Player player = other.GetComponent<Player>();
        PlayerBot playerBot = other.GetComponent<PlayerBot>();

        if (player != null)
        {
            if (IsFriendlyFire(owner.GetComponent<Player>(), player)) return;
            //iceObj = PoolManager.Spawn(hitFX, other.transform.position, Quaternion.identity);
            //iceObj.GetComponent<Spec_General>().owner = other;
        }
        if (playerBot != null)
        {
            iceBoxObj = PoolManager.Spawn(hitFX, other.transform.position, other.GetComponent<Player>().turret.rotation);
            iceBoxObj.GetComponent<Spec_General>().owner = other;
            
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

    void OnSpawn()
    {
        PoolManager.Despawn(gameObject, despawnDelay);
    }

    void OnDespawn()
    {
        owner.GetComponent<Player>().isIceCanon = false;
    }

}
