/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

namespace TanksMP
{
  /// <summary>
  /// Implementation of AI bots by overriding methods of the Player class.
  /// </summary>
  public class PlayerBot : Player
  {
    //custom properties per PhotonPlayer do not work in offline mode
    //(actually they do, but for objects spawned by the master client,
    //PhotonPlayer is always the local master client. This means that
    //setting custom player properties would apply to all objects)
    [HideInInspector] public string myName;
    [HideInInspector] public int teamIndex;
    [HideInInspector] public int health;
    [HideInInspector] public int shield;
    [HideInInspector] public int ammo;
    [HideInInspector] public int currentBullet;

    /// <summary>
    /// Radius in units for detecting other players.
    /// </summary>
    public float range = 6f;

    //list of enemy players that are in range of this bot
    private List<GameObject> inRange = new List<GameObject>();

    public bool isDebug;
    Color debugColor = new Color();

    //reference to the agent component
    private NavMeshAgent agent;

    //current destination on the navigation mesh
    private Vector3 targetPoint;

    //timestamp when next shot should happen
    private float nextShot;

    //toggle for update logic
    private bool isDead = false;


    //called before SyncVar updates
    void Start()
    {
      debugColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
      //get components and set camera target
      camFollow = Camera.main.GetComponent<FollowTarget>();
      agent = GetComponent<NavMeshAgent>();
      agent.speed = moveSpeed;

      //get corresponding team and colorize renderers in team color
      GameManager gameManager = GameManager.GetInstance();
      int teamInx = GetView().GetTeam();
      targetPoint = gameManager.GetSpawnPosition(teamInx);
      
      agent.Warp(targetPoint);

      Team team = GameManager.GetInstance().teams[GetView().GetTeam()];
      Material _material;
      flag.color = team.color;
      // if (id == 0)
      //   _material = team.material;
      // else
      //   _material = team.customMaterial[id - 1];
      // for (int i = 0; i < renderers.Length; i++)
      //   renderers[i].material = _material;

      //set name in label
      label.text = myName = "Bot" + System.String.Format("{0:0000}", Random.Range(1, 9999));
      //call hooks manually to update
      OnHealthChange(GetView().GetHealth());
      OnShieldChange(GetView().GetShield());

      GameManager.GetInstance().players.Add(this);

      //start enemy detection routine
      StartCoroutine(DetectPlayers());
    }


    //sets inRange list for player detection
    IEnumerator DetectPlayers()
    {
      //wait for initialization
      yield return new WaitForEndOfFrame();

      //detection logic
      while (true)
      {
        //empty list on each iteration
        inRange.Clear();

        //casts a sphere to detect other player objects within the sphere radius
        Collider[] cols = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Player"));
        //loop over players found within bot radius
        for (int i = 0; i < cols.Length; i++)
        {
          //get other Player component
          //only add the player to the list if its not in this team
          Player p = cols[i].gameObject.GetComponent<Player>();
          PhotonView pv = p.GetView();
          int teamNo = pv.GetTeam();

          if ( teamNo != GetView().GetTeam() && !inRange.Contains(cols[i].gameObject))// p.GetView().GetTeam()
                    {
            inRange.Add(cols[i].gameObject);
          }
        }

        //wait a second before doing the next range check
        yield return new WaitForSeconds(1);
      }
    }


    //calculate random point for movement on navigation mesh
    private void RandomPoint(Vector3 center, float range, out Vector3 result)
    {
      //clear previous target point
      result = Vector3.zero;

      //try to find a valid point on the navmesh with an upper limit (10 times)
      for (int i = 0; i < 10; i++)
      {
        //find a point in the movement radius
        Vector3 randomPoint = center + (Vector3)Random.insideUnitCircle * range;
        randomPoint.y = 0;
        NavMeshHit hit;

        //if the point found is a valid target point, set it and continue
        if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
        {
          result = hit.position;
          break;
        }
      }

      //set the target point as the new destination
      agent.SetDestination(result);
    }


