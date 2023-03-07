using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BotWheelRoate : MonoBehaviour
{    
    public GameObject mainBody;

    float speedRatio = 555.0f;

    // Start is called before the first frame update
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Horizontal val: " + Input.GetAxis("Vertical"));
        Vector2 inp = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        float speedInp = 1.0f * Time.deltaTime * speedRatio;// inp.magnitude;
        /*if (transform.localPosition.x > 0)
        {
            speedInp = Mathf.Abs( Input.GetAxis("Vertical") - Input.GetAxis("Horizontal"));

        }
        else
        {
            speedInp = Mathf.Abs (Input.GetAxis("Vertical") + Input.GetAxis("Horizontal"));
        }*/
        //if (mainBody.GetComponent<Rigidbody>().velocity.y > 0)
        {
            transform.Rotate(speedInp, 0, 0, Space.Self);
        }
    }
}
