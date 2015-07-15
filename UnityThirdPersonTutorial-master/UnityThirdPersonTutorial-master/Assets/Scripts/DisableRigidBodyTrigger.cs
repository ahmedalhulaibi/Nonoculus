﻿using UnityEngine;
using System.Collections;

public class DisableRigidBodyTrigger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "RigidBodyCube")
        {
            Destroy(other.rigidbody);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "RigidBodyCube")
        {
        }
    }
}
