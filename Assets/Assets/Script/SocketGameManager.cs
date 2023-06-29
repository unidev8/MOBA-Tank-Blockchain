using Firesplash.UnityAssets.SocketIO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Photon.Pun;
using TanksMP;
using UnityEngine.SceneManagement;

#if HAS_JSON_NET
//If Json.Net is installed, this is required for Example 6. See documentation for informaiton on how to install Json.NET

using JSON.NET;
#endif


public class SocketGameManager : MonoBehaviour
{
    public SocketIOCommunicator sioCom;
    public static SocketGameManager instance;

    void Start()
    {
        sioCom
            .Instance
            .On(Constants.GetSecurityCode("error"),
            (string json) =>
            {
                string data = Constants.GetDecodeData(json);
                ErrorType error = new ErrorType();
                JsonUtility.FromJsonOverwrite(data, error);
                UIManager.instance.Error(error.error);
                Debug.Log(error.error);
            });
        sioCom
            .Instance
            .On(Constants.GetSecurityCode("user-tanks"),
            (string json) =>
            {
                UIManager.instance.ShowLoading(false);
                UIManager.instance.AddDebug("receive user-tanks, ");
                string data = Constants.GetDecodeData(json);
                NftTanks nftTanks = JsonConvert.DeserializeObject<NftTanks>(data);
                PlayerManager.instance.userTanks = nftTanks;
                if (MainPanel.instance != null)
                    MainPanel.instance.SetTankList(nftTanks);
            });
        sioCom
            .Instance
            .On(Constants.GetSecurityCode("update-tank"),
            (string json) =>
            {
                string data = Constants.GetDecodeData(json);
                NftTank nftTank = JsonConvert.DeserializeObject<NftTank>(data);
                if (TanksMP.GameManager.GetInstance() != null)
                {
                    TanksMP.GameManager.GetInstance().CmdRPCUpdateTank(nftTank);
                }
                //Debug.Log("SocketGameManager.Event(update-tank) " + JsonUtility.ToJson(nftTank));
            });
        //TODO
        sioCom
          .Instance
          .On(Constants.GetSecurityCode("rentable-tanks"),
          (string json) =>
          {
              UIManager.instance.ShowLoading(false);
              UIManager.instance.AddDebug("received rentable-tanks, ");
              string data = Constants.GetDecodeData(json);
              NftTanks nftTanks = JsonConvert.DeserializeObject<NftTanks>(data);
              PlayerManager.instance.userTanks = nftTanks;
              if (TankRentPanel.instance != null)
                  TankRentPanel.instance.SetTankList(nftTanks);
          });
        sioCom
          .Instance
          .On(Constants.GetSecurityCode("rentTank-result"),
          (string json) =>
          {
              UIManager.instance.ShowLoading(false);
              string data = Constants.GetDecodeData(json);
              bool result = JsonConvert.DeserializeObject<bool>(data);
              UIManager.instance.AddDebug("received rentTank-result event >>" + result);
              if (TankRentPanel.instance != null)
                  TankRentPanel.instance.RentTankResult(result);
          });
        sioCom
          .Instance
          .On(Constants.GetSecurityCode("[SocketGameManager.Event]rentTank-result"),
          (string json) =>
          {
              string data = Constants.GetDecodeData(json);
              bool result = JsonConvert.DeserializeObject<bool>(data);
              if (TankRentPanel.instance != null)
                  TankRentPanel.instance.RentTankResult(result);
              print("[SocketGameManager.Event]rentTank-result");
          });
        sioCom
          .Instance
          .On(Constants.GetSecurityCode("leaderBoard"),
          (string json) =>
          {
              string data = Constants.GetDecodeData(json);
              List<LeaderBoardPlayerPayload> result = JsonConvert.DeserializeObject<List<LeaderBoardPlayerPayload>>(data);
              if (LeaderBoardPanel.instance != null)
                  LeaderBoardPanel.instance.SetItemList(result);
              print("[SocketGameManager.Event]leaderBoard");
          });
        sioCom
            .Instance
            .On(Constants.GetSecurityCode("kicked"),
            (string json) =>
            {
                string data = Constants.GetDecodeData(json);
                KickInfo kickInfo = JsonConvert.DeserializeObject<KickInfo>(data);
                if (TanksMP.GameManager.GetInstance() != null)
                {
                    Kick(kickInfo.ownerNickName);
                }
            });
        sioCom
           .Instance
           .On(Constants.GetSecurityCode("update-tank-energy"),
           (string json) =>
           {
               string data = Constants.GetDecodeData(json);
               GetEnergy energy = JsonConvert.DeserializeObject<GetEnergy>(data);
               if (TanksMP.GameManager.GetInstance() != null)
               {
                   TanksMP.GameManager.GetInstance().UpdateTankEnergy(energy.energy);
               }
           });
        sioCom
           .Instance
           .On(Constants.GetSecurityCode("global-chat"),
           (string json) =>
           {
               string data = Constants.GetDecodeData(json);
               GlobalChatPayload payload = JsonConvert.DeserializeObject<GlobalChatPayload>(data);
               if (ChatManager.instance != null)
               {
                   ChatManager.instance.ReceiveGlobalChat(payload);
               }
           });
        // defend cheat
        sioCom
           .Instance
           .On(Constants.GetSecurityCode("spawn-tank"),
           (string json) =>
           {
               if (GameManager.GetInstance() != null)
               {
                   GameManager.GetInstance().StartSendGameStateToBackend();
               }
           });

        sioCom
           .Instance
           .On(Constants.GetSecurityCode("royalRoom-info"),
           (string json) =>
           {
               if (SceneManager.GetActiveScene().buildIndex == 0)
               {
                   string data = Constants.GetDecodeData(json);
                   Debug.Log("SocketGameManager.royalRoom-info >> " + data);
                   FullRoyalRoomInfo payload = JsonConvert.DeserializeObject<FullRoyalRoomInfo>(data);
                   if (MainPanel.instance)
                   {
                       MainPanel.instance.UpdateRoyalRoomInfo(payload);
                   }
               }
           });

    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void GetUserData()
    {
        sioCom.Instance.Emit(Constants.GetSecurityCode("auth-data"));
    }

    public void SendGlobalChat(string chatText)
    {
        sioCom.Instance.Emit(Constants.GetSecurityCode("global-chat"), Constants.GetEncodedSocketData(JsonUtility.ToJson(new StringPayLoad(chatText))), false);
    }

    public void SendGameState(GameStatePayload payload)
    {
        sioCom.Instance.Emit(Constants.GetSecurityCode("game-state"), Constants.GetEncodedSocketData(JsonConvert.SerializeObject(payload)), false);
    }
    public void GetTanks()
    {
        UIManager.instance.AddDebug("UserTanksREQ1, ");
        GetTankReqData getTankReq = new GetTankReqData();
        getTankReq.socketId = sioCom.Instance.SocketID;
        sioCom.Instance.Emit(Constants.GetSecurityCode("getUsertanks"), Constants.GetEncodedSocketData(JsonUtility.ToJson(getTankReq)), false);
        UIManager.instance.AddDebug("getTankInfoREQ, ");
    }

    public void GetLeaderBoardInfo()
    {
        sioCom.Instance.Emit(Constants.GetSecurityCode("getLeaderBoard"));
    }

    public void AddExperience(string SocketID, string nft_id, int level)
    {
        //PhotonNetwork.Disconnect();
        AddExpReqData addExpData = new AddExpReqData();
        addExpData.nft_id = nft_id;
        addExpData.socketID = SocketID;
        addExpData.level = level;
        sioCom.Instance.Emit(Constants.GetSecurityCode("addExperience"), Constants.GetEncodedSocketData(JsonUtility.ToJson(addExpData)), false);
        //Debug.Log("SocketGameManager.AddExperience: sendData => " + JsonUtility.ToJson(addExpData));
    }

    public void GetRentableTanks(string sortType)
    {
        sioCom.Instance.Emit(Constants.GetSecurityCode("getRentableTanks"), Constants.GetEncodedSocketData(JsonUtility.ToJson(new StringPayLoad(sortType))), false);
    }

    public void RentRank(string id)
    {
        sioCom.Instance.Emit(Constants.GetSecurityCode("rentTank"), Constants.GetEncodedSocketData(JsonUtility.ToJson(new StringPayLoad(id))), false);
    }

    public void GetEnergy(string SocketID, string nft_id, int level)
    {
        AddExpReqData addExpData = new AddExpReqData();
        addExpData.nft_id = nft_id;
        addExpData.socketID = SocketID;
        addExpData.level = level;
        sioCom.Instance.Emit(Constants.GetSecurityCode("getEnegy"), Constants.GetEncodedSocketData(JsonUtility.ToJson(addExpData)), false);
    }
    public void SpawnTank(int photonViewID, string nft_id, NetworkMode gameMode)
    {
        return;
        SpawnTankData payload = new SpawnTankData(photonViewID, nft_id, (int)gameMode);
        sioCom.Instance.Emit(Constants.GetSecurityCode("spawn-tank"), Constants.GetEncodedSocketData(JsonUtility.ToJson(payload)), false);
    }

    Photon.Realtime.Player killedPlayer;
    private void Kick(string nickname)
    {
        if (string.IsNullOrEmpty(nickname))
        {
            return; // log error?
        }

        foreach (KeyValuePair<int, Photon.Realtime.Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            if (nickname.Equals(player.Value.NickName))
            {
                killedPlayer = player.Value;
                break;
            }
        }
        if (killedPlayer == null) return;

        if (!killedPlayer.IsMasterClient)
        {
            Kick(killedPlayer);
        }
        else if (killedPlayer.IsMasterClient)
        {
            PhotonNetwork.Disconnect();
            UIManager.instance.Error("No enough tank energy!");
        }
        // log error? player is local or not found?
    }
    private void Kick(Photon.Realtime.Player playerToKick)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return; // log error?
        }
        PhotonNetwork.CloseConnection(playerToKick);
    }

    public void CreateRoyalRoom()
    {
        RoyalRoomInfoPayLod payload = DedicatedServerManager.instance.GetRoyalRoomInfo();
        sioCom.Instance.Emit(Constants.GetSecurityCode("create-royal-room"), Constants.GetEncodedSocketData(JsonUtility.ToJson(payload)), false);
    }

    public void SetRoyalRoomState(RoyalRoomStatePayload state)
    {
        sioCom.Instance.Emit(Constants.GetSecurityCode("set-royalRoom-state"), Constants.GetEncodedSocketData(JsonUtility.ToJson(state)), false);
    }

    public void SendRoyalGameResult()
    {
        sioCom.Instance.Emit(Constants.GetSecurityCode("royalGame_result"));
    }
}