    void FixedUpdate()
    {
      //don't execute anything if the game is over already,
      //but termine the agent and path finding routines
      if (GameManager.GetInstance().IsGameOver())
      {
        agent.isStopped = true;
        StopAllCoroutines();
        enabled = false;
        return;
      }

      //don't continue if this bot is marked as dead
      if (isDead) return;

      //stat visualization does not update automatically
      OnHealthChange(health);
      OnShieldChange(shield);

      //no enemy players are in range
      if (inRange.Count == 0)
      {
        //if this bot reached the the random point on the navigation mesh,
        //then calculate another random point on the navmesh on continue moving around
        //with no other players in range, the AI wanders from team spawn to team spawn
        if (Vector3.Distance(transform.position, targetPoint) < agent.stoppingDistance)
        {
          int teamCount = GameManager.GetInstance().teams.Length;
          RandomPoint(GameManager.GetInstance().teams[Random.Range(0, teamCount)].spawn.position, range, out targetPoint);
        }
      }
      else
      {
        if(isDebug){
          Debug.Log("");
        }
        //if we reached the targeted point, calculate a new point around the enemy
        //this simulates more fluent "dancing" movement to avoid being shot easily
        if (Vector3.Distance(transform.position, targetPoint) < agent.stoppingDistance)
        {
          RandomPoint(inRange[0].transform.position, range * 2, out targetPoint);
        }
        
        //shooting loop 
        for (int i = 0; i < inRange.Count; i++)
        {
          RaycastHit hit;
          Debug.DrawLine(transform.position, inRange[i].transform.position, debugColor);
          //raycast to detect visible enemies and shoot at their current position
          if (Physics.Linecast(transform.position, inRange[i].transform.position, out hit))
          {
            //get current enemy position and rotate this turret
            Vector3 lookPos = inRange[i].transform.position;
            turret.LookAt(lookPos);
            turret.eulerAngles = new Vector3(0, turret.eulerAngles.y, 0);
            turretRotation = (short)turret.eulerAngles.y;

            //find shot direction and shoot there
            Vector3 shotDir = lookPos - transform.position;
            Shoot(SpecConsts.bulletType_Bullet, new Vector2(shotDir.x, shotDir.z));
            break;
          }
        }
      }
    }


    /// <summary>
    /// Override of the base method to handle bot respawn separately.
    /// </summary>
    [PunRPC]
    protected override void RpcRespawn(short senderId, PhotonMessageInfo info)
    {
      gameObject.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
      StartCoroutine(Respawn(senderId));
    }


    //the actual respawn routine
    IEnumerator Respawn(short senderId)
    {
      //stop AI updates
      isDead = true;
      inRange.Clear();
      agent.isStopped = true;
      killedBy = null;

      //find original sender game object (killedBy)
      PhotonView senderView = senderId > 0 ? PhotonView.Find(senderId) : null;
      if (senderView != null && senderView.gameObject != null) killedBy = senderView.gameObject;

      //detect whether the current user was responsible for the kill
      //yes, that's my kill: increase local kill counter
      if (killedBy == GameManager.GetInstance().localPlayer.gameObject)
      {
        GameManager.GetInstance().ui.killCounter[0].text = (int.Parse(GameManager.GetInstance().ui.killCounter[0].text) + 1).ToString();
        GameManager.GetInstance().ui.killCounter[0].GetComponent<Animator>().Play("Animation");
      }

      if (explosionFX)
      {
        //spawn death particles locally using pooling and colorize them in the player's team color
        GameObject particle = PoolManager.Spawn(explosionFX, transform.position, transform.rotation);
        ParticleColor pColor = particle.GetComponent<ParticleColor>();
        if (pColor) pColor.SetColor(GameManager.GetInstance().teams[GetView().GetTeam()].material.color);
      }

      //play sound clip on player death
      if (explosionClip) AudioManager.Play3D(explosionClip, transform.position);

      //toggle visibility for all rendering parts (off)
      ToggleComponents(false);
      //wait global respawn delay until reactivation
      yield return new WaitForSeconds(GameManager.GetInstance().respawnTime);
      //toggle visibility again (on)
      ToggleComponents(true);

      //respawn and continue with pathfinding
      targetPoint = GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam());
      transform.position = targetPoint;
      agent.Warp(targetPoint);
      agent.isStopped = false;
      isDead = false;
        GetView().SetHealth(maxHealth);
      }


    //disable rendering or blocking components
    void ToggleComponents(bool state)
    {
      GetComponent<Rigidbody>().isKinematic = state;
      GetComponent<Collider>().enabled = state;

      for (int i = 0; i < transform.childCount; i++)
        transform.GetChild(i).gameObject.SetActive(state);
    }
  }
}
