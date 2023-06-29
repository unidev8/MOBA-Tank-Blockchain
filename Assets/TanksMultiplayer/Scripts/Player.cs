/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using Thinksquirrel.CShake;
using Thinksquirrel.CShake;

using ExitGames.Client.Photon;
using UnityEngine.AI;

namespace TanksMP
{
    /// <summary>
    /// Networked player class implementing movement control and shooting.
    /// Contains both server and client logic in an authoritative approach.
    /// </summary> 
    public class Player : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback, IOnEventCallback
    {
        /// <summary>
        /// UI Text displaying the player name.
        /// </summary>    
        public const byte UpdateTankEventCode = 1;
        public int id;
        //NFT id
        public string nft_id;
        public string socketID;
        public TMP_Text label;
        public TMP_Text levelUI;
        public TMP_Text stateChangeText;
        public Animator stateChangeAnimator;
        public Image flag;

        /// <summary>
        /// Maximum health value at game start.
        /// </summary>
        public int maxHealth = 100;

        /// <summary>
        /// Current turret rotation and shooting direction.
        /// </summary>
        [HideInInspector]
        public short turretRotation;

        /// <summary>
        /// Delay between shots.
        /// </summary>
        public float fireRate = 0.75f;
        public float firePower;

        /// <summary>
        /// Movement speed in all directions.
        /// </summary>
        public float moveSpeed = 8f;

        public int exp;

        public int level;
        public int energy;
        public int maxEnergy;
        public int energyPool;
        public int maxEnergyPool;
        /// <summary>
        /// UI Slider visualizing health value.
        /// </summary>
        public Slider healthSlider;
        public Slider levelSlider;
        public Slider energySlider;

        /// <summary>
        /// UI Slider visualizing shield value.
        /// </summary>
        public Slider shieldSlider;

        /// <summary>
        /// Clip to play when a shot has been fired.
        /// </summary>
        public AudioClip shotClip;

        /// <summary>
        /// Clip to play on player death.
        /// </summary>
        public AudioClip explosionClip;

        /// <summary>
        /// Object to spawn on shooting.
        /// </summary>
        public GameObject shotFX;

        /// <summary>
        /// Object to spawn on player death.
        /// </summary>
        public GameObject explosionFX;

        /// <summary>
        /// Turret to rotate with look direction.
        /// </summary>
        public Transform turret;

        /// <summary>
        /// Position to spawn new bullets at.
        /// </summary>
        public Transform shotPos;

        /// <summary>
        /// Array of available bullets for shooting.
        /// </summary>
        public GameObject[] bullets;

        /// <summary>
        /// MeshRenderers that should be highlighted in team color.
        /// </summary>
        public MeshRenderer[] renderers;

        /// <summary>
        /// Last player gameobject that killed this one.
        /// </summary>
        [HideInInspector]
        public GameObject killedBy;

        /// <summary>
        /// Reference to the camera following component.
        /// </summary>
        [HideInInspector]
        public FollowTarget camFollow;

        //timestamp when next shot should happen
        private float nextFire;

        //reference to this rigidbody
#pragma warning disable 0649
        private Rigidbody rb;
#pragma warning restore 0649

        #region LockMove
        Vector3 lastPosition = Vector3.zero;
        List<Vector3> stackPosition = new List<Vector3>();
        bool isStayCollision = false;
        int collisonCount = 0;
        #endregion

        #region Spec
        public byte[] specIdx;
        [HideInInspector]
        public bool isSpecShield = false;
        [HideInInspector]
        public float curMoveSpeed = 0f;
        [HideInInspector]
        public bool isReinforcedBullet = false;
        [HideInInspector]
        public bool isPossibleSpec = true;
        [HideInInspector]
        public bool isFire = true;
        #endregion 

        #region effects
        public GameObject trackEffectObject;
        #endregion

        //initialize server values for this player
        void Awake()
        {
            //only let the master do initialization
            if (!PhotonNetwork.IsMasterClient)
                return;

            stackPosition.Clear();

            //set players current health value after joining

            //GetView().SetHealth(maxHealth);
            //print("tank Awake!" + gameObject.name);
        }

