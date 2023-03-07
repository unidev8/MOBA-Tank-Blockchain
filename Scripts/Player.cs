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

using ExitGames.Client.Photon;
using Unity.VisualScripting;
using System.Net.Sockets;
using UnityEditorInternal;
using Knife.ScifiEffects;
using System.ComponentModel;
using UnityEngine.Rendering.RendererUtils;

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
        public Transform shieldPos;

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
        //Vector3 lastPosition = Vector3.zero;
        List<Vector3> stackPosition = new List<Vector3>();
        bool isStayCollision = false;
        int collisonCount = 0;
        #endregion
  //---------------------- Speciall skill Start---------------//      
        [SerializeField]
        private GameObject[] Specs;

        private GameObject particleObj;
        private GameObject shieldObj;
        private GameObject speedyObj;
        private GameObject areaMissileObj;        
        private GameObject reinforceCanon;
        private GameObject speedyCanon;

        private GameObject spec_02_IndicatorObj;
        private GameObject iceObj;
        private GameObject electricObj;

        public bool isAreaMissil = false;
        public bool isIceCanon = false;
        public bool isElectricCanon = false;
        public bool isWindWalk = false;
        
        private Vector3 hitPos;

        private float countDownTime = 8.0f;
        private short countDown = 5;

        private float nextSpeedySkill;
        private float nextShieldSkill;
        private float nextIceCanonSkill;
        private float nextAreaMissileSkill;
        private float nextSpeedyCanonSkill;
        private float nextReinforceCanonSkill;
        private float nextElectricSkill;

        private Material originalMat;
        ///---------------------- Special Skill End---------------------//


        //initialize server values for this player
        void Awake()
        {
            //only let the master do initialization
            if (!PhotonNetwork.IsMasterClient)
                return;

            stackPosition.Clear();
            isAreaMissil = false;
            //set players current health value after joining

            //GetView().SetHealth(maxHealth);
            print("tank Awake!" + gameObject.name);
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
            //PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
            if (socketID != "" && nft_id != "")
            {
                SocketGameManager.instance.GetEnergy(socketID, nft_id, -1);
            }
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            //PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
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

                SetNftPropertys(PlayerManager.instance.selectedTank);
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {

            PhotonNetwork.EnableCloseConnection = true;
            if (PhotonNetwork.IsMasterClient)
            {
                //SocketGameManager.instance.
            }
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
            SocketGameManager.instance.Killed(socketID, instantiationData[1].ToString(), 0);
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
            camFollow.target = turret;
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
                levelUI.text = "Lv" + _nftTank.tankLevel;
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
      }
      else
      {
        //read out moving directions and calculate force
        moveDir.x = Input.GetAxis("Horizontal");
        moveDir.y = Input.GetAxis("Vertical");
        Move(moveDir);
      }

      //cast a ray on a plane at the mouse position for detecting where to shoot 
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      Plane plane = new Plane(Vector3.up, Vector3.up);
      float distance = 0f;
      hitPos = Vector3.zero;

      //the hit position determines the mouse position in the scene
      if (plane.Raycast(ray, out distance))
      {
        hitPos = ray.GetPoint(distance) - transform.position;
      }

      //we've converted the mouse position to a direction
      turnDir = new Vector2(hitPos.x, hitPos.z);

      //rotate turret to look at the mouse direction
      RotateTurret(new Vector2(hitPos.x, hitPos.z));          
            
            //replicate input to mobile controls for illustration purposes
#if UNITY_EDITOR
            GameManager.GetInstance().ui.controls[0].position = moveDir;
            GameManager.GetInstance().ui.controls[1].position = turnDir;
#endif
    }
