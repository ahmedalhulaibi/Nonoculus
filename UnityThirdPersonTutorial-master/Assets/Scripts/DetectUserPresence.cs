using UnityEngine;
using System.Collections;

[RequireComponent (typeof(UserPresenceComponent))]

public class DetectUserPresence : MonoBehaviour {

    private UserPresenceComponent userPresence;
    public Light directionalLight;
    private GameObject mainCam;

	// Use this for initialization
	void Start () {
        userPresence = GetComponent<UserPresenceComponent>();
        directionalLight = gameObject.GetComponent<Light>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	// Update is called once per frame
	void Update () {

        if (userPresence.IsValid &&
            userPresence.IsUserPresent && mainCam.GetComponent<ThirdPersonCamera>().EyeControlState != ThirdPersonCamera.EyeControlStates.No_Gaze)
        {
            directionalLight.color = Color.white;
        }
        else if(mainCam.GetComponent<ThirdPersonCamera>().EyeControlState != ThirdPersonCamera.EyeControlStates.No_Gaze)
        {
            directionalLight.color = Color.red;
        }else if(mainCam.GetComponent<ThirdPersonCamera>().EyeControlState == ThirdPersonCamera.EyeControlStates.No_Gaze)
        {
            if (Input.GetKey(KeyCode.Backslash))
            {
                directionalLight.color = Color.red;
            }
            else
            {
                directionalLight.color = Color.white;
            }
        }
	
	}
}
