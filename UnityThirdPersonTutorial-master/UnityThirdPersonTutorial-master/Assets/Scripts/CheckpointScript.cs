using UnityEngine;
using System.Collections;

public class CheckpointScript : MonoBehaviour {
    [SerializeField]
    public static Vector3 lastCheckpoint;
    public Vector3 editorLastCheckpoint;
	// Use this for initialization
	void Start () {
        lastCheckpoint = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
           editorLastCheckpoint = lastCheckpoint = this.transform.position;
        }
       // editorLastCheckpoint = lastCheckpoint = this.transform.position;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            editorLastCheckpoint = lastCheckpoint = this.transform.position;
        }

       // editorLastCheckpoint = lastCheckpoint = this.transform.position;
    }
}
