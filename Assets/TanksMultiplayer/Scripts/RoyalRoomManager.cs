using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

namespace TanksMP
{
    public class RoyalRoomManager : MonoBehaviourPunCallbacks
    {
        public static RoyalRoomManager instance;
        public float startAreaPosX = -156.5f;
        public float finalAreaPosX = -20.6f;
        public float areaDecreaseInterval = 3f;
        public int areaDecreaseTime = 50;
        public int areaDamage = 10;
        private float lerpTime = 0f;

        public Transform royalArea;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        private void Start()
        {
            SetAreaPos(startAreaPosX);

            if (PhotonNetwork.IsMasterClient)
                StartCoroutine(RoyalTimeMaker());
        }


        private void Update()
        {

        }
        //TODO this should be form backend
        IEnumerator RoyalTimeMaker()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                PhotonNetwork.CurrentRoom.SetRoyalTime(PhotonNetwork.CurrentRoom.GetRoyalTime() + 1);
                if (!PhotonNetwork.CurrentRoom.GetGameState() && PhotonNetwork.CurrentRoom.GetRoyalTime() > 0)
                {
                    if(PhotonNetwork.CurrentRoom.PlayerCount - 1 < DedicatedServerManager.instance.minPlayerCount)
                    {
                        PhotonNetwork.CurrentRoom.SetRoyalTime(-20);
                    }
                    else
                    {
                        PhotonNetwork.CurrentRoom.SetGameState(true);
                        PhotonNetwork.CurrentRoom.MaxPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
                    }
                }
                //Debug.Log("RoyalRoomManager.RoyalTimeMaker: max players " + PhotonNetwork.CurrentRoom.MaxPlayers);
                ApplyAreaEffect();
                SocketGameManager.instance.SetRoyalRoomState(new RoyalRoomStatePayload(PhotonNetwork.CurrentRoom.GetRoyalTime(), PhotonNetwork.CurrentRoom.PlayerCount - 1));
            }
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            Photon.Realtime.Room room = PhotonNetwork.CurrentRoom;
            OnRoyalTimeChanged(room.GetRoyalTime());
        }


        public void OnRoyalTimeChanged(int time)
        {
            if (time > 0)
            {
                DecreaseArea(time);

            }
        }
        public void ApplyAreaEffect()
        {
            GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
            List<Player> activePlayerList = new List<Player>();

            //get all Player components from root objects
            for (int i = 0; i < rootObjs.Length; i++)
            {
                Player p = rootObjs[i].GetComponentInChildren<Player>(true);
                if (p != null)
                {
                    if (p.gameObject.activeInHierarchy)
                    {
                        CheckPlayerLocationInArea(p);
                    }
                }

            }
        }

        private bool CheckPlayerLocationInArea(Player player)
        {
            if (PhotonNetwork.CurrentRoom.GetRoyalTime() > 0)
            {
                Vector2 playerPosition = new Vector2(player.gameObject.transform.position.x, player.gameObject.transform.position.z);
                if (playerPosition.x <= GetCurrentAreaPosX())
                {
                    player.TakeDamage(areaDamage, (short)(Time.time * 100));
                }
            }
            // x:x, y:z
            return false;
        }
        public void DecreaseArea(int time)
        {
            if (GetCurrentAreaPosX() >= finalAreaPosX)
            {
                return;
            }
            if (time % areaDecreaseInterval == 0)
            {
                //royalAreaLight.cookieSize = maxAreaSize - time * areaDecreaseSize / areaDecreaseInterval;
                StartCoroutine(DecreaseAreaEffect(GetCurrentAreaPosX(), startAreaPosX + (finalAreaPosX - startAreaPosX) * time / areaDecreaseTime, 0.5f));

            }

        }

        IEnumerator DecreaseAreaEffect(float start, float end, float t)
        {
            float timeElapsed = 0;
            while (timeElapsed < t)
            {
                float lerp = Mathf.Lerp(start, end, timeElapsed / t);
                SetAreaPos(lerp);
                if (GetCurrentAreaPosX() > finalAreaPosX)
                {
                    SetAreaPos(finalAreaPosX);
                    yield return null;
                }
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            if (GetCurrentAreaPosX() == finalAreaPosX)
            {
                SetAreaPos(finalAreaPosX);
            }
            else
            {
                SetAreaPos(end);
            }
        }

        private float GetCurrentAreaPosX()
        {
            return royalArea.position.x;
        }

        private void SetAreaPos(float x){
            Vector3 pos = royalArea.position;
            pos.x = x;
            royalArea.position = pos;
        }

        public void RestartGame(){

            PhotonNetwork.CurrentRoom.SetGameState(false);
            PhotonNetwork.CurrentRoom.MaxPlayers = (byte)(DedicatedServerManager.instance.royalTeamCount * DedicatedServerManager.instance.royalTeamMemberCount);
            PhotonNetwork.CurrentRoom.SetRoyalTime(-10);
            SetAreaPos(startAreaPosX);
        }
        
    }


}
