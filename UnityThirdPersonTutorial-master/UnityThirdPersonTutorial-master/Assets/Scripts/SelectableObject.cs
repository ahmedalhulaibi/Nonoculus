using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GazeAwareComponent))]
public class SelectableObject : MonoBehaviour
{
    public GazeAwareComponent GazeAwareComp;
    public Vector3 target;
    public bool selected;
    private bool gravity;
    public float speed = 3.0f;
    public bool Freeze_X = false;
    public bool Freeze_Y = false;
    public bool Freeze_Z = false;
    public float MIN_X = -100000.0f;
    public float MAX_X = 100000.0f;
    public float MIN_Y = -100000.0f;
    public float MAX_Y = 100000.0f;
    public float MIN_Z = -100000.0f;
    public float MAX_Z = 100000.0f;
    public Color color;

    private AudioSource isSelectedSound;
    // Use this for initialization
    void Start()
    {
        GazeAwareComp = GetComponent<GazeAwareComponent>();
        target = Vector3.zero;//target is set in ObjectSelection script
        selected = false;
        color = this.renderer.material.color;
        isSelectedSound = this.GetComponent<AudioSource>();
       // gravity = GetComponent<Rigidbody>().useGravity;
    }

    // Update is called once per frame
    void Update()
    {
        
        //if the object is selected
        if (selected == true)
        {
            if (!isSelectedSound.isPlaying)
            {
                isSelectedSound.Play();
            }
          //  GetComponent<Rigidbody>().useGravity = false;
            float step = speed * Time.deltaTime;
            if (Freeze_X)
            {
                target.x = this.transform.position.x;
            }
            if (Freeze_Y)
            {
                target.y = this.transform.position.y;
            }
            if (Freeze_Z)
            {
                target.z = this.transform.position.z;
            }
            transform.position = Vector3.Lerp(transform.position, target, step);
            this.renderer.material.color = new Color(1.0f, 0.0f, 0.0f);
        }
        else
        {
           // transform.position = transform.position;
            //transform.position = Vector3.Lerp(transform.position, transform.position, 0.0f);
           // GetComponent<Rigidbody>().useGravity = gravity;
            this.renderer.material.color = color;
        }

        if (transform.position.x > MAX_X)
        {
            transform.position = new Vector3(MAX_X, transform.position.y, transform.position.z);
        }
        if (transform.position.x < MIN_X)
        {
            transform.position = new Vector3(MIN_X, transform.position.y, transform.position.z);
        }
        if (transform.position.y > MAX_Y)
        {
            transform.position = new Vector3(transform.position.x, MAX_Y, transform.position.z);
        }
        if (transform.position.y < MIN_Y)
        {
            transform.position = new Vector3(transform.position.x, MIN_Y, transform.position.z);
        }
        if (transform.position.z > MAX_Z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, MAX_Z);
        }
        if (transform.position.z < MIN_Z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, MIN_Z);
        }

    }
}
