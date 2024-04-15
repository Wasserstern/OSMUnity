using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WorldSpaceScaler : MonoBehaviour
{
    public float canvasUnitWidth;
    public float canvasUnitHeight;
    public float canvasWidth;
    public float canvasHeight;
    RectTransform canvasTransform;
    Canvas canvas;
    Vector3 currentCanvasScale;
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasTransform = GetComponent<RectTransform>();
        currentCanvasScale = new Vector3(0f, 0f, 0f);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(canvas != null && canvas.renderMode == RenderMode.WorldSpace){
            Vector3 newCanvasScale = new Vector3(canvasUnitWidth / canvasWidth, canvasUnitWidth / canvasHeight, canvasUnitWidth / canvasWidth);
            if(currentCanvasScale != newCanvasScale){
                currentCanvasScale = newCanvasScale;
                canvasTransform.localScale = newCanvasScale;
            }
        }
    }
}
