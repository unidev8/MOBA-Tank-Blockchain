using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TanksMP;

public class DedicatedServerManager : MonoBehaviour
{
    // Royal room info
    public string royalMap = "Map-4";
    public GameMode gameMode = GameMode.ROYAL;

    public int royalMapIndex = 1;
    public int royalTeamCount = 4;
    public int royalTeamMemberCount = 2;
    public int defaultRoyalTime = -10;
    public int minPlayerCount = 6;
    public int rewardTokenAmount = 500;
    public int rewardExpAmount = 10000;
    //
    public static DedicatedServerManager instance;
    public bool serverMode;

    #region startParams
    [SerializeField]
    private string username = "";
    [SerializeField]
    private string password = "";

    #endregion

    private const string usernameArg = "username:";
    private const string passwordArg = "password:";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        if (serverMode)
        {
            ParseCommandLineArguments();
        }
    }

    private void ParseCommandLineArguments()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log("start command args:" + i + " = " + args[i]);
            if (args[i].StartsWith(usernameArg) == true)
            {
                username = args[i].Replace(usernameArg, "");

            }
            else if (args[i].StartsWith(passwordArg) == true)
            {
                password = args[i].Replace(passwordArg, "");

            }
        }
        if (username == "" || password == "")
        {
            AppDelayQuit(2f, "username or password is null");
        }
    }

    public void Login()
    {
        loginManager.instance.ServerLogin(username, password);
    }

    public void CreateRoyalRoom()
    {
        Debug.Log(Helper.Colorize(string.Format("[Server] create royal room starting... [MAP INDEX] {0}, [TEAM COUNT] {1}, [TEAM MEMBER] {2}", royalMapIndex, royalTeamCount, royalTeamMemberCount), HexColors.DarkOrange, false));
        PlayerPrefs.SetInt(PrefsKeys.networkMode, (int)NetworkMode.Online);
        PlayerPrefs.SetInt(PrefsKeys.royalMapIndex, royalMapIndex);
        PlayerPrefs.SetString(PrefsKeys.map, royalMap);
        PlayerPrefs.SetInt(PrefsKeys.royalTeamCount, royalTeamCount);
        PlayerPrefs.SetInt(PrefsKeys.royalTeamMemberCount, royalTeamMemberCount);
        PlayerPrefs.SetInt(PrefsKeys.gameMode, (int)GameMode.ROYAL);
        PlayerPrefs.Save();
        MainPanel.instance.Play();
    }

    public bool IsServer()
    {
        return serverMode;
    }

    public void AppDelayQuit(float delay, string info = "null")
    {
        StartCoroutine(AppQuit(delay, info));
    }

    IEnumerator AppQuit(float delay, string info)
    {
        Debug.Log(Helper.Colorize(string.Format("[Server] will be closed after {0}s, [INFO] {1}", delay, info), HexColors.Red, true));
        yield return new WaitForSeconds(delay);
        Application.Quit();
    }

    public RoyalRoomInfoPayLod GetRoyalRoomInfo()
    {
        RoyalRoomInfoPayLod payload = new RoyalRoomInfoPayLod(royalTeamCount, royalTeamMemberCount, rewardTokenAmount, rewardExpAmount);
        return payload;
    }

}
