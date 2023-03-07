using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static UIManager instance;
    public ErrorDialog errorDialog;
    public UnityEngine.UI.Text debugUI;
    public GameObject loddingUI;
    public bool isDebug;

    void Start()
    {
        Debug.Log("start");
        UIManager.instance.ShowLoading(true);
    }

    private void Awake() {
        if(instance == null){
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Error(string error){
        errorDialog.Show(error);
    }

    public void AddDebug(string debug){
        if(!isDebug) return;
        debugUI.text += debug;
    }

    public void OnRefresh(){
        ShowLoading(true);
        SocketGameManager.instance.GetUserData();
    }

    public void OnLogOut(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        loginManager.instance.LogOut();
    }

    public void ShowLoading(bool isLoading){
        loddingUI.SetActive(isLoading); 
    }
}
