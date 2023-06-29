using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGroup : MonoBehaviour
{
    public List<TankItem> selectedUIs = new List<TankItem>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelected(){
        foreach(var item in selectedUIs){
            item.isActive = false;
            item.selectedUI.SetActive(false);
        }
    }
}
