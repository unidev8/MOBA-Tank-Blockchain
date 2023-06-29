using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TanksMP;
using Shapes2D;
using Firesplash.UnityAssets.SocketIO;

public class MainPanel : MonoBehaviour
{
    public static MainPanel instance;
    public GameObject loadingWindow;
    public GameObject connectionErrorWindow;
    #region ItemDescription
    public Text titleTxtUI;
    public Text speedTxtUI;
    public Text fireRateTxtUI;
    public Text firePowerTxtUI;
    public Text ammoTxtUI;
    public Shape itemTexture;

    #endregion
    #region NftTankItmeList
    public Transform tankListContnet;
    public List<TankItem> tankItems = new List<TankItem>();
    #endregion

    #region Panels
    public GameObject mainPanel;
    public GameObject mapPanel;
    public Text userNameUI;
    public Shapes2D.Shape userAvataImg;
    public Text meritUI;
    #endregion

    #region royal info
    public UIRoyalRoomInfo royalRoomInfoPanel;
    #endregion
    public void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager.instance.userInfo.name != "")
        {
            SocketGameManager.instance.GetUserData();
        }
        if (!PlayerPrefs.HasKey(PrefsKeys.playerName)) PlayerPrefs.SetString(PrefsKeys.playerName, "User" + System.String.Format("{0:0000}", Random.Range(1, 9999)));
        if (!PlayerPrefs.HasKey(PrefsKeys.networkMode)) PlayerPrefs.SetInt(PrefsKeys.networkMode, 0);
        if (!PlayerPrefs.HasKey(PrefsKeys.gameMode)) PlayerPrefs.SetInt(PrefsKeys.gameMode, 0);
        if (!PlayerPrefs.HasKey(PrefsKeys.serverAddress)) PlayerPrefs.SetString(PrefsKeys.serverAddress, "127.0.0.1");
        if (!PlayerPrefs.HasKey(PrefsKeys.playMusic)) PlayerPrefs.SetString(PrefsKeys.playMusic, "true");
        if (!PlayerPrefs.HasKey(PrefsKeys.appVolume)) PlayerPrefs.SetFloat(PrefsKeys.appVolume, 1f);
        if (!PlayerPrefs.HasKey(PrefsKeys.activeTank)) PlayerPrefs.SetString(PrefsKeys.activeTank, Encryptor.Encrypt("0"));

        PlayerPrefs.Save();

        //read the selections and set them in the corresponding UI elements
        //listen to network connection and IAP billing errors
        NetworkManagerCustom.connectionFailedEvent += OnConnectionError;
    }

    void OnConnectionError()
    {
        if(UIManager.instance != null)
        {
            UIManager.instance.ShowLoading(false);
        }
        //game shut down completely
        if (this == null)
            return;

        StopAllCoroutines();
        loadingWindow.SetActive(false);
        //connectionErrorWindow.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateRoyalRoomInfo(FullRoyalRoomInfo info)
    {
        royalRoomInfoPanel.UpdateUI(info);
    }

    public void OnGameModeChanged(int value)
    {
        if (PlayerManager.instance.userTanks == null)
        {
            return;
        }

        if (PlayerManager.instance.userTanks.tanks.Length == 0)
        {
            UIManager.instance.Error("Please Create or Lend NFT Tank to play game");
            return;
        }
        PlayerPrefs.SetInt(PrefsKeys.networkMode, (int)NetworkMode.Online);
        PlayerPrefs.SetInt(PrefsKeys.gameMode, value);
        PlayerPrefs.Save();
        if (PlayerManager.instance.selectedTank != null)
        {
            NftTank tank = PlayerManager.instance.selectedTank;
            if (tank.energy > tank.health)
            {
                //Play();
                mainPanel.SetActive(false);
                mapPanel.SetActive(true);
            }
            else
            {
                UIManager.instance.Error("No enough tank energy!");
            }

        }

    }

    public void OnMapSelected(string map)
    {
        PlayerPrefs.SetString(PrefsKeys.map, map);
        PlayerPrefs.Save();
        Play();
    }

    public void OnTraining()
    {
        if (PlayerManager.instance.userTanks == null)
        {
            return;
        }
        if (PlayerManager.instance.userTanks.tanks.Length == 0)
        {
            UIManager.instance.Error("Please Create or Lend NFT Tank to play game");
            return;
        }
        PlayerPrefs.SetInt(PrefsKeys.networkMode, (int)NetworkMode.Offline);
        PlayerPrefs.SetInt(PrefsKeys.gameMode, (int)GameMode.TDM);
        PlayerPrefs.Save();
        if (PlayerManager.instance.selectedTank.energy < PlayerManager.instance.selectedTank.health)
        {
            UIManager.instance.Error("No enough tank energy!");
            return;
        }
        mainPanel.SetActive(false);
        mapPanel.SetActive(true);
        //Play();
    }

    public void Play()
    {
        if (!DedicatedServerManager.instance.IsServer())
        {
            if (PlayerManager.instance.userTanks.tanks == null)
            {
                UIManager.instance.Error("Please Create or Lend NFT Tank to play game");
                return;
            }

            if (PlayerManager.instance.userTanks.tanks.Length == 0)
            {
                UIManager.instance.Error("Please Create or Lend NFT Tank to play game");
                return;
            }
            //loadingWindow.SetActive(true);
            UIManager.instance.ShowLoading(true);
        }


        NetworkManagerCustom.StartMatch((NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode));
        StartCoroutine(HandleTimeout());
    }

    IEnumerator HandleTimeout()
    {
        yield return new WaitForSeconds(10);
        //timeout has passed, we would like to stop joining a game now
        Photon.Pun.PhotonNetwork.Disconnect();
        //display connection issue window
        OnConnectionError();
    }

    public void SetItemDescription(int classType, string title, string speed, string fireRate, string firePower, string ammo, Texture2D texture)
    {
        int count = tankItems.Count;
        for (int i = 0; i < count; i++)
        {
            tankItems[i].ShowSelectedUI(false);
            //items.RemoveAt(i);
        }
        PlayerPrefs.SetString(PrefsKeys.activeTank, Encryptor.Encrypt((classType).ToString()));
        titleTxtUI.text = title;
        speedTxtUI.text = "Speed:  " + "<color=#CF5D20>" + speed + "</color>";
        fireRateTxtUI.text = "FireRate:  " + "<color=#CF5D20>" + fireRate + "</color>";
        ammoTxtUI.text = "Ammo:  " + "<color=#CF5D20>" + ammo + "</color>";
        firePowerTxtUI.text = "FirePower:  " + "<color=#CF5D20>" + firePower + "</color>";
        itemTexture.settings.fillTexture = texture;
    }

    IEnumerator TankItemListReset()
    {
        int count = tankItems.Count;
        for (int i = 0; i < count; i++)
        {

            tankItems[i].Delete();
            //items.RemoveAt(i);
        }
        tankItems.Clear();
        yield return new WaitForSeconds(0.1f);
    }

    public void SetTankList(NftTanks nftTanks)
    {
        StartCoroutine(TankItemListReset());
        int num = nftTanks.tanks.Length;
        for (int i = 0; i < num; i++)
        {
            AddTankItem(i, nftTanks.tanks[i]);
        }
    }

    public void AddTankItem(int id, NftTank tank)
    {
        GameObject tankListItem = GameObject.Instantiate(Resources.Load("items/TankItem"), tankListContnet) as GameObject;
        TankClass tankClass = PlayerManager.instance.tankCalssList[int.Parse(tank.classType)];
        tankListItem.GetComponent<TankItem>().SetData(id, tankClass.image, tank, TankItemType.vipTankItem);
        tankListItem.transform.SetParent(tankListContnet);
        tankItems.Add(tankListItem.GetComponent<TankItem>());
    }

}
