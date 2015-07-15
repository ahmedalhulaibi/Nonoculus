using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(FixationDataComponent))]
public class ObjectSelection : MonoBehaviour
{

    public GameObject selected;
    public EyeXFixationPoint fixation;
    private FixationDataComponent FixationComponent;
    private GazePointDataComponent gazeData;
    private AudioSource confirmSelectionSound;
    public GameObject[] selectableObjects;
    public float distance;

    // Use this for initialization
    void Start()
    {
        FixationComponent = GetComponent<FixationDataComponent>();
        //FixationComponent.enabled = false;
        selectableObjects = GameObject.FindGameObjectsWithTag("MoveableObject");
        gazeData = GetComponent<GazePointDataComponent>();
        confirmSelectionSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

        //if nothing has been selected yet
        if (selected == null)
        {
            if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(1))
            {
                //Check if SelectableObjects have the user's Gaze
                for (int i = 0; i < selectableObjects.Length; i++)
                {
                    if (selectableObjects[i].GetComponent<SelectableObject>().GazeAwareComp.HasGaze)
                    {
                        selected = selectableObjects[i];
                        selected.GetComponent<SelectableObject>().selected = true;
                        confirmSelectionSound.Play();
                        break;
                    }
                }
                //If the GazeAwareComponent returns null, use fixation and ray cast to see if an object is selectable
                if (selected == null)
                {
                    RaycastHit hit;
                    Ray ray = new Ray();
                    if (GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ThirdPersonCamera>().EyeControlState == ThirdPersonCamera.EyeControlStates.No_Gaze)
                    {
                        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    }
                    else
                    {
                        //FixationComponent.enabled = false;
                        fixation = FixationComponent.LastFixation;

                        Debug.Log(fixation.GazePoint.Screen);

                        if (fixation.IsValid && fixation.GazePoint.IsWithinScreenBounds)
                        {
                            //get the point you are fixated on
                            Vector3 gazePointScreen = fixation.GazePoint.Screen;

                            Debug.Log(gazePointScreen);
                            //draw a ray from this point
                            ray = Camera.main.ScreenPointToRay(gazePointScreen);

                        }

                    }

                    //if the ray hits an object
                    if (Physics.Raycast(ray, out hit, 10000))
                    {
                        //select the given object if it can be selected
                        if (hit.collider.gameObject.tag == "MoveableObject")
                        {
                            //set the selected object to selected
                            hit.collider.GetComponent<SelectableObject>().selected = true;
                            confirmSelectionSound.Play();
                            //save that game object so that you can move it
                            selected = hit.collider.gameObject;

                            distance = transform.position.z - selected.transform.position.z;
                        }
                    }
                }

            }
        }
        // if there is an object selected
        else if (selected)
        {
            if (GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ThirdPersonCamera>().EyeControlState == ThirdPersonCamera.EyeControlStates.No_Gaze)
            {
                selected.GetComponent<SelectableObject>().target = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100));
            }
            else
            {
                Debug.Log(gazeData.LastGazePoint);
                if (gazeData.LastGazePoint.IsValid && gazeData.LastGazePoint.IsWithinScreenBounds)
                {
                    //keep object at the gaze point x units away
                    selected.GetComponent<SelectableObject>().target = Camera.main.ScreenToWorldPoint(new Vector3(gazeData.LastGazePoint.Screen.x, gazeData.LastGazePoint.Screen.y, 100));
                }
                //hit the j key while an object is selected to stop selecting the object

            }
            if (Input.GetKeyDown(KeyCode.N) || Input.GetMouseButtonDown(2))
            {
                selected.GetComponent<SelectableObject>().selected = false;
                selected = null;

            }


        }

    }
}
