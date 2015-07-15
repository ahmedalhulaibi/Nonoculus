/// <summary>
/// UnityTutorials - A Unity Game Design Prototyping Sandbox
/// <copyright>(c) John McElmurray and Julian Adams 2013</copyright>
/// 
/// UnityTutorials homepage: https://github.com/jm991/UnityTutorials
/// 
/// This software is provided 'as-is', without any express or implied
/// warranty.  In no event will the authors be held liable for any damages
/// arising from the use of this software.
///
/// Permission is granted to anyone to use this software for any purpose,
/// and to alter it and redistribute it freely, subject to the following restrictions:
///
/// 1. The origin of this software must not be misrepresented; you must not
/// claim that you wrote the original software. If you use this software
/// in a product, an acknowledgment in the product documentation would be
/// appreciated but is not required.
/// 2. Altered source versions must be plainly marked as such, and must not be
/// misrepresented as being the original software.
/// 3. This notice may not be removed or altered from any source distribution.
/// </summary>
using UnityEngine;
using System.Collections;
//using UnityEditor;


/// <summary>
/// Struct to hold data for aligning camera
/// </summary>
struct CameraPosition
{
    // Position to align camera to, probably somewhere behind the character
    // or position to point camera at, probably somewhere along character's axis
    private Vector3 position;
    // Transform used for any rotation
    private Transform xForm;

    public Vector3 Position { get { return position; } set { position = value; } }
    public Transform XForm { get { return xForm; } set { xForm = value; } }

    public void Init(string camName, Vector3 pos, Transform transform, Transform parent)
    {
        position = pos;
        xForm = transform;
        xForm.name = camName;
        xForm.parent = parent;
        xForm.localPosition = Vector3.zero;
        xForm.localPosition = position;
    }
}

/// <summary>
/// #DESCRIPTION OF CLASS#
/// </summary>
[RequireComponent(typeof(BarsEffect))]
[RequireComponent(typeof(GazePointDataComponent))]
[RequireComponent(typeof(FixationDataComponent))]
public class ThirdPersonCamera : MonoBehaviour
{
    #region Variables (private)

    // Inspector serialized	
    [SerializeField]
    private Transform cameraXform;
    [SerializeField]
    private float distanceAway;
    [SerializeField]
    private float distanceAwayMultipler = 1.5f;
    [SerializeField]
    private float distanceUp;
    [SerializeField]
    private float distanceUpMultiplier = 5f;
    [SerializeField]
    private CharacterControllerLogic follow;
    [SerializeField]
    private Transform followXform;
    [SerializeField]
    private float widescreen = 0.2f;
    [SerializeField]
    private float targetingTime = 0.5f;
    [SerializeField]
    private float firstPersonLookSpeed = 3.0f;
    [SerializeField]
    private float freeThreshold = -0.1f;
    [SerializeField]
    private Vector2 camMinDistFromChar = new Vector2(1f, -0.5f);
    [SerializeField]
    private float rightStickThreshold = 0.1f;
    [SerializeField]
    private const float freeRotationDegreePerSecond = -5f;
    [SerializeField]
    private float mouseWheelSensitivity = 3.0f;
    [SerializeField]
    private float compensationOffset = 0.2f;
    [SerializeField]
    private CamStates startingState = CamStates.Free;
    [SerializeField]
    private FreeCameraViewModes startingFreeCameraViewMode = FreeCameraViewModes.OverTheShoulder;
    [SerializeField]
    private Transform centeredTarget;
    [SerializeField]
    private Transform overTheShoulderTarget;
    [SerializeField]
    private EyeControlStates startingEyeState = EyeControlStates.Gaze_Filtered;
    [SerializeField]
    private KeyCode freezeCameraKey = KeyCode.F;
    [SerializeField]
    private KeyCode freezeCameraKey2 = KeyCode.J;

    // Smoothing and damping
    private Vector3 velocityCamSmooth = Vector3.zero;
    [SerializeField]
    private float camSmoothDampTime = 0.1f;
    private Vector3 velocityLookDir = Vector3.zero;
    [SerializeField]
    private float lookDirDampTime = 0.1f;




    // Private global only
    private Vector3 lookDir;
    private Vector3 curLookDir;
    private BarsEffect barEffect;
    private CamStates camState = CamStates.Free;
    private FreeCameraViewModes freeCameraViewMode = FreeCameraViewModes.OverTheShoulder;
    private float xAxisRot = 0.0f;
    private float lookWeight;
    private const float TARGETING_THRESHOLD = 0.01f;
    private Vector3 savedRigToGoal;
    private float distanceAwayFree;
    private float distanceUpFree;
    private Vector2 rightStickPrevFrame = Vector2.zero;
    private float lastStickMin = float.PositiveInfinity;	// Used to prevent from zooming in when holding back on the right stick/scrollwheel
    private Vector3 nearClipDimensions = Vector3.zero; // width, height, radius
    private Vector3[] viewFrustum;
    private Vector3 characterOffset;
    private Vector3 targetPosition;

