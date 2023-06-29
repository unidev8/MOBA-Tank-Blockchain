using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TanksMP;

public class PlayerManager : MonoBehaviour
{
  public static PlayerManager instance;
  public string SocketId;
  public UserInfo userInfo;
  public NftTanks userTanks = new NftTanks();
  public NftTank selectedTank = new NftTank();

  public TankClass[] tankCalssList;
  // Start is called before the first frame update
  void Start()
  { 

  }
  private void Awake()
  {
    if (instance == null)
    {
      instance = this;
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void SetUserInfo(string name, string email, string address, string avata_url, int merit)
  {
    userInfo = new UserInfo();
    userInfo.name = name;
    userInfo.email = email;
    userInfo.address = address;
    userInfo.avata_url = avata_url;
    userInfo.merit = merit;
    PlayerPrefs.SetString(PrefsKeys.playerName, name);
    if (MainPanel.instance != null)
    {
      MainPanel.instance.userNameUI.text = name;
      MainPanel.instance.meritUI.text = merit.ToString();
      StartCoroutine(loadSpriteImageFromUrl(MainPanel.instance.userAvataImg, avata_url));
    }
  }

    public int GetSelectedTankType()
    {
        return int.Parse(selectedTank.classType);
    }

    IEnumerator loadSpriteImageFromUrl(Shapes2D.Shape target, string URL)
  {

    WWW www = new WWW(URL);
    while (!www.isDone)
    {
      // Debug.Log("Download image on progress" + www.progress);
      yield return null;
    }

    if (!string.IsNullOrEmpty(www.error))
    {
      Debug.Log("Download failed");
    }
    else
    {
      Debug.Log("Download succes");
      Texture2D texture = new Texture2D(1, 1);
      www.LoadImageIntoTexture(texture);

      target.settings.fillTexture = texture;
    }
  }
  // public void SetUserNftTanks(NftTanks nftTanks)
  // {
  //   userTanks = nftTanks;
  //   MainPanel.instance.SetTankList(nftTanks);
  // }
}
