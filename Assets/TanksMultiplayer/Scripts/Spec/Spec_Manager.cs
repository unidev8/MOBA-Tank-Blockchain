using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using Photon.Pun;

using TanksMP;

public class Spec_Manager : MonoBehaviourPunCallbacks
{
    public GameObject[] mainSpecs;
    public GameObject[] subSpecs;
    public GameObject sldCooldown;
    public GameObject sldValualbe;

    [HideInInspector]
    public byte curIdx = SpecConsts.cSpec_noSpec;
    [HideInInspector]
    public float cooldownStart = 0f;
    //public byte specType; // 0 = property type, 1 = shooting type

    public static Spec_Manager instance = null;

    private GameObject skillMainObj;
    private GameObject skillSubObj;


    public static Spec_Manager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Spec_Manager();
            }
            return instance;
        }
    }

    //private Transform shortPos, turret;
    private byte curFireNum = 0;

    private Vector3 mouseWorldPos = Vector3.zero;
    private GameObject playerObject;
    private float countdownTime;
    [HideInInspector]
    public bool isIndicatorPossible = false;
    private bool triggerIndicator= true;

    private Material originalMat;
    private bool isWindWalk = false;
    private PhotonView _playerPhotonView;


    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        instance = this;
        curIdx = SpecConsts.cSpec_noSpec;
    }

    private void Update()
    {
        if (curIdx == SpecConsts.cSpec_0202_Rocket 
            || curIdx == SpecConsts.cSpec_0502_Confusion 
            || curIdx == SpecConsts.cSpec_0602_Silence)
        {
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, 0);
            if (plane.Raycast(ray, out distance))
            {
                mouseWorldPos = ray.GetPoint(distance);
            }
            else
                mouseWorldPos = Vector3.zero;
            if (playerObject)
            {
                float distanceIndicate = (mouseWorldPos - playerObject.transform.position).magnitude;
                if (distanceIndicate < 25f)
                {
                    if (skillMainObj)
                        skillMainObj.transform.position = mouseWorldPos;
                }
            }            
        }

        if (Time.time > countdownTime && isWindWalk)
        {
            WindWalker(_playerPhotonView, false);
            isWindWalk = false;
            cooldownStart = Time.time;
        }

    }

    public IEnumerator SetSkill(PhotonView playerPhotonView)
    {
        switch (curIdx)
        {
            case SpecConsts.cSpec_0101_FastSpeedy://0
                {
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break;
                    if (Time.time > countdownTime)
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        EquipSpeedy(playerPhotonView.ViewID);
                    }
                    break;
                }
            case SpecConsts.cSpec_0102_Shield:
                {
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break; // cooldown time after skill end
                    if (Time.time > countdownTime) // defend repeat using from skill begining to skill ending
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        EquipShield(playerPhotonView);
                    }
                    break;
                }
            case SpecConsts.cSpec_0201_IceShell:
                {                    
                    break;
                }
            case SpecConsts.cSpec_0202_Rocket:
                {
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break; 
                    //if (Time.time > countdownTime)
                    if (triggerIndicator)
                    {
                        //countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        SetAreaMissileIndicator(playerPhotonView);
                        triggerIndicator = false;
                        isIndicatorPossible = true;
                        Debug.Log("cSpec_0202_Rocket is applied");
                    }
                    break;
                }
            case SpecConsts.cSpec_0301_2xShell:
                {                    
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break;
                    if (Time.time > countdownTime)
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        ReinforceCanon(playerPhotonView);
                    }
                    break;
                }
            case SpecConsts.cSpec_0302_AttactkSpeed:
                {                    
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break;
                    if (Time.time > countdownTime)
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        SpeedyCanon(playerPhotonView);
                    }
                    break;
                }
            case SpecConsts.cSpec_0401_Confusion:
                {
                    break;
                }
            case SpecConsts.cSpec_0402_WindWalk:
                {
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break;
                    if (Time.time > countdownTime && !isWindWalk)
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        WindWalker(playerPhotonView, true);
                        isWindWalk = true;
                        _playerPhotonView = playerPhotonView;
                    }
                    break;
                }
            case SpecConsts.cSpec_0501_Repair:
                {
                    curFireNum = 0;
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break; // cooldown time after skill end
                    if (Time.time > countdownTime) // defend repeat using from skill begining to skill ending
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        IncreasHealth(playerPhotonView);
                    }                    
                    break;
                }
            case SpecConsts.cSpec_0502_Confusion:
                {
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break;
                    if (Time.time > countdownTime)
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        SetAreaStopIndicator(playerPhotonView);
                        isIndicatorPossible = true;
                    }
                    break;
                }
            case SpecConsts.cSpec_0601_Help:
                {
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break; // cooldown time after skill end
                    if (Time.time > countdownTime) // defend repeat using from skill begining to skill ending
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        TeamHealthUp(playerPhotonView);
                    }
                    break;
                }
            case SpecConsts.cSpec_0602_Silence:
                {
                    if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) break;
                    if (Time.time > countdownTime)
                    {
                        countdownTime = Time.time + SpecConsts.cSpec_Delay_Time[curIdx];
                        SetAreaNoSkillIndicator(playerPhotonView);
                        isIndicatorPossible = true;
                    }
                    break;
                }
        }

        yield return null;
    }

    public IEnumerator FireSpec(PhotonView playerPhotonView)// Transform shotPos, short turretRotation)
    {
        switch (curIdx)
        {
            case SpecConsts.cSpec_0201_IceShell: // Ice Canon
                {
                    if (!IsSpecFireCountPossible()) break; // countdown number and cooldown limit
                    curFireNum += 1;
                    FireIceCanon(playerPhotonView);
                    break;
                }
            case SpecConsts.cSpec_0202_Rocket:
                {
                    if (isIndicatorPossible) // Indicator Fire avalable 
                    {
                        Debug.Log("FireSpec: curFireNum =" + curFireNum);
                        if (!IsSpecFireCountPossible()) break;
                        curFireNum += 1;
                        FireAreaMissile(playerPhotonView);
                    }
                    break;
                }            
            case SpecConsts.cSpec_0401_Confusion:
                {
                    if (!IsSpecFireCountPossible()) break;
                    curFireNum += 1;
                    FireElectric_Confusion(playerPhotonView);
                    break;
                }
            case SpecConsts.cSpec_0502_Confusion:
                {
                    if (isIndicatorPossible) // Indicator Fire avalable 
                    {
                        if (!IsSpecFireCountPossible()) break;
                        curFireNum += 1;
                        FireAreaStop(playerPhotonView);
                    }
                    break;
                }
            case SpecConsts.cSpec_0602_Silence:
                {
                    if (isIndicatorPossible) // Indicator Fire avalable 
                    {
                        if (!IsSpecFireCountPossible()) break;
                        curFireNum += 1;
                        FireAreaNoSkill(playerPhotonView);
                    }
                    break;
                }
        }
        yield return null;
    }


    private void TeamHealthUp(PhotonView playerPhotonView)
    {
        photonView.RPC("CmdTeamHealthUp", RpcTarget.AllViaServer, curIdx, playerPhotonView.ViewID );
    }

    [PunRPC]
    private void CmdTeamHealthUp(byte curIdx, int playerPhotonViewID)
    {
        GameObject playerObj = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;
        Player[] allTanks = GameObject.FindObjectsOfType<Player>();
        //GameObject[] skillObj = new GameObject[allTanks.Length];
        foreach (Player tank in allTanks)
        {
            //Debug.Log("TeamHealthUp: All tanks: tnak = " + tank.gameObject.name);
            //if (tank.gameObject == playerObj) continue;
            if ( playerObj.GetPhotonView().GetTeam() == tank.GetView().GetTeam())
            {
                GameObject skillObj;
                Debug.Log("SpecManager.CmdTeamHealthUp: curIdx >> " + curIdx);
                //Debug.Log("TeamHealthUp: team tank = " + tank.gameObject.name);
                skillObj = PoolManager.Spawn(mainSpecs[curIdx], tank.transform.position, tank.turret.rotation);// shotCenter, syncedRot);
                skillObj.GetComponent<Spec_06_TeamHealthUp>().player = tank.gameObject;
                skillObj.GetComponent<Spec_06_TeamHealthUp>().spec_Manager = this.gameObject;
                tank.GetView().SetHealth(tank.GetView().GetHealth() + 20);
            }
            
        }
    }

    private void SetAreaNoSkillIndicator(PhotonView playerPhotonView)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[curIdx], new Vector3(mouseWorldPos.x, 1f, mouseWorldPos.z), Quaternion.identity);
        skillMainObj.GetComponent<Spec_06_SilenceIndicator>().spec_Manager = this.gameObject;
        playerObject = playerPhotonView.GetComponent<Player>().gameObject; // for mouse indicator moveing limit
    }

    private void FireAreaNoSkill(PhotonView playerPhotonView)
    {
        short[] pos = new short[] { (short)(mouseWorldPos.x * 10), (short)(mouseWorldPos.z * 10) };
        photonView.RPC("Cmd06IndicatorFire", RpcTarget.AllViaServer, pos, playerPhotonView.ViewID );
        if (skillMainObj.GetComponent<Spec_06_SilenceIndicator>().gameObject)
            skillMainObj.GetComponent<Spec_06_SilenceIndicator>().gameObject.SetActive(false); ;
    }

    [PunRPC]
    protected void Cmd06IndicatorFire(short[] pos, int playerPhotonViewID)
    {
        Collider[] hitColliders = Physics.OverlapSphere(new Vector3(pos[0] / 10f, 0f, pos[1] / 10f), SpecConsts.areaRadius);
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log("ApplyAreaNoSkill: gameObject = " + hitCollider.gameObject.name);
            if (hitCollider.gameObject.GetComponent<Player>() 
                && hitCollider.gameObject.GetPhotonView().GetTeam() != PhotonNetwork.GetPhotonView (playerPhotonViewID).GetTeam())
            {
                skillSubObj = PoolManager.Spawn(subSpecs[3], hitCollider.gameObject.transform.position, transform.rotation);
                skillSubObj.GetComponent<Spec_06_SilenceEffet>().player = hitCollider.gameObject;
                hitCollider.gameObject.GetComponent<Player>().isPossibleSpec = false;
            }
        }
    }


    private void SetAreaStopIndicator(PhotonView playerPhotonView)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[curIdx], new Vector3(mouseWorldPos.x, 1f, mouseWorldPos.z), Quaternion.identity);
        skillMainObj.GetComponent<Spec_05_Indicator>().spec_Manager = this.gameObject;
        playerObject = playerPhotonView.GetComponent<Player>().gameObject; // for mouse indicator moveing limit
    }

    private void FireAreaStop(PhotonView playerPhotonView)
    {
        short[] pos = new short[] { (short)(mouseWorldPos.x * 10), (short)(mouseWorldPos.z * 10) };
        photonView.RPC("Cmd05IndicatorFire", RpcTarget.AllViaServer, pos, playerPhotonView.ViewID );
        if (skillMainObj.GetComponent<Spec_05_Indicator>().gameObject)
            skillMainObj.GetComponent<Spec_05_Indicator>().gameObject.SetActive(false);
    }

    [PunRPC]
    protected void Cmd05IndicatorFire(short[] pos, int playerPhotonViewID)
    {
        Collider[] hitColliders = Physics.OverlapSphere(new Vector3(pos[0] / 10f, 0f, pos[1] / 10f), SpecConsts.areaRadius);
        foreach (var hitCollider in hitColliders)
        {
            //Debug.Log("ApplyAreaStop: gameObject = " + hitCollider.gameObject.name);
            if (hitCollider.gameObject.GetComponent<Player>()
                && hitCollider.gameObject.GetPhotonView().GetTeam() != PhotonNetwork.GetPhotonView (playerPhotonViewID).GetTeam())
            {
                skillSubObj = PoolManager.Spawn(subSpecs[2], hitCollider.gameObject.transform.position, transform.rotation);
                skillSubObj.GetComponent<Spec_05_AreaStopEffect>().player = hitCollider.gameObject;
                hitCollider.gameObject.GetComponent<Player>().SpeedDown(0);
                hitCollider.gameObject.GetComponent<Player>().isFire = false;
            }
        }
    }
       

    protected void IncreasHealth(PhotonView playerPhotonView)
    {
        photonView.RPC("CmdIncreaseHealth", RpcTarget.AllViaServer, curIdx, playerPhotonView.ViewID );
    }

    [PunRPC]
    protected void CmdIncreaseHealth(byte specIdx, int playerPhotonViewID)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[specIdx], PhotonNetwork.GetPhotonView (playerPhotonViewID).GetComponent<Player>().shotPos.position, PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().turret.rotation);// shotCenter, syncedRot);
        playerObject = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;
        skillMainObj.GetComponent<Spec_05_IncreaseHealth>().player = playerObject;
        skillMainObj.GetComponent<Spec_05_IncreaseHealth>().spec_Manager = this.gameObject;
        PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().maxHealth = SpecConsts.maxHealth;
    }    

    protected void SpeedyCanon(PhotonView playerPhotonView)
    {
        photonView.RPC("CmdSpeedyCanon", RpcTarget.AllViaServer, curIdx, playerPhotonView.ViewID );
    }

    [PunRPC]
    protected void CmdSpeedyCanon(byte specIdx, int playerPhotonViewID)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[specIdx], PhotonNetwork.GetPhotonView (playerPhotonViewID).GetComponent<Player>().shotPos.position, PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().turret.rotation);// shotCenter, syncedRot);
        playerObject = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;
        skillMainObj.GetComponent<Spec_03_SpeedyFire>().player = playerObject;
        skillMainObj.GetComponent<Spec_03_SpeedyFire>().spec_Manager = this.gameObject;
        PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().fireRate /= SpecConsts.fireSpeedTimes;
    }


    protected void ReinforceCanon(PhotonView playerPhotonView)
    {
        photonView.RPC("CmdReinforceCanon", RpcTarget.AllViaServer, curIdx, playerPhotonView.ViewID );
    }

    [PunRPC]
    protected void CmdReinforceCanon(byte specIdx, int playerPhotonViewID)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[specIdx], PhotonNetwork.GetPhotonView (playerPhotonViewID).GetComponent<Player>().shotPos.position, PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().turret.rotation);// shotCenter, syncedRot);
        playerObject = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;
        skillMainObj.GetComponent<Spec_03_Reinforced>().player = playerObject;
        skillMainObj.GetComponent<Spec_03_Reinforced>().spec_Manager = this.gameObject;
        PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().isReinforcedBullet = true;
    }

    protected void EquipSpeedy(int photonViewID)
    {
        photonView.RPC("CmdEquipSpeedy", RpcTarget.AllViaServer, curIdx, photonViewID);
    }

    [PunRPC]
    protected void CmdEquipSpeedy(byte specIdx, int photonViewID) //, short[] position, short angle)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[specIdx], PhotonNetwork.GetPhotonView(photonViewID).GetComponent<Player>().shotPos.position, PhotonNetwork.GetPhotonView(photonViewID).GetComponent<Player>().turret.rotation);// shotCenter, syncedRot);
        playerObject = PhotonNetwork.GetPhotonView (photonViewID).GetComponent<Player>().gameObject;
        skillMainObj.GetComponent<Spec_01_Speedy>().player = playerObject;
        skillMainObj.GetComponent<Spec_01_Speedy>().spec_Manager = this.gameObject;
        PhotonNetwork.GetPhotonView (photonViewID).GetComponent<Player>().moveSpeed *= SpecConsts.speedTimes;
    }

    protected void EquipShield(PhotonView playerPhotonView)
    {
        photonView.RPC("CmdEquipShield", RpcTarget.AllViaServer, curIdx, playerPhotonView.ViewID );
    }

    [PunRPC]
    protected void CmdEquipShield(byte specIdx, int playerPhotonViewID) //, short[] position, short angle)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[specIdx], PhotonNetwork.GetPhotonView (playerPhotonViewID).GetComponent<Player>().shotPos.position, PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().turret.rotation);// shotCenter, syncedRot);
        playerObject = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;
        skillMainObj.GetComponent<Spec_01_Shield>().player = playerObject;
        skillMainObj.GetComponent<Spec_01_Shield>().spec_Manager = this.gameObject;
        PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().isSpecShield = true;
    }

    private void SetAreaMissileIndicator(PhotonView playerPhotonView)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[curIdx], new Vector3(mouseWorldPos.x, 1f, mouseWorldPos.z), Quaternion.identity);
        skillMainObj.GetComponent<Spec_02_Indicator>().spec_Manager = this.gameObject;
        playerObject = playerPhotonView.GetComponent<Player>().gameObject; // for mouse indicator moveing limit
    }

    private void FireAreaMissile(PhotonView playerPhotonView)
    {
        Debug.Log("FireAreMissle! curFireNum = " + curFireNum);

        short[] pos = new short[] { (short)(mouseWorldPos.x * 10), (short)(mouseWorldPos.z * 10) };
        photonView.RPC("CmdFireAreaMissile", RpcTarget.AllViaServer, pos, playerPhotonView.ViewID );
        if (skillMainObj.GetComponent<Spec_02_Indicator>().gameObject)
            skillMainObj.GetComponent<Spec_02_Indicator>().gameObject.SetActive(false);
    }

    [PunRPC]
    protected void CmdFireAreaMissile(short[] pos, int playerPhotonViewID)
    {
        Debug.Log("CmdFireAreaMissile! curFireNum = " + curFireNum);

        skillSubObj = PoolManager.Spawn(subSpecs[1], new Vector3(pos[0] / 10f, 0f, pos[1] / 10f), Quaternion.identity);
        //skillSubObj.GetComponent<Spec_02_AreaMissile>().player =  
        GameObject armo = skillSubObj.GetComponent<Spec_02_AreaMissile>().transform.GetChild(1).gameObject;
        armo.GetComponent<Spec_02_AreaMissile_HitCollison>().owner = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;

        /* 
         Collider[] hitColliders = Physics.OverlapSphere(new Vector3(pos[0] / 10f, 0f, pos[1] / 10f), SpecConsts.areaRadius);
         foreach (var hitCollider in hitColliders)
         {
             //Debug.Log("CmdFireAreaMissile: gameObject = " + hitCollider.gameObject.name);
             if (hitCollider.gameObject.GetComponent<Player>() && hitCollider.gameObject.GetPhotonView().GetTeam() != PhotonNetwork.GetPhotonView(playerPhotonViewID).GetTeam())
             {
                 skillSubObj = PoolManager.Spawn(subSpecs[5], hitCollider.gameObject.transform.position, transform.rotation);
                 skillSubObj.GetComponent<Spec_02_AreaMissileExploison>().player = hitCollider.gameObject;
                 //if (hitCollider.gameObject.GetComponent<PlayerBot>())
                 {
                     //Debug.Log("ApplyAreaMissile: this Bot is =" + hitCollider.gameObject);
                     hitCollider.gameObject.GetComponent<Player>().TakeDamage(SpecConsts.missileDamage, 0);
                 }
                 //else
                 {                  
                     //if (GameManager.GetInstance().localPlayer.gameObject ==  hitCollider.gameObject) //.GetComponent<PhotonView>().IsMine)
                     {
                         //Debug.Log("ApplyAreaMissile: this Player is =" + hitCollider.gameObject);
                         //hitCollider.gameObject.GetComponent<PhotonView>().RPC("CmdCallAreaMissileTakeDamage", RpcTarget.AllViaServer);
                     }
                 }
             }
         }*/
    }

   
    protected void FireElectric_Confusion(PhotonView playerPhotonView)
    {
        photonView.RPC("CmdElectricCanonConfusion", RpcTarget.AllViaServer, curIdx, playerPhotonView.ViewID );
    }

    [PunRPC]
    protected void CmdElectricCanonConfusion(byte specIdx, int playerPhotonViewID)
    {
        skillMainObj = PoolManager.Spawn(mainSpecs[specIdx], PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().shotPos.position, PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().turret.rotation);// shotCenter, syncedRot);
        playerObject = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;
        skillMainObj.GetComponent<Spec_04_ElectricConfusion>().owner = playerObject;
    }

    private void FireIceCanon(PhotonView playerPhotonView)
    {
        this.photonView.RPC("CmdFireIceCanon", RpcTarget.AllViaServer, curIdx, playerPhotonView.ViewID );//, pos, turretRotation);
    }

    [PunRPC]
    protected void CmdFireIceCanon(byte specIdx, int playerPhotonViewID) //, short[] position, short angle)
    {
        Vector3 pos = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().shotPos.position + PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().turret.transform.forward * 1f; 
        skillMainObj = PoolManager.Spawn(mainSpecs[specIdx], pos, PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().turret.rotation);// shotCenter, syncedRot);
        playerObject = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;
        skillMainObj.GetComponent<Spec_02_IceCanon>().owner = playerObject;
    }

    public void WindWalker(PhotonView playerPhotonView, bool isWindWalker)
    {
        photonView.RPC("CmdWindWalker", RpcTarget.AllViaServer, playerPhotonView.ViewID, isWindWalker);        
    }

    [PunRPC]
    protected void CmdWindWalker(int playerPhotonViewID, bool isWindWalker)
    {
        GameObject tankObj = PhotonNetwork.GetPhotonView(playerPhotonViewID).GetComponent<Player>().gameObject;
        if (isWindWalker)
        {
            if (GameManager.GetInstance().localPlayer.gameObject != tankObj) //if (photonView.IsMine)
            {
                SetWindWalker(tankObj, true, false);
            }
            else
            {
                SetWindWalker(tankObj, true, true);
            }
        }
        else
        {
            SetWindWalker(tankObj, false);
        }

    }

   
    private void SetWindWalker(GameObject obj, bool isWindWalker, bool isMine = false)
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
                    if (isWindWalker)
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
        EnableShadow( obj, isWindWalker);

        foreach (Renderer objectRenderer in obj.GetComponentsInChildren<MeshRenderer>())
        {
            if (isWindWalker)
            {
                originalMat = objectRenderer.material;// child.GetComponent<Material>();
                if (isMine)
                    objectRenderer.material = Resources.Load("Force Field", typeof(Material)) as Material;
                else
                    objectRenderer.material = Resources.Load("Force Field_00", typeof(Material)) as Material;
                //Debug.Log("SetWinWalk: frenselPower = " + frenselPower + ", objectRenderer" + objectRenderer.material);
            }
            else
            {
                objectRenderer.material = originalMat;
            }
        }

    }

    private void EnableShadow(GameObject obj, bool isWindWalker)
    {
        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;
            try
            {
                //Debug.Log("EnableShadow: Obj = " + child.gameObject);
                if (child.gameObject.GetComponent<MeshRenderer>())
                {
                    if (isWindWalker)
                        child.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    else
                        child.gameObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                EnableShadow(child.gameObject, isWindWalker);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    private bool IsSpecFireCountPossible ( )
    {
        if (curFireNum >= SpecConsts.cSpec_Count_Num[curIdx])
        {
            Debug.Log("curFireNum =" + curFireNum );
            isIndicatorPossible = false;
            cooldownStart = Time.time;
            curFireNum = 0;
            curIdx = SpecConsts.cSpec_noSpec;
            triggerIndicator = true;

            PoolManager.Despawn(skillMainObj, 0);

            sldValualbe.GetComponent<Slider>().value = 1f;
            sldValualbe.GetComponent<Slider>().value = 0f;
            return false;
        }

        //if (Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx]) return false;

        return true;
    }

    private void FixedUpdate()
    {
        if (curIdx == SpecConsts.cSpec_noSpec) return;

        if (cooldownStart < Time.time && Time.time < cooldownStart + SpecConsts.cSpec_CoolDownTime[curIdx])
        {
            //sldCooldown.GetComponent<Slider>().value = (Time.time - cooldownStart) / SpecConsts.cSpec_CoolDownTime[curIdx];
            //sldValualbe.GetComponent<Slider>().value = 0;
            return;
        }
        
        if (curFireNum > 0)
        {
            //sldValualbe.GetComponent<Slider>().value = (float)(curFireNum / SpecConsts.cSpec_Count_Num[curIdx]);
            //sldCooldown.GetComponent<Slider>().value = 0;
            return;
        }
        else if (countdownTime  - Time.time > 0)
        {
            //sldValualbe.GetComponent<Slider>().value = (float)(countdownTime / SpecConsts.cSpec_Count_Num[curIdx]);
            //sldCooldown.GetComponent<Slider>().value = 0;
        }      

    }
}


public static class SpecConsts
{
    public const byte cSpec_0101_FastSpeedy = 0;
    public const byte cSpec_0102_Shield = 1;
    public const byte cSpec_0201_IceShell = 2;
    public const byte cSpec_0202_Rocket = 3;
    public const byte cSpec_0301_2xShell = 4;
    public const byte cSpec_0302_AttactkSpeed = 5;
    public const byte cSpec_0401_Confusion = 6;
    public const byte cSpec_0402_WindWalk = 7;
    public const byte cSpec_0501_Repair = 8;
    public const byte cSpec_0502_Confusion = 9;
    public const byte cSpec_0601_Help = 10;
    public const byte cSpec_0602_Silence = 11;

    //public static byte[] cSpec_Delay_Time_Num = new byte[] { 4, 5, 5, 1, 5, 5, 3, 5, 1, 2, 1, 5 };
    //public static byte[] cSpec_CoolDownTime = new byte[] { 15, 25, 15, 10, 15, 15, 15, 15, 20, 15, 15, 20 };

    public static byte[] cSpec_Delay_Time = new byte[] { 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };
    public static byte[] cSpec_Count_Num = new byte[] { 0, 0, 5, 1, 5, 0, 5, 0, 0, 1, 0, 1 };
    public static byte[] cSpec_CoolDownTime = new byte[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

    public static byte cSpec_noSpec = 100;

    public const byte bulletType_Bullet = 0;
    public const byte bulletType_Spec = 1;

    public const byte fireTank_ID = 0;
    public const byte electricTank_ID = 5;

    public const byte speedTimes = 3;
    public const byte fireSpeedTimes = 5;
    public const float areaRadius = 3f;
    public const int missileDamage = 30;
    public const int maxHealth = 120;
}