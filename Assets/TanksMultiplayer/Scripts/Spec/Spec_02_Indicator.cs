using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TanksMP;

public class Spec_02_Indicator : MonoBehaviour 
{
    private float despawnDelay = SpecConsts.cSpec_Delay_Time[SpecConsts.cSpec_0202_Rocket];
    [HideInInspector]
    public GameObject spec_Manager;
   
    void OnSpawn()
    {       
        
    }
    void OnDespawn()
    {
        if (spec_Manager)
        {
            //spec_Manager.GetComponent<Spec_Manager>().cooldownStart = Time.time;
            //spec_Manager.GetComponent<Spec_Manager>().isPossible = false;
            //spec_Manager.GetComponent<Spec_Manager>().curIdx = SpecConsts.cSpec_noSpec;
        }            
    }
}
