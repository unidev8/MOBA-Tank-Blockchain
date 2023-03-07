using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ErrorDialog : MonoBehaviour
{
    // Start is called before the first frame update
    //Animator animator;
    //public UnityEngine.UI.Text info;
    public TMP_Text info;
    public float delay;
    Coroutine show;
    public GameObject window;
    void Start()
    {
        //animator = GetComponent<Animator>();
        //window.SetActive(false);
    }

    IEnumerator Hide(){
        yield return new WaitForSeconds(delay);
        window.SetActive(false);
        //animator.SetBool("Show", false);

        info.text = "";
    }

    public void Show(string txt){
        if(show != null)
            StopCoroutine(show);
        //animator.SetBool("Show", true);
        window.SetActive(true);
        info.text = txt;
        show = StartCoroutine(Hide());
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
