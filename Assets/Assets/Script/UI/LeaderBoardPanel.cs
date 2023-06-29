using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoardPanel : MonoBehaviour
{
    public static LeaderBoardPanel instance;
    public Transform itemListContent;
    public List<LeaderBoardItem> items = new List<LeaderBoardItem>();

    private void Awake() {
        instance = this;
    }

    public void Reset(){
        foreach (var item in items)
        {
            item.Delete();
        }
        items.Clear();
    }

    private void OnEnable() {
        GetLeaderBoardInfo();
    }

    private void GetLeaderBoardInfo(){
        UIManager.instance.ShowLoading(true);
        SocketGameManager.instance.GetLeaderBoardInfo();
    }

    public void SetItemList(List<LeaderBoardPlayerPayload> players){
        Reset();
        for (int i = 0; i < players.Count; i++)
        {
            AddItem(i, players[i]);
        }
    }

    private void AddItem(int randIndex, LeaderBoardPlayerPayload player){
        UIManager.instance.ShowLoading(false);
        GameObject obj = GameObject.Instantiate(Resources.Load("items/LeaderBoardItem"), itemListContent) as GameObject;
        obj.GetComponent<LeaderBoardItem>().SetData(randIndex, player);
        obj.transform.SetParent(itemListContent);
        items.Add(obj.GetComponent<LeaderBoardItem>());
    }
}
