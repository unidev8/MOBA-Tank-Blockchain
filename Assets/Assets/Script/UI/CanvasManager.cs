using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    // Start is called before the first frame update
    public CanvasScaler mainCanvas;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        ScaleFix();
    }

    void ScaleFix()
    {
        if (mainCanvas == null)
        {
            GameObject mainCanvasObject = GameObject.Find("Canvas UI");
            if (mainCanvasObject == null) return;
            mainCanvas = mainCanvasObject.GetComponent<CanvasScaler>();
        }
        //float defaultRatio = 2560f / 1440f;
        float currentRatio = Screen.width / Screen.height;
        float scale_h = Screen.height / 1440f;
        mainCanvas.scaleFactor = 3f * scale_h;
    }
}