#endif
        void Update ()
        {
            if (photonView.IsMine && gameObject.GetComponent<PlayerBot>()) return;

            float distance;
            Vector3 mouseWorldPos;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, 0);
            if (plane.Raycast(ray, out distance))
            {
                mouseWorldPos = ray.GetPoint(distance);
            }
            else
                mouseWorldPos = Vector3.zero;

            if (spec_02_IndicatorObj)
            {
                float distanceIndicate = ( mouseWorldPos - transform.position).magnitude;
                if (distanceIndicate < 15f)
                    spec_02_IndicatorObj.transform.position = mouseWorldPos;
            }

            //shoot bullet on left mouse click
            if ( !isAreaMissil && !isElectricCanon && !isIceCanon )
            {
                //Debug.Log("PlayerUpdate: isIceCanon=" + isIceCanon + ", isElectriCanon=" + isElectricCanon + ", isAreaMissil" + isAreaMissil);
                if (Input.GetButton("Fire1"))                
                    FireFlame(); //Shoot();
            }
            else
            {
                //Debug.Log("PlayerUpdate: isIceCanon=" + isIceCanon + ", isElectriCanon=" + isElectricCanon + ", isAreaMissil" + isAreaMissil);
                if (Input.GetButtonDown("Fire1"))
                {
                    if (isAreaMissil)
                    {
                        CallAreaMissile(spec_02_IndicatorObj.transform.position);
                        //Debug.Log("Player.Update:2- mousePos =" + Input.mousePosition + ", worldPos =" + worldPos);                    
                    }
                    else if (isElectricCanon)
                    {
                        //Debug.Log("Player.Update: Eectric Obj !");
                        FireElectric();
                    }
                    else if (isIceCanon)
                    {
                        FireIce();
                    }
                }

            }

            if (Input.GetButton("Jump"))
                Shoot();
            
            if(Input.GetKeyDown(KeyCode.Alpha1) && Time.time > nextSpeedySkill )
            {                
                EquipSpeedy();
                nextSpeedySkill = Time.time + countDownTime;                 
            }
            if(Input.GetKeyDown(KeyCode.Alpha2) && Time.time > nextShieldSkill)
            {
                EquipShield();
                nextShieldSkill = Time.time + countDownTime;
            }
            if(Input.GetKeyDown(KeyCode.Alpha3) )
            {
                //if (Time.time > nextIceCanonSkill)
                {                    
                    nextIceCanonSkill = Time.time + countDownTime;
                    ResetFireSate();
                    isIceCanon = true;
                }
                //else
                    //isIceCanon = false;
            }
            if(Input.GetKeyDown(KeyCode.Alpha4))
            {
                isAreaMissil = !isAreaMissil;
                //if (Time.time > nextAreaMissileSkill)
                if (isAreaMissil)
                {                    
                    nextAreaMissileSkill = Time.time + countDownTime;
                    GameObject IndicatorObj = Specs[4].GetComponent<spec_02_AreaMissile>().indicatorObj;
                    spec_02_IndicatorObj = PoolManager.Spawn(IndicatorObj, new Vector3(mouseWorldPos.x, 1f, mouseWorldPos.z), Quaternion.identity);
                    ResetFireSate();
                    isAreaMissil = true;
                }
                //else
                else
                {
                    if (spec_02_IndicatorObj)
                    {
                        PoolManager.Despawn(spec_02_IndicatorObj, 0f);
                    }
                    //isAreaMissil = false;
                }
            }
            if(Input.GetKeyDown(KeyCode.Alpha5) && Time.time > nextReinforceCanonSkill)
            {
                ReinforceCanon();
                nextReinforceCanonSkill = Time.time + countDownTime;
            }
            if(Input.GetKeyDown(KeyCode.Alpha6) && Time.time > nextSpeedyCanonSkill)
            {                
                SpeedyCanon();
                nextSpeedyCanonSkill = Time.time + countDownTime;

            }
            if (Input.GetKeyDown (KeyCode.Alpha7) )
            {
                //if (Time.time > nextElectricSkill)
                {                    
                    nextElectricSkill = Time.time + countDownTime;
                    ResetFireSate();
                    isElectricCanon = true;
                }    
                //else
                    //isElectricCanon = false;
            }
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                isWindWalk = !isWindWalk;
                if (isWindWalk)
                    WindWalker(true);
                else
                    WindWalker(false);
                //ResetFireSate();
            }

            if(shieldObj != null)
            {
                shieldObj.transform.position = shieldPos.position;
                shieldObj.transform.rotation = transform.rotation;
            }
            if(speedyObj != null)
            {
                speedyObj.transform.position = shieldPos.position;
                speedyObj.transform.rotation = transform.rotation;
            }
            if (speedyCanon != null)
            {
                speedyCanon.transform.position = shieldPos.position;
                speedyCanon.transform.rotation = transform.rotation;
            }
            if (reinforceCanon != null)
            {
                reinforceCanon.transform.position = shieldPos.position;
                reinforceCanon.transform.rotation = transform.rotation;
            }
            

        }

        private void ResetFireSate()
        {
            isAreaMissil = false;
            isIceCanon = false;
            isElectricCanon = false;
        }

        protected void WindWalker(bool isWindWalker)
        {
            if (isWindWalker)
                photonView.RPC("CmdWindWalker", RpcTarget.AllViaServer, true);
            else
                photonView.RPC("CmdWindWalker", RpcTarget.AllViaServer, false);
        }

        [PunRPC]
        protected void CmdWindWalker(bool isWindWalker)
        {
            if (isWindWalker)
            {
                if (photonView.IsMine)
                {
                    SetWindWalker(this.gameObject, true, 5f);
                }
                else
                {
                    SetWindWalker(this.gameObject, true, 1f);
                }
            }
            else
            {
                SetWindWalker(this.gameObject, false );
            }
           
        }

        private void SetWindWalker(GameObject obj, bool direction, float frenselPower = 0f )
        {
            if (null == obj)
                return;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                    continue;
                try
                {                    
                    //Debug.Log("SetMaterialAlpha: Obj = " + child.gameObject);
                    if (child.gameObject.name == "HUD" || child.gameObject.name == "Shield")
                    {
                        if (direction)
                            child.gameObject.SetActive(false);
                        else
                            child.gameObject.SetActive(true);
                    } 
                    //SetWindWalker(child.gameObject);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                
            }
            EnableShadow(this.gameObject, direction);

            foreach (Renderer objectRenderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                if (direction)
                {
                    originalMat = objectRenderer.material;// child.GetComponent<Material>();
                    objectRenderer.material = Resources.Load("Force Field", typeof(Material)) as Material;
                    objectRenderer.material.SetFloat("FresnelPower", frenselPower);
                    Debug.Log("SetWinWalk: frenselPower = " + frenselPower + ", objectRenderer" + objectRenderer.material);
                }
                else
                {
                    if (originalMat)
                        objectRenderer.material = originalMat;
                }

                Debug.Log("SetMaterialAlpha:_MaterialObj = " + objectRenderer.gameObject + ", Material =" + originalMat);
            }           
           
        }

        private void EnableShadow (GameObject obj, bool isWindWalker)
        {
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                    continue;
                try
                {
                    Debug.Log("EnableShadow: Obj = " + child.gameObject);
                    if (child.gameObject.GetComponent <MeshRenderer>())
                    {                        
                        if (isWindWalker)
                            child.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        else
                            child.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    }
                    EnableShadow(child.gameObject, isWindWalk);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

            }
        }

       
        protected void ReinforceCanon()
        {
            short[] pos = new short[] { (short)(shieldPos.position.x * 10), (short)(shieldPos.position.z * 10) };
            photonView.RPC("CmdReinforceCanon", RpcTarget.AllViaServer, pos);            
        }

        [PunRPC]
        protected void CmdReinforceCanon(short[] pos)
        {
            reinforceCanon = PoolManager.Spawn(Specs[5], new Vector3(pos[0] / 10f, 3f, pos[1] / 10f), transform.rotation);            
        }

        protected void SpeedyCanon()
        {
            short[] pos = new short[] { (short)(shieldPos.position.x * 10), (short)(shieldPos.position.z * 10) };
            photonView.RPC("CmdSpeedyCanon", RpcTarget.AllViaServer, pos);
        }

        [PunRPC]
        protected void CmdSpeedyCanon(short[] pos)
        {
            speedyCanon= PoolManager.Spawn(Specs[6], new Vector3(pos[0] / 10f, 1f, pos[1] / 10f), transform.rotation);            
        }

        protected void EquipSpeedy()
        {
            short[] pos = new short[] { (short)(shieldPos.position.x * 10), (short)(shieldPos.position.z * 10) };
            photonView.RPC("CmdEquipSpeedy", RpcTarget.AllViaServer, pos);            
        }

        [PunRPC]
        protected void CmdEquipSpeedy(short[] pos)
        {
            speedyObj = PoolManager.Spawn(Specs[1], new Vector3(pos[0] / 10f, 1f, pos[1]/10f), transform.rotation);            
        }

        protected void EquipShield()
        {  
            short[] pos = new short[] { (short)(shieldPos.position.x * 10), (short)(shieldPos.position.z * 10) };            
            photonView.RPC("CmdEquipShield", RpcTarget.AllViaServer, pos);
            //Debug.Log("Player.Equipshield: PooledObj=" + shieldObj);
        }

        [PunRPC]
        protected void CmdEquipShield( short[] pos)
        {
            shieldObj = PoolManager.Spawn(Specs[2], new Vector3(pos[0]/10f, 0f, pos[1]/10f), Quaternion.identity);            
        }

        protected void CallAreaMissile(Vector3 worldPos)
        {
            short[] pos = new short[] { (short)(worldPos.x * 10), (short)(worldPos.z * 10) };
            photonView.RPC("CmdCallAreaMissile", RpcTarget.AllViaServer, pos); 
            
        }

        [PunRPC]
        protected void CmdCallAreaMissile(short[] pos)
        {
            areaMissileObj = PoolManager.Spawn(Specs[4], new Vector3(pos[0] / 10f, 0f, pos[1] / 10f), Quaternion.identity);
            areaMissileObj.GetComponent<spec_02_AreaMissile>().owner = gameObject;
        }

        protected void FireFlame( Vector2 direction = default(Vector2))
        {
            //if (Time.time > s_nextFire)
            {
                //s_nextFire = Time.time + s_fireRate;
                short[] pos = new short[] { (short)(shotPos.position.x * 10), (short)(shotPos.position.z * 10) };
                this.photonView.RPC("CmdFireFlame", RpcTarget.AllViaServer, pos, turretRotation );
            }
        }

        [PunRPC]
        protected void CmdFireFlame( short[] position, short angle )
        {
            Vector3 shotCenter = Vector3.Lerp(shotPos.position, new Vector3(position[0] / 10f, shotPos.position.y, position[1] / 10f), 0.6f);
            Quaternion syncedRot = turret.rotation = Quaternion.Euler(0, angle, 0);
            //Debug.Log("Player.CmdDoSpec: Specs=" + Specs[idx] + ", shotCenter =" + shotCenter + ", synceRot=" + syncedRot );
            particleObj = PoolManager.Spawn(Specs[0], shotCenter, syncedRot);
            particleObj.GetComponent<Spec_01_Fire>().owner = gameObject;
            if ((int)firePower != 0)
            {
                //particleObj.GetComponent<Spec>().damage = (int)firePower * particleObj.GetComponent<Spec>().doubleRate / 100;
            }
            //if (shotFX || shotClip)
            //RpcOnShot();
        }

        protected void FireIce()
        {
            short[] pos = new short[] { (short)(shotPos.position.x * 10), (short)(shotPos.position.z * 10) };
            this.photonView.RPC("CmdFireIce", RpcTarget.AllViaServer, pos, turretRotation);
            //Debug.Log("FIreIce");
        }

        [PunRPC]
        protected void CmdFireIce(short[] position, short angle)
        {
            Vector3 shotCenter = Vector3.Lerp(shotPos.position, new Vector3(position[0] / 10f, shotPos.position.y, position[1] / 10f), 0.6f);
            Quaternion syncedRot = turret.rotation = Quaternion.Euler(0, angle, 0);
            iceObj = PoolManager.Spawn(Specs[3], shotCenter, syncedRot);
            iceObj.GetComponent<Spec_02_Ice>().owner = gameObject;
            //Debug.Log("CmdFireIce: iceObj=" + iceObj.ToString ());
        }

        protected void FireElectric()
        {
            short[] pos = new short[] { (short)(shotPos.position.x * 10), (short)(shotPos.position.z * 10) };
            photonView.RPC("CmdElectricCanon", RpcTarget.AllViaServer, pos, turretRotation);
            //Debug.Log("FireElectric");
        }

        [PunRPC]
        protected void CmdElectricCanon(short[] position, short angle)
        {
            Vector3 shotCenter = Vector3.Lerp(shotPos.position, new Vector3(position[0] / 10f, shotPos.position.y, position[1] / 10), 0.6f);
            Quaternion syncedRot = turret.rotation = Quaternion.Euler(0, angle, 0);
            electricObj = PoolManager.Spawn(Specs[7], shotCenter, syncedRot);
            //electricObj.GetComponent<Spec_04_Electric>().lightningObj.GetComponent<Lightning>().SetCustomActive(true);
            electricObj.GetComponent<Spec_04_Electric>().lightningObj.GetComponent <Lightning>().owner = gameObject;
            //Debug.Log("CmdElectricCanon: electricObj=" + electricObj.ToString());
        }

        //shoots a bullet in the direction passed in
        //we do not rely on the current turret rotation here, because we send the direction
        //along with the shot request to the server to absolutely ensure a synced shot position
        protected void Shoot(Vector2 direction = default(Vector2))
        {
            //if shot delay is over  
            if (Time.time > nextFire)
            {
                //set next shot timestamp
                nextFire = Time.time + fireRate;

                //send current client position and turret rotation along to sync the shot position
                //also we are sending it as a short array (only x,z - skip y) to save additional bandwidth
                short[] pos = new short[] { (short)(shotPos.position.x * 10), (short)(shotPos.position.z * 10) };
                //send shot request with origin to server
                this.photonView.RPC("CmdShoot", RpcTarget.AllViaServer, pos, turretRotation);
            }
        }

        //called on the server first but forwarded to all clients
        [PunRPC]
        protected void CmdShoot(short[] position, short angle)
        {
            //get current bullet type
            int currentBullet = GetView().GetBullet();

            //calculate center between shot position sent and current server position (factor 0.6f = 40% client, 60% server)
            //this is done to compensate network lag and smoothing it out between both client/server positions
            Vector3 shotCenter = Vector3.Lerp(shotPos.position, new Vector3(position[0] / 10f, shotPos.position.y, position[1] / 10f), 0.6f);
            Quaternion syncedRot = turret.rotation = Quaternion.Euler(0, angle, 0);

            //spawn bullet using pooling
            GameObject obj = PoolManager.Spawn(bullets[currentBullet], shotCenter, syncedRot);
            obj.GetComponent<Bullet>().owner = gameObject;
            if ((int)firePower != 0)
            {
                obj.GetComponent<Bullet>().damage = (int)firePower * obj.GetComponent<Bullet>().doubleRate / 100;
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

        public void TakeDamage_Fire(Spec_01_Fire spec)
        {
            int health = GetView().GetHealth();
            int shield = GetView().GetShield();

            if (shield > 0)
            {
                GetView().DecreaseShield(1);
                return;
            }
            health -= spec.damage;
            //Spec killed the player.
            if (health <= 0)
            {
                Player other = spec.owner.GetComponent<Player>();
                if (PhotonNetwork.IsMasterClient && other.socketID != "")
                {
                    SocketGameManager.instance.AddExperience(other.socketID, other.nft_id, level);
                }
                //the game is already over so don't do anything
                if (GameManager.GetInstance().IsGameOver()) return;
                // get killer and increase socre for that enemy team
                int otherTeam = other.GetView().GetTeam();
                if (GetView().GetTeam() != otherTeam)
                {
                    GameManager.GetInstance().AddScore(ScoreType.Kill, otherTeam);
                }
                if (GameManager.GetInstance().IsGameOver())
                {
                    //Close room for joinning players
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    // tell all clients the winning team
                    this.photonView.RPC("RpcGameOver", RpcTarget.All, (byte)otherTeam);
                    return;
                }
                // the gaem is not over yet, reset runtime values
                // also tell all clients to despawn this player
                GetView().SetHealth(maxHealth);
                //GetView().SetSpec(idx);

                //clean up collectibles on this player by letting them drop down
                Collectible[] collectibles = GetComponentsInChildren<Collectible>(true);
                for (int i = 0; i < collectibles.Length; i++)
                {
                    PhotonNetwork.RemoveRPCs(collectibles[i].spawner.photonView);
                    collectibles[i].spawner.photonView.RPC("Drop", RpcTarget.AllBuffered, transform.position);
                }
                // tell the dead player who killed him(owner of the bullet)
                short senderId = 0;
                if (spec.owner != null)
                    senderId = (short)spec.owner.GetComponent<PhotonView>().ViewID;
                this.photonView.RPC("RpcRespawn", RpcTarget.All, senderId);
            }
            else
            {
                // we didn't die, set health to new value
                GetView().SetHealth(health);
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


        //called on all clients after bullet spawn
        //spawn effects or sounds locally, if set
        protected void RpcOnShot()
        {
            if (shotFX) PoolManager.Spawn(shotFX, shotPos.position, Quaternion.identity);
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
            healthSlider.value = (float)value / maxHealth;
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
        public void TakeDamage(Bullet bullet)
        {
            //store network variables temporary
            int health = GetView().GetHealth();
            int shield = GetView().GetShield();

            //reduce shield on hit
            if (shield > 0)
            {
                GetView().DecreaseShield(1);
                return;
            }

            //substract health by damage
            //locally for now, to only have one update later on
            health -= bullet.damage;

            //bullet killed the player
            if (health <= 0)
            {
                Player other = bullet.owner.GetComponent<Player>();
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

                //the game is not over yet, reset runtime values
                //also tell all clients to despawn this player
                GetView().SetHealth(maxHealth);
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
                if (bullet.owner != null)
                    senderId = (short)bullet.owner.GetComponent<PhotonView>().ViewID;

                this.photonView.RPC("RpcRespawn", RpcTarget.All, senderId);
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
            print(info.photonView.gameObject.GetComponent<Player>().socketID);
            //toggle visibility for player gameobject (on/off)
            Player killedPlayer = info.photonView.gameObject.GetComponent<Player>();
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
                if (this != GameManager.GetInstance().localPlayer && killedBy == GameManager.GetInstance().localPlayer.gameObject)
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
            }

            if (PhotonNetwork.IsMasterClient)
            {
                if (killedPlayer != null && isActive == true)
                {
                    SocketGameManager.instance.Killed(killedPlayer.socketID, killedPlayer.nft_id, killedPlayer.level);
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
                ResetPosition();
            else
            {
                //local player was killed, set camera to follow the killer
                if (killedBy != null) camFollow.target = killedBy.transform;
                //hide input controls and other HUD elements
                camFollow.HideMask(true);
                //display respawn window (only for local player)
                GameManager.GetInstance().DisplayDeath();
            }
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
    }
}