using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent (typeof (GazePointDataComponent))]

public class GazePathData : MonoBehaviour {
	private GazePointDataComponent gazeData;
    public bool drawLine = false;
	public List<Vector2> points_ScreenSpace = new List<Vector2>();
	public List<Vector3> points_WorldSpace = new List<Vector3>();
    private int cuttableLayer;
    ThirdPersonCamera cam;
    public GameObject explosionObj;
    float waitTime;
    Ray ray;
    private int numLinesDrawn = 0;
    [SerializeField]
    private const float drawDelay = 0.2f;
    private const int raysPerLine = 20;

    private Texture2D lineTex;

	// Use this for initialization
	void Start () 
	{
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ThirdPersonCamera>();
        cuttableLayer = 1 << LayerMask.NameToLayer("Cuttable");
		gazeData = GetComponent<GazePointDataComponent> ();
		if(!gazeData)
		{
			gazeData = new GazePointDataComponent();
		}

        lineTex = new Texture2D(2, 2);
        lineTex.SetPixel(0, 0, Color.white);
        lineTex.wrapMode = TextureWrapMode.Repeat;
        lineTex.Apply();
	}
	
	// Update is called once per frame
	void Update () 
	{
        if(drawLine)
        {
            waitTime += Time.deltaTime;
			if (gazeData) 
			{
				if(gazeData.LastGazePoint.IsValid && gazeData.LastGazePoint.IsWithinScreenBounds)
				{
					Vector2 point_ss = gazeData.LastGazePoint.Screen;
					//point_ss.y = Screen.height - point_ss.y; /* UNITY'S RAYCAST FUNCTION DOES THIS TRANSFORMATION FOR US*/

                    //LIMIT NUMBER OF POINTS STORED PER FRAME
                    if(points_ScreenSpace.Count > 0)
                    {
                        if (point_ss != points_ScreenSpace[points_ScreenSpace.Count - 1])
                        {
                            if(waitTime >= drawDelay && points_ScreenSpace.Count < 50)
                            {
                                waitTime = 0.0f;
                                points_ScreenSpace.Add(point_ss);
                            }
                        }
                    }
                    else
                    {
                        points_ScreenSpace.Add(point_ss);
                    }
                    points_WorldSpace.Add(camera.ScreenToWorldPoint(new Vector3(point_ss.x, point_ss.y, camera.nearClipPlane)));
				}
			}
            if (cam.EyeControlState == ThirdPersonCamera.EyeControlStates.No_Gaze)
            {
                Vector2 point_ss = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                // point_ss.y = Screen.height - point_ss.y;  /* UNITY'S RAYCAST FUNCTION DOES THIS TRANSFORMATION FOR US*/

                //LIMIT NUMBER OF POINTS STORED PER FRAME
                if (points_ScreenSpace.Count > 0)
                {
                    if (point_ss != points_ScreenSpace[points_ScreenSpace.Count - 1])
                    {
                        if (waitTime >= drawDelay && points_ScreenSpace.Count < 50)
                        {
                            waitTime = 0.0f;
                            points_ScreenSpace.Add(point_ss);
                        }
                    }
                }
                else
                {
                    points_ScreenSpace.Add(point_ss);
                }
                points_WorldSpace.Add(camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane)));
            }
		}
        else if (!drawLine)
		{
			points_WorldSpace.Clear ();
			points_ScreenSpace.Clear ();
            numLinesDrawn = 0;
		}
	}

    private void LineDraw(Vector2 start, Vector2 end, Texture2D point, int width = 2)
    {
        Vector2 d = end - start;
        float a = Mathf.Rad2Deg * Mathf.Atan(d.y / d.x);
        if (d.x < 0)
            a += 180;

        int width2 = (int)Mathf.Ceil(width / 2);

        GUIUtility.RotateAroundPivot(a, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width2, d.magnitude, width), point);
        GUIUtility.RotateAroundPivot(-a, start);
    }

	void OnGUI()
	{
		for(int i = 0; i + 1 < points_ScreenSpace.Count; i++)
		{
            //TRANSFORM SCREEN SPACE Y FOR DRAWING ONLY
            LineDraw(new Vector2(points_ScreenSpace[i].x, Screen.height - points_ScreenSpace[i].y),new Vector2(points_ScreenSpace[i + 1].x,Screen.height - points_ScreenSpace[i + 1].y), lineTex);

            if(i == points_ScreenSpace.Count - 2)
            {
                numLinesDrawn = points_ScreenSpace.Count;
            }
		}
	}

    void LateUpdate()
    {
        for (int i = 0; i + 1 < points_ScreenSpace.Count; i++)
        {
            if (i < numLinesDrawn + 1)
            {
                //Debug.Log("Raycasting Line " + i.ToString());
                for (int j = 0; j < raysPerLine + 1; j++)
                {
                    Vector2 vec2 = Vector2.Lerp(points_ScreenSpace[i], points_ScreenSpace[i + 1], ((float)j / (float)raysPerLine));
                    ray = Camera.main.ScreenPointToRay(new Vector3(vec2.x, vec2.y, 0.0f));
                    RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(ray, out hit, 10000.0f, cuttableLayer))
                    {
                        Instantiate(explosionObj);
                        explosionObj.transform.position = hit.collider.transform.position;
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
            if (i == points_ScreenSpace.Count - 2)
            {
                numLinesDrawn = points_ScreenSpace.Count;
            }
        }
    }
}