    // Eye trakcer variables only
    private FixationDataComponent fixationData;
    private GazePointDataComponent gazeData;
    private EyeControlStates eyeControlState = EyeControlStates.Fixation_Sensitive;
    private bool freezeCameraKeyDown;

    #endregion


    #region Properties (public)

    public Transform CameraXform
    {
        get
        {
            return this.cameraXform;
        }
    }

    public Vector3 LookDir
    {
        get
        {
            return this.curLookDir;
        }
    }

    public CamStates CamState
    {
        get
        {
            return this.camState;
        }
    }

    public enum CamStates
    {
        Behind,			// Single analog stick, Japanese-style; character orbits around camera; default for games like Mario64 and 3D Zelda series
        FirstPerson,	// Traditional 1st person look around
        Target,			// L-targeting variation on "Behind" mode
        Free			// High angle; character moves relative to camera facing direction
    }

    public EyeControlStates EyeControlState
    {
        get
        {
            return this.eyeControlState;
        }
    }

    public enum EyeControlStates
    {
        Gaze_Unfiltered,
        Gaze_Filtered,
        Fixation_Sensitive,
        Fixation_Slow,
        No_Gaze
    }

    public FreeCameraViewModes FreeCameraViewMode
    {
        get
        {
            return freeCameraViewMode;
        }
    }

    public enum FreeCameraViewModes
    {
        OverTheShoulder,
        Centered
    }

    public Vector3 RigToGoalDirection
    {
        get
        {
            // Move height and distance from character in separate parentRig transform since RotateAround has control of both position and rotation
            Vector3 rigToGoalDirection = Vector3.Normalize(characterOffset - this.transform.position);
            // Can't calculate distanceAway from a vector with Y axis rotation in it; zero it out
            rigToGoalDirection.y = 0f;

            return rigToGoalDirection;
        }
    }

    #endregion


