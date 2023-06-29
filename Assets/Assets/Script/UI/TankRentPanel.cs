using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankRentPanel : MonoBehaviour
{
    public static TankRentPanel instance;
    public Transform tankListContnet;
    public List<TankItem> tankItems = new List<TankItem>();
    private List<string> sortType = new List<string> { "level", "pool" };
    private int sortIndex = 0;
    private string selectedTankId;
    public void Awake()
    {
        instance = this;
    }

    public void Reset()
    {
        int count = tankItems.Count;
        for (int i = 0; i < count; i++)
        {

            tankItems[i].Delete();
            //items.RemoveAt(i);
        }
        tankItems.Clear();
    }

    private void OnEnable()
    {
        selectedTankId = null;
        GetRentableTanks();
    }

    private void OnDisable()
    {

    }

    public void SetSortIndex(int index)
    {
        sortIndex = index;
        GetRentableTanks();
    }

    private void GetRentableTanks()
    {
        //TODO add sort index
        SocketGameManager.instance.GetRentableTanks(sortType[sortIndex]);
        UIManager.instance.ShowLoading(true);
    }

    public void RentTank()
    {
        UIManager.instance.ShowLoading(true);
        if (selectedTankId != null)
        {
            SocketGameManager.instance.RentRank(selectedTankId);
        }
    }

    public void GetUserTank()
    {
        SocketGameManager.instance.GetUserData();
    }

    public void SetRentTank(string id)
    {
        selectedTankId = id;
        foreach (var item in tankItems)
        {
            item.ShowSelectedUI(false);
        }
    }

    public void RentTankResult(bool result)
    {
        if (result)
        {
            UIManager.instance.Error("Successed Rent");
            GetRentableTanks();
        }
        else
        {
            UIManager.instance.Error("Failed Rent");
        }
        print(string.Format("rantTank-result{0}", result));
    }

    public void SetTankList(NftTanks nftTanks)
    {
        if (nftTanks.tanks.Length > 0)
        {
            if (selectedTankId == null)
            {
                selectedTankId = nftTanks.tanks[0].id;
            }
        }
        else
        {
            selectedTankId = null;
        }
        UIManager.instance.ShowLoading(false);
        Reset();
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
        tankListItem.GetComponent<TankItem>().SetData(id, tankClass.image, tank, TankItemType.rentTankItem);
        tankListItem.transform.SetParent(tankListContnet);
        tankItems.Add(tankListItem.GetComponent<TankItem>());
    }

}
