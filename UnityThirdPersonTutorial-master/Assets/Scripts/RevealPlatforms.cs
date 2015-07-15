using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UserPresenceComponent))]
public class RevealPlatforms : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            this.renderer.enabled = true;
            this.GetComponents<AudioSource>()[0].Play();
        }
    }

    void OnTriggerStay(Collider other)
    {
        Camera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();;
        if (other.gameObject.tag == "Player")
        {
            if (this.GetComponent<UserPresenceComponent>().IsUserPresent == false && GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ThirdPersonCamera>().EyeControlState != ThirdPersonCamera.EyeControlStates.No_Gaze)
            {
                this.renderer.enabled = false; ;
                this.GetComponents<AudioSource>()[1].Play();
                camera.cullingMask |= 1 << LayerMask.NameToLayer("HiddenUserPresence");
            }
            else if (GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ThirdPersonCamera>().EyeControlState == ThirdPersonCamera.EyeControlStates.No_Gaze)
            {
                if (Input.GetKey(KeyCode.Backslash))
                {
                    this.renderer.enabled = false; ;
                    this.GetComponents<AudioSource>()[1].Play();
                    camera.cullingMask |= 1 << LayerMask.NameToLayer("HiddenUserPresence");
                }
            }
        }
    }
}
