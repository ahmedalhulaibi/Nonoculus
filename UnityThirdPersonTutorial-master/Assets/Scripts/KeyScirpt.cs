using UnityEngine;
using System.Collections;

public class KeyScirpt : MonoBehaviour {
    public static int KeysCollected = 0;
    private bool isCollected = false;
    private float initialY;
    private bool goingUp = true;
    public float MaxY;
    public float MinY;
	// Use this for initialization
	void Start () {
        initialY = this.transform.position.y;
	}
	
	// Update is called once per frame
    void Update()
    {
        if (goingUp && !isCollected)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y + 1, this.transform.position.z), Time.deltaTime);
            if (this.transform.position.y >= MaxY)
            {
                goingUp = false;
            }
        }
        else if(!goingUp && !isCollected)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y - 1, this.transform.position.z), Time.deltaTime);
            if (this.transform.position.y <= MinY)
            {
                goingUp = true;
            }
        }
        this.transform.Rotate(Vector3.up, 2);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !isCollected)
        {
            KeysCollected++;
            isCollected = true;
            GetComponent<AudioSource>().Play();
        }
    }

}
