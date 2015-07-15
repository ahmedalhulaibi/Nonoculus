using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GazePointDataComponent))]

public class GazePointShow : MonoBehaviour
{
    ThirdPersonCamera cam;
    private GazePointDataComponent gazeData;

    // Use this for initialization
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ThirdPersonCamera>();
        gazeData = GetComponent<GazePointDataComponent>();
        if (!gazeData)
        {
            gazeData = new GazePointDataComponent();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        
        if(cam.EyeControlState == ThirdPersonCamera.EyeControlStates.No_Gaze)
        {
            GUI.Box(new Rect(Input.mousePosition.x - 12.5f, (Screen.height - Input.mousePosition.y) - 12.5f, 25.0f, 25.0f), "x");
        }else if (gazeData)
        {
            if (gazeData.LastGazePoint.IsValid && gazeData.LastGazePoint.IsWithinScreenBounds)
            {
                GUI.Box(new Rect(gazeData.LastGazePoint.Screen.x - 12.5f, (Screen.height - gazeData.LastGazePoint.Screen.y) -12.5f, 25.0f, 25.0f), "x");
            }
        }
    }
}