        void InsertBufPos(Vector3 pos)
        {

            if (stackPosition.Count > 1)
            {
                stackPosition.RemoveAt(0);
                stackPosition.Add(pos);
            }
            else
            {
                stackPosition.Add(pos);
            }
        }
        public void OnEnable()
        {
            if (PhotonNetwork.IsMasterClient && DedicatedServerManager.instance.IsServer())
                if (PhotonNetwork.IsMasterClient && DedicatedServerManager.instance.IsServer())
                {
                    return;
                }
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
            if (socketID != "" && nft_id != "")
            {
                SocketGameManager.instance.GetEnergy(socketID, nft_id, -1);
            }
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
            PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            //Debug.Log("Player.OnEvent: event code = " + eventCode);
            //Debug.Log("Player.OnEvent: event code = " + eventCode);
            if (eventCode == UpdateTankEventCode)
            {
                object[] data = (object[])photonEvent.CustomData;
                if (nft_id != data[0].ToString())
                {
                    return;
                }
                PlayerManager.instance.selectedTank.id = data[0].ToString();
                PlayerManager.instance.selectedTank.tankLevel = Convert.ToInt32(data[1]);
                PlayerManager.instance.selectedTank.experience = Convert.ToInt32(data[2]);
                PlayerManager.instance.selectedTank.health = Convert.ToInt32(data[3]);
                PlayerManager.instance.selectedTank.speed = Convert.ToInt32(data[4]);
                PlayerManager.instance.selectedTank.fireRate = Convert.ToInt32(data[5]);
                PlayerManager.instance.selectedTank.firePower = Convert.ToInt32(data[6]);
                PlayerManager.instance.selectedTank.energy = Convert.ToInt32(data[7]);
                PlayerManager.instance.selectedTank.maxEnergy = Convert.ToInt32(data[8]);
                PlayerManager.instance.selectedTank.energyPool = Convert.ToInt32(data[9]);
                PlayerManager.instance.selectedTank.maxEnergyPool = Convert.ToInt32(data[10]);
                PlayerManager.instance.selectedTank.name = data[11].ToString();
                //Debug.Log("Player.OnEvent: event code = " + JsonUtility.ToJson(data));
                //Debug.Log("Player.OnEvent: event code = " + JsonUtility.ToJson(data));
                SetNftPropertys(PlayerManager.instance.selectedTank);
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {

            PhotonNetwork.EnableCloseConnection = true;
            if (info.photonView.InstantiationData == null)
            {
                GetView().SetHealth(maxHealth);
                return;
            }
            object[] instantiationData = info.photonView.InstantiationData;
            socketID = instantiationData[0].ToString();

            NftTank _nftTank = new NftTank(
              instantiationData[1].ToString(),
              "",
              instantiationData[2].ToString(),
              Convert.ToInt32(instantiationData[3]),
              Convert.ToInt32(instantiationData[4]),
              Convert.ToInt32(instantiationData[5]),
              Convert.ToInt32(instantiationData[6]),
              Convert.ToInt32(instantiationData[7]),
              Convert.ToInt32(instantiationData[8]),
              Convert.ToInt32(instantiationData[9]),
              Convert.ToInt32(instantiationData[10]),
              Convert.ToInt32(instantiationData[11]),
              Convert.ToInt32(instantiationData[12]),
              instantiationData[13].ToString()
              );

            SetNftPropertys(_nftTank);
            if (!PhotonNetwork.IsMasterClient)
                return;
            GetView().SetHealth(maxHealth);
            if (PhotonNetwork.IsMasterClient)
            {
                SocketGameManager.instance.SpawnTank(GetView().ViewID, instantiationData[1].ToString(), (NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode));
            }
            GameManager.GetInstance().players.Add(this);

            //print("tank Instantiate--" + ((NftTank)instantiationData[0]).health);
            // ...
        }


        /// <summary>
        /// Initialize synced values on every client.
        /// Initialize camera and input for this local client.
        /// </summary>
        void Start()
        {
            //get corresponding team and colorize renderers in team color
            Team team = GameManager.GetInstance().teams[GetView().GetTeam()];
            // Material _material;
            // if (id < 6)
            // {
            //     if (id == 0)
            //         _material = team.material;
            //     else
            //         _material = team.customMaterial[id - 1];
            //     for (int i = 0; i < renderers.Length; i++)
            //         renderers[i].material = _material;
            // }
            flag.color = team.color;
            //set name in label
            label.text = GetView().GetName();
            //call hooks manually to update
            OnHealthChange(GetView().GetHealth());
            OnShieldChange(GetView().GetShield());

            //called only for this client 
            if (!photonView.IsMine)
                return;

            //set a global reference to the local player
            GameManager.GetInstance().localPlayer = this;

            //get components and set camera target
            rb = GetComponent<Rigidbody>();
            camFollow = Camera.main.GetComponent<FollowTarget>();
            camFollow.target = gameObject.transform;
            //set property

            //initialize input controls for mobile devices
            //[0]=left joystick for movement, [1]=right joystick for shooting
#if !UNITY_STANDALONE && !UNITY_WEBGL
            GameManager.GetInstance().ui.controls[0].onDrag += Move;
            GameManager.GetInstance().ui.controls[0].onDragEnd += MoveEnd;

            GameManager.GetInstance().ui.controls[1].onDragBegin += ShootBegin;
            GameManager.GetInstance().ui.controls[1].onDrag += RotateTurret;
            GameManager.GetInstance().ui.controls[1].onDrag += Shoot;
#endif
        }


        /// <summary>
        /// This method gets called whenever player properties have been changed on the network.
        /// </summary>
        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player player, ExitGames.Client.Photon.Hashtable playerAndUpdatedProps)
        {
            //only react on property changes for this player
            if (player != photonView.Owner)
                return;
            //Debug.Log(string.Format("Player.OnPlayerPropertiesUpdate: object {0}", gameObject.transform.name));
            if (GetComponent<PlayerBot>())
            {
                return;
            }
            //update values that could change any time for visualization to stay up to date
            OnHealthChange(player.GetHealth());
            OnShieldChange(player.GetShield());
        }

        protected void SetNftPropertys(NftTank _nftTank)
        {
            //tank property variables
            // fireRate = PlayerManager.instance.selectedTank.fireRate / 100f;
            // moveSpeed = PlayerManager.instance.selectedTank.speed;
            // exp = PlayerManager.instance.selectedTank.experience;
            // level = PlayerManager.instance.selectedTank.tankLevel;
            // maxHealth = PlayerManager.instance.selectedTank.health;
            //movement variables
            if (photonView.IsMine)
            {
                GameManager.GetInstance().ui.tankLevelUI.text = _nftTank.tankLevel.ToString();
                GameManager.GetInstance().ui.expUI.text = _nftTank.experience.ToString();
            }

            if (levelSlider != null)
            {
                levelSlider.value = Helper.GetExpSliderValue(_nftTank.experience);
            }
            if (energySlider != null)
            {
                energySlider.value = (float)_nftTank.energy / (float)_nftTank.maxEnergy;
            }
            if (levelUI != null)
            {
                levelUI.text = _nftTank.tankLevel.ToString();
                levelUI.text = _nftTank.tankLevel.ToString();
            }
            nft_id = _nftTank.id;
            exp = _nftTank.experience;
            level = _nftTank.tankLevel;
            maxHealth = _nftTank.health;
            fireRate = (float)_nftTank.fireRate / 100f;
            firePower = _nftTank.firePower / 100f;
            moveSpeed = (float)_nftTank.speed / 100f;
            energy = _nftTank.energy;
            maxEnergy = _nftTank.maxEnergy;
            energyPool = _nftTank.energyPool;
            maxEnergyPool = _nftTank.maxEnergyPool;
        }


        //this method gets called multiple times per second, at least 10 times or more
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //here we send the turret rotation angle to other clients
                stream.SendNext(turretRotation);
            }
            else
            {
                //here we receive the turret rotation angle from others and apply it
                this.turretRotation = (short)stream.ReceiveNext();
                OnTurretRotation();
            }
        }


