using Photon.Pun;
using Shapes2D;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TanksMP
{
    public class Spec_01_Fire : MonoBehaviour
    {        
        public int damage = 5;
        private float despawnDelay = 8f;        
        public GameObject hitFX;

        public AudioClip hitClip;
        public AudioClip explosionClip;

        [HideInInspector]
        public GameObject owner;
        private GameObject explosionObj;


        void OnParticleCollision(GameObject otherObj)
        {
            //Debug.Log("Spec.OnParticleCollision: other = " + otherObj.name!);
            Player player = otherObj.GetComponent<Player>();
            PlayerBot playerBot = otherObj.GetComponent<PlayerBot>();

            if (player != null)
            {
                if (IsFriendlyFire(owner.GetComponent<Player>(), player)) return;
                if (hitFX)
                {
                    explosionObj = PoolManager.Spawn(hitFX, otherObj.transform.position, Quaternion.identity);
                    explosionObj.GetComponent<Spec_01_Explosion>().owner = otherObj;
                }
                //if (hitClip) AudioManager.Play3D(hitClip, transform.position);
            }
            if (playerBot != null)
            {
                if (hitFX)
                {
                    explosionObj = PoolManager.Spawn(hitFX, otherObj.transform.position, Quaternion.identity);
                    explosionObj.GetComponent<Spec_01_Explosion>().owner = otherObj;
                }
                if (hitClip) AudioManager.Play3D(hitClip, transform.position);
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
                targets[i].TakeDamage_Fire(this);

                //if(ownerPlayer.socketID != ""){
                //SocketGameManager.instance.AddExperience(ownerPlayer.socketID, targets[i].level);
                //}
            }

            if (otherObj.name == "Tree" || otherObj.name.Contains ("House") || otherObj.name.Contains ("logs") || otherObj.name.Contains("tent") || otherObj.name.Contains("Well"))
            {
                if (hitFX)
                    explosionObj = PoolManager.Spawn(hitFX, new Vector3(otherObj.transform.position.x, 0, otherObj.transform.position.z), Quaternion.identity);
                if (hitClip) AudioManager.Play3D(hitClip, transform.position);
                explosionObj.GetComponent<Spec_01_Explosion>().owner = otherObj;
            }

        }   

        //method to check for friendly fire (same team index).
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

        //set despawn effects and reset variables
        void OnDespawn()
        {

        }
    }

}

