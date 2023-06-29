using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Shapes2D;
public enum TankItemType
  {
    rentTankItem,
    vipTankItem
  }
public class TankItem : MonoBehaviour
{
  // Start is called before the first frame update
  public NftTank tankInfo;
  ButtonGroup buttonGroup;
  public string tankName;
  public Text expTxtUI;
  public Text energyTxtUI;
  public Slider expSlider;
  public Slider energySlider;
  public Text levelTxtUI;
  public Shape image;
  public bool isActive;
  public int id;
  public GameObject selectedUI;
  public TankItemType itemType;
  
  void Start()
  {

    ShowSelectedUI(false);
    if (id == 0)
    {
      isActive = true;
      SetItemDescription();
    }
    // image.settings.fillTexture = property.texture;
    // expTxtUI.text = property.exp.ToString();
    // selectedUI.SetActive(isActive); 
    // if(isActive)
    //     SetItemDescription();
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void SetData(int _id, Texture2D tankImage, NftTank _tankInfo, TankItemType _itemType)
  {
    id = _id;
    tankName = _tankInfo.name;
    image.settings.fillTexture = tankImage;
    int[] expValues = Helper.GetExpValues(_tankInfo.tankLevel, _tankInfo.experience);
    expSlider.value = (float)expValues[0] / (float)expValues[1];
    expTxtUI.text = expValues[0] + "/" + expValues[1];
    //Pool silder
    energySlider.value = (float)_tankInfo.energy / (float)_tankInfo.maxEnergy;
    energyTxtUI.text = _tankInfo.energy + "/" + _tankInfo.maxEnergy;
    levelTxtUI.text = _tankInfo.tankLevel.ToString();
    tankInfo = _tankInfo;
    itemType = _itemType;
  }
  public void OnSelected()
  {
    isActive = true;
    SetItemDescription();
  }

  void SetItemDescription()
  {
    PlayerManager.instance.selectedTank = tankInfo;
    if(itemType == TankItemType.rentTankItem){
        TankRentPanel.instance.SetRentTank(tankInfo.id);
    }else if(itemType == TankItemType.vipTankItem){
        MainPanel.instance.SetItemDescription(int.Parse(tankInfo.classType), tankName, tankInfo.speed.ToString(), tankInfo.fireRate.ToString(), tankInfo.firePower.ToString(), tankInfo.health.ToString(), image.settings.fillTexture);
    }
    ShowSelectedUI(true);
  }

  public void Delete()
  {
    Destroy(gameObject);
  }

  public void ShowSelectedUI(bool show)
  {
    selectedUI.SetActive(show);
  }
}
