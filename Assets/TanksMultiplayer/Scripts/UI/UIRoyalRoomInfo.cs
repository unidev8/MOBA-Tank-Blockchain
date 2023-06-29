using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIRoyalRoomInfo : MonoBehaviour
{
    public GameObject content;
    public TMP_Text teamCountTxt;
    public TMP_Text teamMemberCountTxt;
    public TMP_Text rewardTxt;
    public TMP_Text playerJoinedCountTxt;
    public TMP_Text leftHourTxt;
    public TMP_Text leftMinuteTxt;
    public TMP_Text leftSecondTxt;

    public TMP_Text leftTimeTxt;
    private float lastUpdateTime;
    public int maxWaitingTime = 3;

    private void Start()
    {
        content.SetActive(false);
    }

    private void Update()
    {
        if (Time.time - lastUpdateTime > maxWaitingTime && content.activeInHierarchy)
        {
            content.SetActive(false);
        }
    }

    public void UpdateUI(FullRoyalRoomInfo info)
    {
        lastUpdateTime = Time.time;
        content.SetActive(true);
        teamCountTxt.text = info.roomInfo.royalTeamCount.ToString();
        teamMemberCountTxt.text = info.roomInfo.royalTeamMemberCount.ToString();
        rewardTxt.text = string.Format("DEFL : {0} \n EXP : {1}", info.roomInfo.rewardTokenAmount, info.roomInfo.rewardExpAmount);

        int hour = Mathf.Abs(info.roomState.time) / 3600;
        int minute = (Mathf.Abs(info.roomState.time) % 3600) / 60;
        int second = Mathf.Abs(info.roomState.time) % 60;

        leftHourTxt.text = hour.ToString() + "h";
        leftMinuteTxt.text = minute.ToString() + "m";
        leftSecondTxt.text = second.ToString() + "s";

        if(info.roomState.time <= 0){
            leftTimeTxt.text = "Starts in:";
        }
        else
        {
            leftTimeTxt.text = "Started:";
        }

        playerJoinedCountTxt.text = string.Format("{0} player joined", info.roomState.playerCount);
    }


}
