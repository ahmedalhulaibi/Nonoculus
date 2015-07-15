using UnityEngine;
using System.Collections;

public class GoToLastCheckpointScript : MonoBehaviour {
    public Vector3 lastCheckpoint;
    // Use this for initialization
    void Start()
    {
        lastCheckpoint = CheckpointScript.lastCheckpoint;
    }

    // Update is called once per frame
    void Update()
    {
        lastCheckpoint = CheckpointScript.lastCheckpoint;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.transform.position = lastCheckpoint;
            this.GetComponent<AudioSource>().Play();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.transform.position = lastCheckpoint;
        }
    }
}
