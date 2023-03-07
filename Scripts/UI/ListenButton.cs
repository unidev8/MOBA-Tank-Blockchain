using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListenButton : MonoBehaviour
{
    public bool isRefresh;
    public bool isLoginButton;
    public bool isLogOutButton;
    // Start is called before the first frame update
    void Start()
    {
        if(isRefresh){
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(UIManager.instance.OnRefresh);
        }
        
        if(isLogOutButton){
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(UIManager.instance.OnLogOut);
        }

        if(isLoginButton){
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(loginManager.instance.Login);
        }
        
    }

    private void OnDestroy() {
        if(isRefresh){
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(UIManager.instance.OnRefresh);
        }
        
        if(isLogOutButton){
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(UIManager.instance.OnLogOut);
        }

        if(isLoginButton){
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.RemoveListener(loginManager.instance.Login);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
