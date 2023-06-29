using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TanksMP;


public class EnvironmentManager : MonoBehaviour
{
    public Transform[] teamPositions;
    private void Awake()
    {
        //Debug.Log("netwokkMode:" + ((NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode)).ToString());
        //int nm = PlayerPrefs.GetInt(PrefsKeys.networkMode);
        if ((NetworkMode)PlayerPrefs.GetInt(PrefsKeys.networkMode) != NetworkMode.Offline)
        {
            TanksMP.GameManager.GetInstance().SetTeamSpwanPosition(teamPositions);
        }
    }

    void Start()
    {
        TanksMP.GameManager.GetInstance().SetTeamSpwanPosition(teamPositions);
    }
}