    #region Unity event functions

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    void Start()
    {
        cameraXform = this.transform;//.parent;
        if (cameraXform == null)
        {
            Debug.LogError("Parent camera to empty GameObject.", this);
        }

        follow = GameObject.FindWithTag("Player").GetComponent<CharacterControllerLogic>();
        followXform = GameObject.FindWithTag("CameraFollowXForm").transform;
        centeredTarget = GameObject.FindWithTag("CenteredTarget").transform;
        overTheShoulderTarget = GameObject.FindWithTag("OverTheShoulderTarget").transform;

        lookDir = followXform.forward;
        curLookDir = followXform.forward;

        barEffect = GetComponent<BarsEffect>();
        if (barEffect == null)
        {
            Debug.LogError("Attach a widescreen BarsEffect script to the camera.", this);
        }


        camState = startingState;
        eyeControlState = startingEyeState;
        freeCameraViewMode = startingFreeCameraViewMode;

        if (freeCameraViewMode == FreeCameraViewModes.Centered)
        {
            //set position to centered target
            this.transform.position = centeredTarget.transform.position;
        }
        else
        {
            this.transform.position = overTheShoulderTarget.transform.position;
        }


        // Intialize values to avoid having 0s
        characterOffset = followXform.position + new Vector3(0f, distanceUp, 0f);
        distanceUpFree = distanceUp;
        distanceAwayFree = distanceAway;
        savedRigToGoal = RigToGoalDirection;


        //Eye tracking code
        fixationData = GetComponent<FixationDataComponent>();
        gazeData = GetComponent<GazePointDataComponent>();
        if (!fixationData)
        {
            fixationData = new FixationDataComponent();
        }
        if (!gazeData)
        {
            gazeData = new GazePointDataComponent();
        }

    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        bool wKeyDown = Input.GetKey(KeyCode.W);
        bool aKeydown = Input.GetKey(KeyCode.A);
        bool sKeyDown = Input.GetKey(KeyCode.S);
        bool dKeyDown = Input.GetKey(KeyCode.D);
        freezeCameraKeyDown = Input.GetKey(freezeCameraKey) || Input.GetKey(freezeCameraKey2) || Input.GetMouseButton(0);
        if (wKeyDown || aKeydown || sKeyDown || dKeyDown)
        {
            SetCentered();
        }
        else
        {
            SetOverTheShoulder();
        }
        if (freezeCameraKeyDown)
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GazePathData>().drawLine = true;
        }
        else
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<GazePathData>().drawLine = false;
        }
    }

    /// <summary>
    /// Debugging information should be put here.
    /// </summary>
    void OnDrawGizmos()
    {
      //  if (EditorApplication.isPlaying && !EditorApplication.isPaused)
      //  {
       //     DebugDraw.DrawDebugFrustum(viewFrustum);
     //   }
    }

    void LateUpdate()
    {
        viewFrustum = DebugDraw.CalculateViewFrustum(camera, ref nearClipDimensions);

        // Pull values from controller/keyboard
        float rightX = Input.GetAxis("RightStickX") * -1f;
        float rightY = Input.GetAxis("RightStickY");
        float leftX = Input.GetAxis("Horizontal");
        float leftY = Input.GetAxis("Vertical");
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        float mouseWheelScaled = mouseWheel * mouseWheelSensitivity;
        float leftTrigger = Input.GetAxis("Target");
        bool bButtonPressed = Input.GetButton("ExitFPV");
        bool qKeyDown = Input.GetKey(KeyCode.Q);
        bool eKeyDown = Input.GetKey(KeyCode.E);
        bool lShiftKeyDown = Input.GetKey(KeyCode.LeftShift);

        float eyeX;
        float eyeY;

        #region Eye Input Update
        switch (eyeControlState)
        {
            case EyeControlStates.Fixation_Sensitive:
                fixationData.fixationDataMode = Tobii.EyeX.Framework.FixationDataMode.Sensitive;
                eyeX = (fixationData.LastFixation.GazePoint.Screen.x - (Screen.width / 2)) / (Screen.width / 2);
                eyeX *= -1f;
                eyeY = (fixationData.LastFixation.GazePoint.Screen.y - (Screen.height / 2)) / (Screen.height / 2);
                if (fixationData)
                {
                    if (fixationData.LastFixation.GazePoint.IsValid && fixationData.LastFixation.GazePoint.IsWithinScreenBounds)
                    {
                        rightX = eyeX;
                        rightY = eyeY;
                    }
                    else
                    {
                        rightX = 0;
                        rightY = 0;
                    }
                }
                break;
            case EyeControlStates.Fixation_Slow:
                fixationData.fixationDataMode = Tobii.EyeX.Framework.FixationDataMode.Slow;
                eyeX = (fixationData.LastFixation.GazePoint.Screen.x - (Screen.width / 2)) / (Screen.width / 2);
                eyeX *= -1f;
                eyeY = (fixationData.LastFixation.GazePoint.Screen.y - (Screen.height / 2)) / (Screen.height / 2);
                if (fixationData)
                {
                    if (fixationData.LastFixation.GazePoint.IsValid && fixationData.LastFixation.GazePoint.IsWithinScreenBounds)
                    {
                        rightX = eyeX;
                        rightY = eyeY;
                    }
                    else
                    {
                        rightX = 0;
                        rightY = 0;
                    }
                }
                break;
            case EyeControlStates.Gaze_Filtered:
                gazeData.gazePointDataMode = Tobii.EyeX.Framework.GazePointDataMode.LightlyFiltered;
                eyeX = (gazeData.LastGazePoint.Screen.x - (Screen.width / 2)) / (Screen.width / 2);
                eyeX *= -1f;
                eyeY = (gazeData.LastGazePoint.Screen.y - (Screen.height / 2)) / (Screen.height / 2);
                if (gazeData)
                {
                    if (gazeData.LastGazePoint.IsValid && gazeData.LastGazePoint.IsWithinScreenBounds)
                    {
                        rightX = eyeX;
                        rightY = eyeY;
                    }
                    else
                    {
                        rightX = 0;
                        rightY = 0;
                    }
                }
                break;
            case EyeControlStates.Gaze_Unfiltered:
                gazeData.gazePointDataMode = Tobii.EyeX.Framework.GazePointDataMode.Unfiltered;
                eyeX = (gazeData.LastGazePoint.Screen.x - (Screen.width / 2)) / (Screen.width / 2);
                eyeX *= -1f;
                eyeY = (gazeData.LastGazePoint.Screen.y - (Screen.height / 2)) / (Screen.height / 2);
                if (gazeData)
                {
                    if (gazeData.LastGazePoint.IsValid && gazeData.LastGazePoint.IsWithinScreenBounds)
                    {
                        rightX = eyeX;
                        rightY = eyeY;
                    }
                    else
                    {
                        rightX = 0;
                        rightY = 0;
                    }
                }
                break;
            case EyeControlStates.No_Gaze:
                //default to use mouse to emulate eye behaviour
                eyeX = (Input.mousePosition.x - (Screen.width / 2)) / (Screen.width / 2) * (-1f);
                eyeY = (Input.mousePosition.y - (Screen.height / 2)) / (Screen.height / 2);
                rightX = eyeX;
                rightY = eyeY;
                break;
        }
        #endregion

        // Abstraction to set right Y when using mouse
        if (mouseWheel != 0)
        {
          //  rightY = mouseWheelScaled;
        }
        if (qKeyDown)
        {
          //  rightX = 1;
        }
        if (eKeyDown)
        {
        //    rightX = -1;
        }
        if (lShiftKeyDown)
        {
            leftTrigger = 1;
        }
        if (freezeCameraKeyDown)
        {
            rightX = 0;
            rightY = 0;
        }

        characterOffset = followXform.position + (distanceUp * followXform.up);
        Vector3 lookAt = characterOffset;
        targetPosition = Vector3.zero;

        // Set the Look At Weight - amount to use look at IK vs using the head's animation
        follow.Animator.SetLookAtWeight(lookWeight);
        UpdateFollowXFormPosition();
        // Execute camera state
        switch (camState)
        {
            case CamStates.Behind:
                ResetCamera();

                // Only update camera look direction if moving
                if (follow.Speed > follow.LocomotionThreshold && follow.IsInLocomotion() && !follow.IsInPivot())
                {
                    lookDir = Vector3.Lerp(followXform.right * (leftX < 0 ? 1f : -1f), followXform.forward * (leftY < 0 ? -1f : 1f), Mathf.Abs(Vector3.Dot(this.transform.forward, followXform.forward)));
                    Debug.DrawRay(this.transform.position, lookDir, Color.white);

                    // Calculate direction from camera to player, kill Y, and normalize to give a valid direction with unit magnitude
                    curLookDir = Vector3.Normalize(characterOffset - this.transform.position);
                    curLookDir.y = 0;
                    Debug.DrawRay(this.transform.position, curLookDir, Color.green);

                    // Damping makes it so we don't update targetPosition while pivoting; camera shouldn't rotate around player
                    curLookDir = Vector3.SmoothDamp(curLookDir, lookDir, ref velocityLookDir, lookDirDampTime);
                }

                targetPosition = characterOffset + followXform.up * distanceUp - Vector3.Normalize(curLookDir) * distanceAway;
                Debug.DrawLine(followXform.position, targetPosition, Color.magenta);

                break;
            case CamStates.Target:
                ResetCamera();
                lookDir = followXform.forward;
                curLookDir = followXform.forward;

                targetPosition = characterOffset + followXform.up * distanceUp - lookDir * distanceAway;

                break;
            case CamStates.Free:
                lookWeight = Mathf.Lerp(lookWeight, 0.0f, Time.deltaTime * firstPersonLookSpeed);

                Vector3 rigToGoal = characterOffset - cameraXform.position;
                rigToGoal.y = 0f;
                Debug.DrawRay(cameraXform.transform.position, rigToGoal, Color.red);

                // Panning in and out
                // If statement works for positive values; don't tween if stick not increasing in either direction; also don't tween if user is rotating
                // Checked against rightStickThreshold because very small values for rightY mess up the Lerp function
                if (rightY < lastStickMin && rightY < -1f * rightStickThreshold && rightY <= rightStickPrevFrame.y && Mathf.Abs(rightX) < rightStickThreshold)
                {
                    // Zooming out
                    distanceUpFree = Mathf.Lerp(distanceUp, distanceUp * distanceUpMultiplier, Mathf.Abs(rightY));
                    distanceAwayFree = Mathf.Lerp(distanceAway, distanceAway * distanceAwayMultipler, Mathf.Abs(rightY));
                    targetPosition = characterOffset + followXform.up * distanceUpFree - RigToGoalDirection * distanceAwayFree;
                    lastStickMin = rightY;
                }
                else if (rightY > rightStickThreshold && rightY >= rightStickPrevFrame.y && Mathf.Abs(rightX) < rightStickThreshold)
                {
                    // Zooming in
                    // Subtract height of camera from height of player to find Y distance
                    distanceUpFree = Mathf.Lerp(Mathf.Abs(transform.position.y - characterOffset.y), camMinDistFromChar.y, rightY);
                    // Use magnitude function to find X distance	
                    distanceAwayFree = Mathf.Lerp(rigToGoal.magnitude, camMinDistFromChar.x, rightY);
                    targetPosition = characterOffset + followXform.up * distanceUpFree - RigToGoalDirection * distanceAwayFree;
                    lastStickMin = float.PositiveInfinity;
                }

                // Store direction only if right stick inactive
                if (rightX != 0 || rightY != 0)
                {
                    savedRigToGoal = RigToGoalDirection;
                }


                // Rotating around character
                cameraXform.RotateAround(characterOffset, followXform.up, freeRotationDegreePerSecond * (Mathf.Abs(rightX) > rightStickThreshold ? rightX : 0f));

                // Still need to track camera behind player even if they aren't using the right stick; achieve this by saving distanceAwayFree every frame
                if (targetPosition == Vector3.zero)
                {
                    targetPosition = characterOffset + followXform.up * distanceUpFree - savedRigToGoal * distanceAwayFree;
                }

                break;
        }


        CompensateForWalls(characterOffset, ref targetPosition);
        SmoothPosition(cameraXform.position, targetPosition);
        transform.LookAt(lookAt);

        // Make sure to cache the unscaled mouse wheel value if using mouse/keyboard instead of controller
        rightStickPrevFrame = new Vector2(rightX, rightY);//mouseWheel != 0 ? mouseWheelScaled : rightY);
    }

    #endregion


    #region Methods

    public void SetOverTheShoulder()
    {
        freeCameraViewMode = FreeCameraViewModes.OverTheShoulder;
    }

    public void SetCentered()
    {
        freeCameraViewMode = FreeCameraViewModes.Centered;
    }

    private void UpdateFollowXFormPosition()
    {
        if (freeCameraViewMode == FreeCameraViewModes.Centered)
        {
            //lerp camera target (followXform) from current position to centered target
            this.followXform.position = Vector3.Lerp(this.followXform.position, this.centeredTarget.position, Time.deltaTime);
        }
        if (freeCameraViewMode == FreeCameraViewModes.OverTheShoulder)
        {
            //lerp camera target (followXform) from current position to over the shoulder
            this.followXform.position = Vector3.Lerp(this.followXform.position, this.overTheShoulderTarget.position, Time.deltaTime);
        }
    }

    private void SmoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        // Making a smooth transition between camera's current position and the position it wants to be in
        cameraXform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
    {
        // Compensate for walls between camera
        RaycastHit wallHit = new RaycastHit();
        if (Physics.Linecast(fromObject, toTarget, out wallHit))
        {
            Debug.DrawRay(wallHit.point, wallHit.normal, Color.red);
            toTarget = wallHit.point;
        }

        // Compensate for geometry intersecting with near clip plane
        Vector3 camPosCache = camera.transform.position;
        camera.transform.position = toTarget;
        viewFrustum = DebugDraw.CalculateViewFrustum(camera, ref nearClipDimensions);

        for (int i = 0; i < (viewFrustum.Length / 2); i++)
        {
            RaycastHit cWHit = new RaycastHit();
            RaycastHit cCWHit = new RaycastHit();

            // Cast lines in both directions around near clipping plane bounds
            while (Physics.Linecast(viewFrustum[i], viewFrustum[(i + 1) % (viewFrustum.Length / 2)], out cWHit) ||
                   Physics.Linecast(viewFrustum[(i + 1) % (viewFrustum.Length / 2)], viewFrustum[i], out cCWHit))
            {
                Vector3 normal = wallHit.normal;
                if (wallHit.normal == Vector3.zero)
                {
                    // If there's no available wallHit, use normal of geometry intersected by LineCasts instead
                    if (cWHit.normal == Vector3.zero)
                    {
                        if (cCWHit.normal == Vector3.zero)
                        {
                            Debug.LogError("No available geometry normal from near clip plane LineCasts. Something must be amuck.", this);
                        }
                        else
                        {
                            normal = cCWHit.normal;
                        }
                    }
                    else
                    {
                        normal = cWHit.normal;
                    }
                }

                toTarget += (compensationOffset * normal);
                camera.transform.position += toTarget;

                // Recalculate positions of near clip plane
                viewFrustum = DebugDraw.CalculateViewFrustum(camera, ref nearClipDimensions);
            }
        }

        camera.transform.position = camPosCache;
        viewFrustum = DebugDraw.CalculateViewFrustum(camera, ref nearClipDimensions);
    }

    /// <summary>
    /// Reset local position of camera inside of parentRig and resets character's look IK.
    /// </summary>
    private void ResetCamera()
    {
        lookWeight = Mathf.Lerp(lookWeight, 0.0f, Time.deltaTime * firstPersonLookSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
    }

    #endregion Methods
}
