using UnityEngine;
using System.Collections;

public class Footstep : MonoBehaviour {
    bool onGround = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Physics.Raycast(this.transform.position, Vector3.down, 0.01f) && !onGround)
        {
            this.GetComponent<AudioSource>().Play();
            onGround = true;
        }
        else
        {
            onGround = false;
        }
	}
}