        //continously check for input on desktop platforms
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        void FixedUpdate()
        {
            //skip further calls for remote clients    
            if (!photonView.IsMine)
            {
                //keep turret rotation updated for all clients
                OnTurretRotation();
                if (lastPosition.x == transform.position.x && lastPosition.z == transform.position.z)
                {
                    trackEffectObject.SetActive(false);
                }
                else
                {
                    trackEffectObject.SetActive(true);
                }
                lastPosition = transform.position;
                return;
            }
            //tank property variables
            // fireRate = PlayerManager.instance.selectedTank.fireRate / 100f;
            // moveSpeed = PlayerManager.instance.selectedTank.speed;
            // exp = PlayerManager.instance.selectedTank.experience;
            // level = PlayerManager.instance.selectedTank.tankLevel;
            // maxHealth = PlayerManager.instance.selectedTank.health;
            //movement variables
            Vector2 moveDir;
            Vector2 turnDir;
            //reset moving input when no arrow keys are pressed down
            if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            {
                moveDir.x = 0;
                moveDir.y = 0;
                trackEffectObject.SetActive(false);
            }
            else
            {
                //read out moving directions and calculate force
                moveDir.x = Input.GetAxis("Horizontal");
                moveDir.y = Input.GetAxis("Vertical");
                trackEffectObject.SetActive(true);
                Move(moveDir);
            }

            //cast a ray on a plane at the mouse position for detecting where to shoot 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.up);
            float distance = 0f;
            Vector3 hitPos = Vector3.zero;
            //the hit position determines the mouse position in the scene
            if (plane.Raycast(ray, out distance))
            {
                hitPos = ray.GetPoint(distance) - transform.position;
            }

            //we've converted the mouse position to a direction
            turnDir = new Vector2(hitPos.x, hitPos.z);

            //rotate turret to look at the mouse direction
            RotateTurret(new Vector2(hitPos.x, hitPos.z));

            //shoot bullet on left mouse click
            //if (Input.GetButton("Fire1"))
            //Shoot();
            //if (Input.GetButton("Jump"))
                //Shoot();

            //replicate input to mobile controls for illustration purposes
#if UNITY_EDITOR
            GameManager.GetInstance().ui.controls[0].position = moveDir;
            GameManager.GetInstance().ui.controls[1].position = turnDir;
#endif
        }
#endif
        private void Update()
        {
            if (PhotonNetwork.IsMasterClient && (GameMode)PlayerPrefs.GetInt(PrefsKeys.gameMode) == GameMode.ROYAL)
            {
                return;
            }
            if (GameManager.GetInstance().localPlayer.gameObject != this.gameObject) //photonView.IsMine)
            {
                return;
            }
            if((GameMode)PlayerPrefs.GetInt(PrefsKeys.gameMode) == GameMode.ROYAL && !PhotonNetwork.CurrentRoom.GetGameState())
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!isPossibleSpec) return;
                Spec_Manager.instance.curIdx = specIdx[0];
                StartCoroutine(Spec_Manager.instance.SetSkill(this.photonView));
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                if (!isPossibleSpec) return;
                Spec_Manager.instance.curIdx = specIdx[1];
                StartCoroutine(Spec_Manager.instance.SetSkill(this.photonView));
            }

            /* FireTank (idx 0) main shell: fire
            if (Input.GetButton("Fire1"))
            {
                if (GetComponent<Player>().id == SpecConsts.fireTank_ID )
                {
                    Shoot(SpecConsts.bulletType_Spec);
                    return;
                }
            }*/
            if (Input.GetButtonDown("Fire1"))
            {
                if (Spec_Manager.instance.curIdx == SpecConsts.cSpec_0201_IceShell
                    || Spec_Manager.instance.curIdx == SpecConsts.cSpec_0202_Rocket
                    || Spec_Manager.instance.curIdx == SpecConsts.cSpec_0401_Confusion
                    || Spec_Manager.instance.curIdx == SpecConsts.cSpec_0502_Confusion
                    || Spec_Manager.instance.curIdx == SpecConsts.cSpec_0602_Silence
                    )
                {
                     StartCoroutine(Spec_Manager.instance.FireSpec(photonView));
                    return;
                }/* // electric tank (04) main shell : electric shell
                else if (GetComponent<Player>().id == SpecConsts.electricTank_ID )
                {
                    Shoot(SpecConsts.bulletType_Spec);
                    return;
                }*/
                else
                {
                    if (GetComponent<Player>().bullets[0].GetComponent<Bullet>() && isFire )
                    {
                        Shoot();
                    }
                }

            }

        }


        /// <summary>
        /// Helper method for getting the current object owner.
        /// </summary>
        public PhotonView GetView()
        {
            return this.photonView;
        }


        //moves rigidbody in the direction passed in
        void Move(Vector2 direction = default(Vector2))
        {
            // if (collisonCount > 0)
            //   return;
            //lastPosition = rb.position;
            InsertBufPos(rb.position);
            //if direction is not zero, rotate player in the moving direction relative to camera
            if (direction != Vector2.zero)
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y))
                                     * Quaternion.Euler(0, camFollow.camTransform.eulerAngles.y, 0);

            //create movement vector based on current rotation and speed
            Vector3 movementDir = transform.forward * moveSpeed * Time.deltaTime;

            //apply vector to rigidbody position
            rb.MovePosition(rb.position + movementDir);

            // isStayCollision = false;
            //CheckMove(rb);
        }

        // For lock move
        // bool CheckMove(Rigidbody rb)
        // {
        //   if(isStayCollision){
        //     rb.position = lastPosition;
        //     return false;
        //   }
        //   return true;
        // }

        void OnCollisionStay(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.otherCollider.tag == "Border")
                {
                    if (stackPosition.Count > 1)
                    {
                        Vector3 beforePos = stackPosition[stackPosition.Count - 1];
                        stackPosition.RemoveAt(stackPosition.Count - 1);
                        if (beforePos != Vector3.zero)
                            rb.position = beforePos;
                        //rb.position = lastPosition;
                    }
                }
            }
        }

        void OnCollisionExit(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.otherCollider.tag == "Border")
                {
                    collisonCount--;
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.otherCollider.tag == "Border")
                {
                    collisonCount++;
                }
            }
        }


        //on movement drag ended
        void MoveEnd()
        {
            //reset rigidbody physics values
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        //rotates turret to the direction passed in
        void RotateTurret(Vector2 direction = default(Vector2))
        {
            //don't rotate without values
            if (direction == Vector2.zero)
                return;

            //get rotation value as angle out of the direction we received
            turretRotation = (short)Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)).eulerAngles.y;
            OnTurretRotation();
        }


        //on shot drag start set small delay for first shot
        void ShootBegin()
        {
            nextFire = Time.time + 0.25f;
        }


        //shoots a bullet in the direction passed in
        //we do not rely on the current turret rotation here, because we send the direction
        //along with the shot request to the server to absolutely ensure a synced shot position
        protected void Shoot(byte bulletType = SpecConsts.bulletType_Bullet, Vector2 direction = default(Vector2))
        {
            if ((GameMode)PlayerPrefs.GetInt(PrefsKeys.gameMode) == GameMode.ROYAL)
            {
                if (!PhotonNetwork.CurrentRoom.GetGameState())
                {
                    //UIManager.instance.Error("You cannot fire before starting combat ! :)");
                    //UIManager.instance.Error("You cannot fire before starting combat ! :)");
                    return;
                }
            }
            //if shot delay is over  
            if (Time.time > nextFire)
            {
                //set next shot timestamp

                //if (GetComponent<Player>().id == SpecConsts.fireTank_ID && bulletType == SpecConsts.bulletType_Spec)// 
                //nextFire = 0;
                //else 
                nextFire = Time.time + fireRate;

                //send current client position and turret rotation along to sync the shot position
                //also we are sending it as a short array (only x,z - skip y) to save additional bandwidth
                short[] pos = new short[] { (short)(shotPos.position.x * 10), (short)(shotPos.position.z * 10) };
                //send shot request with origin to server

                this.photonView.RPC("CmdShoot", RpcTarget.AllViaServer, pos, turretRotation, bulletType);
                // camera shake effect
                if (CameraShake.instance && !GetComponent<PlayerBot>())
                {
                    CameraShake.instance.ShootShake();
                }
            }
        }


        //called on the server first but forwarded to all clients
        [PunRPC]
        protected void CmdShoot(short[] position, short angle, byte bulletType)
        {
            //get current bullet type
            int currentBullet;
            currentBullet = GetView().GetBullet();

            //calculate center between shot position sent and current server position (factor 0.6f = 40% client, 60% server)
            //this is done to compensate network lag and smoothing it out between both client/server positions

            Vector3 shotCenter = Vector3.Lerp(shotPos.position, new Vector3(position[0] / 10f, shotPos.position.y, position[1] / 10f), 0.6f);
            Quaternion syncedRot = turret.rotation = Quaternion.Euler(0, angle, 0);

            GameObject obj;
            //if (bulletType == SpecConsts.bulletType_Spec)
            //obj = PoolManager.Spawn(bullets[0], shotCenter, syncedRot);
            //else
            obj = PoolManager.Spawn(bullets[currentBullet], shotCenter, syncedRot);

            //spawn bullet using pooling
            {

                obj.GetComponent<Bullet>().owner = gameObject;
                if ((int)firePower != 0)
                {
                    obj.GetComponent<Bullet>().damage = (int)firePower * obj.GetComponent<Bullet>().doubleRate / 100;
                }
                if (isReinforcedBullet)
                    obj.GetComponent<Bullet>().damage *= 2;
            }

            //check for current ammunition
            //let the server decrease special ammunition, if present
            if (PhotonNetwork.IsMasterClient && currentBullet != 0)
            {
                //if ran out of ammo: reset bullet automatically
                GetView().DecreaseAmmo(1);
            }

            //send event to all clients for spawning effects
            if (shotFX || shotClip)
                RpcOnShot();
        }


        //called on all clients after bullet spawn
        //spawn effects or sounds locally, if set
        protected void RpcOnShot()
        {
            if (shotFX) PoolManager.Spawn(shotFX, shotPos.position, shotPos.transform.rotation, this.turret);
            if (shotFX) PoolManager.Spawn(shotFX, shotPos.position, shotPos.transform.rotation, this.turret);
            if (shotClip) AudioManager.Play3D(shotClip, shotPos.position, 0.1f);
        }


        //hook for updating turret rotation locally
        void OnTurretRotation()
        {
            //we don't need to check for local ownership when setting the turretRotation,
            //because OnPhotonSerializeView PhotonStream.isWriting == true only applies to the owner
            //Debug.Log("curRot: "+ turret.transform.localRotation.eulerAngles.y + ", turretTotation: " + turretRotation);
            //short tempTurretRotation = 0;
            //tempTurretRotation = (short)Mathf.LerpAngle(turret.transform.localRotation.eulerAngles.y, turretRotation, 0.3f);
            //Debug.Log("tempTurretRotation: " + tempTurretRotation);
            turret.rotation = Quaternion.Euler(0, turretRotation, 0);
        }

        //hook for updating health locally
        //(the actual value updates via player properties)
        protected void OnHealthChange(int value)
        {
            //Debug.Log(string.Format("Player.OnHealthChange: tank {0}, value {1} >> ", gameObject.transform.name, value));
            int preValue = (int)(healthSlider.value * maxHealth);
            healthSlider.value = (float)value / maxHealth;
            int changedValue = value - preValue;
            if (changedValue < 0)
            {
                stateChangeAnimator.SetTrigger("StateChange");
                stateChangeText.text = (changedValue).ToString();
            }
        }

        //hook for updating shield locally
        //(the actual value updates via player properties)
        protected void OnShieldChange(int value)
        {
            shieldSlider.value = value;
        }


        /// <summary>
        /// Server only: calculate damage to be taken by the Player,
        /// triggers score increase and respawn workflow on death.
        /// </summary>
        public void TakeDamage(int damage, int timeStamp, GameObject owner = null)
        {
            // if fire tank is seted by shield spec
            if (isSpecShield)
                return;
            //_____________testcode ___________I don't want to be dead!!!!!!!!!!!!!!
            // (!GetComponent<PlayerBot>())
            //return;
            //_______________________________________________________________

            //store network variables temporary
            int health = GetView().GetHealth();
            int shield = GetView().GetShield();
            if (health == 0)
            {
                return;
            }
            //reduce shield on hit
            if (shield > 0)
            {
                GetView().DecreaseShield(1);
                return;
            }

            //substract health by damage
            //locally for now, to only have one update later on
            health -= damage;
            if (owner)
            {
                DamageEvent damageEvent = new DamageEvent();
                damageEvent.timeStamp = (short)(Time.time * 100);
                damageEvent.attackerID = owner.GetComponent<Player>().GetView().ViewID;
                damageEvent.victimID = GetView().ViewID;
                damageEvent.victimHealth = health <= 0 ? 0 : health;
                damageEvent.spawnTime = timeStamp;
                GameManager.GetInstance().AddDamageEvent(damageEvent);
                if (CameraShake.instance && !GetComponent<PlayerBot>())
                {
                    CameraShake.instance.HitShake();
                }
                if (CameraShake.instance && !GetComponent<PlayerBot>())
                {
                    CameraShake.instance.HitShake();
                }
                //Debug.Log("[Player.TakeDamage] add damage event to gameManager");
            }
            //bullet killed the player
            if (health <= 0)
            {
                if (owner)
                {
                    Player other = owner.GetComponent<Player>();
                    //Add exp handler
                    if (PhotonNetwork.IsMasterClient && other.socketID != "")
                    {
                        SocketGameManager.instance.AddExperience(other.socketID, other.nft_id, level);
                    }

                    //the game is already over so don't do anything
                    if (GameManager.GetInstance().IsGameOver()) return;

                    //get killer and increase score for that enemy team

                    int otherTeam = other.GetView().GetTeam();
                    if (GetView().GetTeam() != otherTeam)
                        GameManager.GetInstance().AddScore(ScoreType.Kill, otherTeam);

                    //the maximum score has been reached now
                    if (GameManager.GetInstance().IsGameOver())
                    {
                        //close room for joining players
                        PhotonNetwork.CurrentRoom.IsOpen = false;
                        //tell all clients the winning team
                        this.photonView.RPC("RpcGameOver", RpcTarget.All, (byte)otherTeam);
                        return;
                    }
                }

                //the game is not over yet, reset runtime values
                //also tell all clients to despawn this player

                GetView().SetHealth(0);
                GetView().SetBullet(0);

                //clean up collectibles on this player by letting them drop down
                Collectible[] collectibles = GetComponentsInChildren<Collectible>(true);
                for (int i = 0; i < collectibles.Length; i++)
                {
                    PhotonNetwork.RemoveRPCs(collectibles[i].spawner.photonView);
                    collectibles[i].spawner.photonView.RPC("Drop", RpcTarget.AllBuffered, transform.position);
                }

                //tell the dead player who killed him (owner of the bullet)
                short senderId = 0;
                if (owner != null)
                    senderId = (short)owner.GetComponent<PhotonView>().ViewID;
                this.photonView.RPC("RpcRespawn", RpcTarget.All, senderId);

                //royal mode
                if (GameManager.GetInstance().IsGameOver())
                {
                    //close room for joining players
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    //tell all clients the winning team
                    this.photonView.RPC("RpcGameOver", RpcTarget.All, (byte)GameManager.GetInstance().GetRoyalWinTeamIndex());
                    return;
                }
            }
            else
            {
                //we didn't die, set health to new value
                GetView().SetHealth(health);
            }
        }


        //called on all clients on both player death and respawn
        //only difference is that on respawn, the client sends the request
        [PunRPC]
        protected virtual void RpcRespawn(short senderId, PhotonMessageInfo info)
        {
            //toggle visibility for player gameobject (on/off)
            Player killedPlayer = info.photonView.gameObject.GetComponent<Player>();
            gameObject.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
            gameObject.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
            gameObject.SetActive(!gameObject.activeInHierarchy);
            bool isActive = gameObject.activeInHierarchy;
            killedBy = null;

            //the player has been killed
            if (!isActive)
            {


                //find original sender game object (killedBy)
                PhotonView senderView = senderId > 0 ? PhotonView.Find(senderId) : null;
                if (senderView != null && senderView.gameObject != null) killedBy = senderView.gameObject;

                //detect whether the current user was responsible for the kill, but not for suicide
                //yes, that's my kill: increase local kill counter
                if (!PhotonNetwork.IsMasterClient)
                {
                    if (this != GameManager.GetInstance().localPlayer && killedBy == GameManager.GetInstance().localPlayer.gameObject)
                    {
                        GameManager.GetInstance().ui.killCounter[0].text = (int.Parse(GameManager.GetInstance().ui.killCounter[0].text) + 1).ToString();
                        GameManager.GetInstance().ui.killCounter[0].GetComponent<Animator>().Play("Animation");
                    }
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
            }

            if (PhotonNetwork.IsMasterClient)
            {
                if (killedPlayer != null && isActive == true)
                {
                    SocketGameManager.instance.SpawnTank(killedPlayer.GetView().ViewID, killedPlayer.nft_id, (NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode));
                }

                //send player back to the team area, this will get overwritten by the exact position from the client itself later on
                //we just do this to avoid players "popping up" from the position they died and then teleporting to the team area instantly
                //this is manipulating the internal PhotonTransformView cache to update the networkPosition variable
                GetComponent<PhotonTransformView>().OnPhotonSerializeView(new PhotonStream(false, new object[] { GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam()),
                                                                                                                 Vector3.zero, Quaternion.identity }), new PhotonMessageInfo());
            }

            //further changes only affect the local client
            if (!photonView.IsMine)
                return;

            //local player got respawned so reset states
            if (isActive == true)
            {
                ResetPosition();
                GetView().SetHealth(maxHealth);
            }
            else
            {
                DisPlayDeath();
            }
        }

        void DisPlayDeath()
        {
            float displayTime = 0;
            // if(explosionFX)
            // {
            //     var pSystems = explosionFX.GetComponentsInChildren<ParticleSystem>();
            //     //otherwise find the maximum particle duration
            //     for(int i = 0; i < pSystems.Length; i++)
            //     {
            //         var main = pSystems[i].main;
            //         if(main.duration > displayTime)
            //             displayTime = main.duration;
            //     }
            // }
            //yield return new WaitForSeconds(displayTime);

            //local player was killed, set camera to follow the killer
            if (killedBy != null) camFollow.target = killedBy.transform;
            //hide input controls and other HUD elements
            camFollow.HideMask(true);
            //display respawn window (only for local player)
            GameManager.GetInstance().DisplayDeath();
        }



        /// <summary>
        /// Command telling the server and all others that this client is ready for respawn.
        /// This is when the respawn delay is over or a video ad has been watched.
        /// </summary>
        public void CmdRespawn()
        {
            this.photonView.RPC("RpcRespawn", RpcTarget.AllViaServer, (short)0);
        }

        public void CmdUpdateTank(NftTank tank)
        {
            object[] content = new object[] { tank.id, tank.tankLevel, tank.experience, tank.health, tank.speed, tank.fireRate, tank.firePower, tank.energy, tank.maxEnergy, tank.energyPool, tank.maxEnergyPool, tank.name };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(UpdateTankEventCode, content, raiseEventOptions, SendOptions.SendReliable);
            //this.photonView.RPC("RpcUpdateTank", RpcTarget.AllViaServer, tank.id, tank.tankLevel, tank.experience, tank.health, tank.speed, tank.fireRate, tank.firePower);
        }

        public void CmdUpdateEnergy(int energy)
        {
            this.photonView.RPC("RpcUpdateEnergy", RpcTarget.All, energy);
        }
        [PunRPC]
        public void RpcUpdateEnergy(int _energy)
        {
            if (energySlider != null)
            {
                this.energy = _energy;
                energySlider.value = (float)_energy / (float)maxEnergy;
            }
        }
        // [PunRPC]
        // public void RpcUpdateTank(string _nft_id, int _level, int _exp, int _health, int _speed, int _fireRate, int _firePower)
        // {
        //     if (nft_id != _nft_id)
        //     {
        //         return;
        //     }
        //     SetNftPropertys(_nft_id, _level, _exp, _health, _speed, _fireRate, _firePower);
        // }


        /// <summary>
        /// Repositions in team area and resets camera & input variables.
        /// This should only be called for the local player.
        /// </summary>
        public void ResetPosition()
        {
            //start following the local player again
            camFollow.target = turret;
            camFollow.HideMask(false);

            //get team area and reposition it there
            transform.position = GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam());

            //reset forces modified by input
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.identity;
            //reset input left over
            GameManager.GetInstance().ui.controls[0].OnEndDrag(null);
            GameManager.GetInstance().ui.controls[1].OnEndDrag(null);
        }


        //called on all clients on game end providing the winning team
        [PunRPC]
        protected void RpcGameOver(byte teamIndex)
        {
            //display game over window
            GameManager.GetInstance().DisplayGameOver(teamIndex);
        }

        //[PunRPC]
        public void SpeedDown(float speedTimes)
        {
            if (GetComponent<NavMeshAgent>())
            {
                if (curMoveSpeed == 0f)
                {
                    curMoveSpeed = GetComponent<NavMeshAgent>().speed;

                    if (speedTimes != 0)
                        GetComponent<NavMeshAgent>().speed /= speedTimes;// SpecConsts.speedTimes;
                    else
                        GetComponent<NavMeshAgent>().speed = 0f;
                }
            }
            else
            {
                if (curMoveSpeed == 0f)
                {
                    curMoveSpeed = moveSpeed;

                    if (speedTimes != 0)
                        moveSpeed /= speedTimes;//SpecConsts.speedTimes;
                    else
                        moveSpeed = 0f;
                }
            }
        }

        //[PunRPC]
        public void SpeedReturn()
        {
            if (GetComponent<NavMeshAgent>())
            {
                if (curMoveSpeed != 0f)
                {
                    GetComponent<NavMeshAgent>().speed = curMoveSpeed;
                    curMoveSpeed = 0f;
                }
            }
            else
            {
                if (curMoveSpeed != 0f)
                {
                    moveSpeed = curMoveSpeed;
                    curMoveSpeed = 0f;
                }
            }
        }

        [PunRPC]
        protected void CmdCallAreaMissileTakeDamage()
        {
            this.TakeDamage(30, 0);
        }

    }
}