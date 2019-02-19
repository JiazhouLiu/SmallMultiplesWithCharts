using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using VRTK;
public enum DateType{
	Weekly,     //6: 9sec; 12: 12sec; 26: 22sec; 30: 28sec; 32: 26sec; 34: 27sec ; 35: 28sec
	Fornightly, //6: 9sec; 12: 12sec; 26: 22sec
    Monthly     //6: 9sec; 12: 12sec
}
	
public class SmallMultiplesManagerScript : MonoBehaviour {

    // prefabs
    public GameObject DataPrefab;
	public GameObject frontPillarPrefab;
	public GameObject BpillarPrefab;
	public GameObject pillarIOPrefab;
	public GameObject shelfBoardPrefab;
    public GameObject shelfBoardPiecePrefab;
	public GameObject centroidPrefab;
	public GameObject curveRendererPrefab;
    public GameObject controlBallPrefab;
    public GameObject fakeControlBallPrefab;
    public GameObject tooltipPrefab;
	public GameObject colorSchemePrefab;
    public Shader silhouetteShader;
    public Light haloLightPrefab;
    public TextAsset ACFile;

    [Header("Variables")]
	public bool swipeToRotate = false;
    public bool faceToCurve = true;
    public bool indirectTouch = false;
    public bool indirectRayCast = false;
    //public bool realFocusPoint = false;

    public float gain = 3.0f;

    [HideInInspector]
    // centerZDelta for curve
    public float uniqueCenterZDelta = 0;

    [Header("Dataset")]
    public int dataset = 1;
    public int smallMultiplesNumber;
    public DateType dateType;

    // sensors Info List to store sensor information
    public static Dictionary<string, HashSet<SensorReading>> sensorsInfoList;

    // Small multiples game object list
	List<GameObject> dataSM;
    // static string layout = "";

    List<GameObject> touchableObjects;
    List<GameObject> grabbedObjects;
    List<Vector3> grabbedObjectOldPosition;

    // shelf variables
    GameObject shelf;
    GameObject roofBoard;
	List<GameObject> shelfBoards;
	List<GameObject> shelfPillars;
	List<GameObject> curveRenderers;
    List<GameObject> curveBoards; 
    List<GameObject> pillarMiddleIOs; 
	List<GameObject> pillarTopIOs;
    GameObject centroidGO;
	GameObject controllBall;
	GameObject colorScheme;

    // controller
    GameObject leftController;
    GameObject rightController;

	int smLimit = 0;
    int shelfRows = 0;
    int tmpShelfRows = 0;
	int shelfItemPerRow = 0;
    float delta = 0.65f;
	Vector3 oldLeftPosition;
	Vector3 oldRightPosition;

    // fix grabbing issue
    Vector3 oldControlBallPosition;
    Vector3 oldLeftMiddleIOPosition;
    Vector3 oldRightMiddleIOPosition;
    int oldRowNo = 1;

    float currentY;
    float baseY; // store base y value

    //float originalSMZ; // keep sm default z
    Vector3 currentPillarCenter;

    // pillar Interactive object variables
    bool leftMiddleIOGrabbed = false;
    bool rightMiddleIOGrabbed = false;
    bool leftTopIOGrabbed = false;
    bool rightTopIOGrabbed = false;
    bool bothMiddleGrabbed = false;
    bool controlBallGrabbed = false;

    //bool rotationReset = false;

    float lastIODistance = 0;
	float currentVerticalDiff = 0;

    // curve variables
    float currentCurvature;
	float currentCurveRendererZ; // keep renderer object z value stable

    float currentZDistance;
    float currentBoardPositionZ;

    float curvatureDelta;

    // offset variables
    //float curvedOffset = 0.2f;
    //float linearCurvedOffsetx = 0.6f;
    //float linearCurvedOffsetz = 0.2f;
    Vector3 shelfPositionoffset;
	float boardPositionZDelta = 0f; // 0.005f
    float curveScaleZDelta = 0f; // 0.01f
    float curveRendererZDelta = 0.0025f;

    // string variables
    private char lineSeperater = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate chracter

    string[] tempTagList;
    bool finishAssignTag = false;

    bool bothGripPressed = false;
    //bool bothMenuPressed = false;

    //magnify color scheme and hide control ball
    bool CBHide = false;
    //bool CSMagnified = false;

	// magnify building variable
	Transform[] leftControllerMagnify;
	Transform[] rightControllerMagnify;


    //float curveFlagFloat = 0;
    bool canPush = true;
    bool canPull = true;

    bool controlBallRepositionSwitch = false;

    // Use this for initialization
    void Start () {
        
        if (dataSM == null) {

            if (dataset == 1)
            {
                if (dateType == DateType.Monthly)
                {
                    smLimit = 12;
                }
                else if (dateType == DateType.Fornightly)
                {
                    smLimit = 26;
                }
                else
                {
                    smLimit = 52;
                }
            }
            else {
                smLimit = 12;
            }

			if (smallMultiplesNumber < 1)
			{
				Debug.Log("Please enter a valid small multiples number");
			}
			else if (smallMultiplesNumber > smLimit)
			{
				Debug.Log("More than " +  smLimit + " small multiples are not allowed in this simulation.");
			}
			else
			{
				sensorsInfoList = new Dictionary<string, HashSet<SensorReading>>();
				dataSM = new List<GameObject>();
				shelfBoards = new List<GameObject>();
				shelfPillars = new List<GameObject>();
				curveRenderers = new List<GameObject>();
				curveBoards = new List<GameObject>();
				pillarMiddleIOs = new List<GameObject>();
				pillarTopIOs = new List<GameObject>();

				touchableObjects = new List<GameObject>();
                grabbedObjects = new List<GameObject>();
                grabbedObjectOldPosition = new List<Vector3>();

                tempTagList = new string[smallMultiplesNumber];

				leftControllerMagnify = new Transform[1];
				rightControllerMagnify = new Transform[1];

                shelf = new GameObject("Shelf");
				shelf.transform.SetParent(this.transform);
				shelf.transform.localPosition = Vector3.zero;

                // calculate curvature delta

                curvatureDelta = 1f;


                if (dataset == 1)
                {
                    // read ac file
                    ReadACFile();
                    GameObject.Find("BarChartManagement").SetActive(false);
                }
                else {
                    GameObject barChartManager = GameObject.Find("BarChartManagement");
                    barChartManager.SetActive(true);
                    BarChartCreator bcc = barChartManager.GetComponent<BarChartCreator>();
                    bcc.datasets.Clear();
                    for (int i = 1; i <= smallMultiplesNumber; i++) {
                        TextAsset file = (TextAsset) Resources.Load("bData" + i);
                        bcc.datasets.Add(file);
                    }
                }
				

				CreateShelf();
				currentZDistance = delta;
				currentBoardPositionZ = 0;
				currentPillarCenter = Vector3.Lerp(shelfPillars[0].transform.position, shelfPillars[1].transform.position, 0.5f);


				//originalSMZ = shelfPillars[0].transform.localPosition.z + (delta / 2);

				// create building small multiples
				CreateSM();
            }
		}
    }

	void Update(){
		if (smallMultiplesNumber <= smLimit && smallMultiplesNumber >= 1)
        {
            //SwitchControlMode();
            FindCenter();
            if (dataset == 1) {
                CheckBuildingMagnify();
            }
            CheckGrabbed();
			FollowBall ();
            UpdatePillar();
            UpdateBoards();
            UpdateSM();

            if (dataset == 1) {
                ZoomFloor();
                FunctionToggle();
            }
            FixGrabbing();
            HideCB();

            //Debug.Log(uniqueCenterZDelta);
        }
    }

    void SwitchControlMode() {
        leftController = GameObject.Find("LeftController");
        rightController = GameObject.Find("RightController");
        if (leftController != null && rightController != null)
        {
            VRTK_ControllerEvents lce = leftController.GetComponent<VRTK_ControllerEvents>();
            VRTK_ControllerEvents rce = rightController.GetComponent<VRTK_ControllerEvents>();

            if (lce.gripPressed && rce.gripPressed)
            {
                if (!bothGripPressed)
                {
                    if (indirectTouch)
                    {
                        indirectTouch = false;
                    }
                    else
                    {
                        indirectTouch = true;
                    }
                    bothGripPressed = true;
                }
            }
            else
            {
                bothGripPressed = false;
            }
            //                if (lce.buttonTwoPressed && rce.buttonTwoPressed)
            //                {
            //                    if (!bothMenuPressed)
            //                    {
            //                        if (indirectTouch)
            //                        {
            //                            if (indirectRayCast)
            //                            {
            //                                indirectRayCast = false;
            //                            }
            //                            else
            //                            {
            //                                indirectRayCast = true;
            //                            }
            //                        }
            //                        bothMenuPressed = true;
            //                    }
            //                    else {
            //                        bothMenuPressed = false;
            //                    }
            //                }
        }
    }

    public void SetUniqueCenterZDelta(float centerZDelta) {
        this.uniqueCenterZDelta = centerZDelta;
    }


    void CheckBuildingMagnify(){
		foreach (GameObject go in dataSM) {
			BuildingScript bs = go.transform.GetChild (0).gameObject.GetComponent<BuildingScript> ();
			if (bs.IsExploded())
			{
				indirectRayCast = true;
                MagnifyBuilding(go.transform.GetChild(0));
                if (!bs.IsMagnified()) {
					go.transform.localScale = Vector3.one;
				}
				//Debug.Log(Vector3.Distance(Camera.main.transform.position, this.transform.position));
				
			}
			else {
				indirectRayCast = false;
				go.transform.localScale = Vector3.one;
			}
		}
        //Debug.Log(curveFlagFloat);
        //// check curved or not
        //if (curveFlagFloat != 0)
        //{
        //    roofBoard.SetActive(false);
        //}
        //else {
        //    roofBoard.SetActive(true);
        //}
        if (canPull)
        {
            roofBoard.SetActive(false);
        }
        else
        {
            roofBoard.SetActive(true);
        }

    }

	void MagnifyBuilding(Transform building)
	{
		GameObject leftController = GameObject.Find("Controller (left)");
		GameObject rightController = GameObject.Find("Controller (right)");
        if (leftController != null && rightController != null) {
            SteamVR_TrackedController lstc = leftController.GetComponent<SteamVR_TrackedController>();
            SteamVR_TrackedController rstc = rightController.GetComponent<SteamVR_TrackedController>();

            Vector3 leftControllerDir = leftController.transform.forward;
            Vector3 rightControllerDir = rightController.transform.forward;

            Vector3 leftControllerObjectV = building.parent.position - leftController.transform.position;
            Vector3 rightControllerObjectV = building.parent.position - rightController.transform.position;

            BuildingScript bs = building.gameObject.GetComponent<BuildingScript>();
            if (!lstc.gripped && !rstc.gripped)
            {
                if (Vector3.Angle(leftControllerDir, leftControllerObjectV) < 5)
                {
                    if (!bs.IsMagnified())
                    {
                        if (leftControllerMagnify[0] == null)
                        {
                            leftControllerMagnify[0] = building;
                            building.parent.localScale = new Vector3(building.parent.localScale.x * 2, building.parent.localScale.y * 2, building.parent.localScale.z * 2);
                            bs.SetMagnify(true);
                        }

                    }
                }
                else
                {
                    if (bs.IsMagnified())
                    {
                        if (leftControllerMagnify[0] == building)
                        {
                            leftControllerMagnify = new Transform[1];
                            building.parent.localScale = new Vector3(building.parent.localScale.x / 2, building.parent.localScale.y / 2, building.parent.localScale.z / 2);
                            bs.SetMagnify(false);
                        }
                        else
                        {
                            //Debug.Log ("BUGBUG");
                        }

                    }
                }

                if (Vector3.Angle(rightControllerDir, rightControllerObjectV) < 10)
                {
                    if (!bs.IsMagnified())
                    {
                        if (rightControllerMagnify[0] == null)
                        {
                            rightControllerMagnify[0] = building;
                            building.parent.localScale = new Vector3(building.parent.localScale.x * 2, building.parent.localScale.y * 2, building.parent.localScale.z * 2);
                            bs.SetMagnify(true);
                        }
                    }
                }
                else
                {
                    if (bs.IsMagnified())
                    {
                        if (rightControllerMagnify[0] == building)
                        {
                            rightControllerMagnify = new Transform[1];
                            building.parent.localScale = new Vector3(building.parent.localScale.x / 2, building.parent.localScale.y / 2, building.parent.localScale.z / 2);
                            bs.SetMagnify(false);
                        }
                        else
                        {
                            //Debug.Log("BUGBUG");
                        }
                    }
                }
            }
                
        }
		
			
	}

    void HideCB() {
        GameObject leftController = GameObject.Find("Controller (left)");
        GameObject rightController = GameObject.Find("Controller (right)");
		if (leftController != null && rightController != null) {
			Vector3 leftControllerDir = leftController.transform.forward;
			Vector3 rightControllerDir = rightController.transform.forward;

			Vector3 leftControllerCBV = controllBall.transform.position - leftController.transform.position;
			Vector3 rightControllerCBV= controllBall.transform.position - rightController.transform.position;

			//Vector3 leftControllerCSV = colorScheme.transform.position - leftController.transform.position;
			//Vector3 rightControllerCSV = colorScheme.transform.position - rightController.transform.position;



//			if (Vector3.Angle(leftControllerDir, leftControllerCSV) < 5 || Vector3.Angle(rightControllerDir, rightControllerCSV) < 5)
//			{
//				if (!CSMagnified)
//				{
//					colorScheme.transform.localScale = new Vector3(colorScheme.transform.localScale.x * 2, colorScheme.transform.localScale.y * 2, colorScheme.transform.localScale.z * 2);
//					CSMagnified = true;
//				}
//			}
//			else
//			{
//				if (CSMagnified)
//				{
//					colorScheme.transform.localScale = new Vector3(colorScheme.transform.localScale.x / 2, colorScheme.transform.localScale.y / 2, colorScheme.transform.localScale.z / 2);
//					CSMagnified = false;
//				}
//			}
			float angleDelta = 10;

            if (dataset == 1) {
                BuildingScript bs = dataSM[0].transform.GetChild(0).GetComponent<BuildingScript>();
                if (bs.IsExploded())
                {
                    angleDelta = 3;
                }
                else
                {
                    angleDelta = 10;
                }
            }
			

			VRTK_InteractableObject controllBallIO = controllBall.GetComponent<VRTK_InteractableObject>();
			if (Vector3.Angle(leftControllerDir, leftControllerCBV) < angleDelta || Vector3.Angle(rightControllerDir, rightControllerCBV) < angleDelta)
			{
				if (CBHide)
				{        
					touchableObjects.Add(controllBall);
                    Color color = controllBall.GetComponent<MeshRenderer>().material.color;
                    color.a = 1;
                    controllBall.GetComponent<MeshRenderer>().material.color = color;
                    //controllBall.SetActive(true);
					CBHide = false;
				}
			}
			else
			{
				if (!CBHide && !controllBallIO.IsGrabbed())
				{
					touchableObjects.Remove(controllBall);
                    //Color color = controllBall.GetComponent<MeshRenderer>().material.color;
                    //color.a = 0;
                    //controllBall.GetComponent<MeshRenderer>().material.color = color;
                    //controllBall.SetActive(false);
                    CBHide = true;
				}
                Color color = controllBall.GetComponent<MeshRenderer>().material.color;
                color.a = 0.1f;
                controllBall.GetComponent<MeshRenderer>().material.color = color;
            }
		
		}
        
    }

	void FunctionToggle(){
		if (indirectTouch)
		{
			IndirectSelection();
		}
		else {
			ChangeToDirectSelection();
		}
		// check row number changed
		if (shelfRows != oldRowNo) {
            
            faceToCurve = true;
			ToggleFaceCurve();
		}
		oldRowNo = shelfRows;

		// check if assign temp tag
		if (!finishAssignTag && dateType != DateType.Monthly) {
			if (tempTagList != null && !tempTagList[0].Equals(""))
			{
				for (int i = 0; i < smallMultiplesNumber; i++)
				{
					string tooltipText = tempTagList[i];

					Transform tooltip = dataSM[i].transform.GetChild(1);
					VRTK_ObjectTooltip ot = tooltip.GetComponent<VRTK_ObjectTooltip>();
					ot.displayText = tooltipText;
				}
				finishAssignTag = true;
			}
		}


		if (Input.GetKeyDown(KeyCode.Space))
		{
			PushShelf ();
		}
	}

    void GrabGain() {
        if (grabbedObjects.Count > 0 && grabbedObjectOldPosition.Count == grabbedObjects.Count)
        {
            

            for (int i = 0; i < grabbedObjects.Count; i++) {
                Debug.Log("Yes, " + grabbedObjects[i] + " " + grabbedObjectOldPosition[i]);
                Vector3 velocity = grabbedObjects[i].transform.position - grabbedObjectOldPosition[i];
                grabbedObjects[i].transform.position += 5 * velocity;
            }
        }
    }

    void RecordOldGrabPosition() {

        grabbedObjectOldPosition.Clear();
        if (grabbedObjects.Count > 0) {
            foreach (GameObject go in grabbedObjects)
            {
                grabbedObjectOldPosition.Add(go.transform.position);
            }
        }
        
    }

    public void AssignTempTag( string[] tempTagList) {
        this.tempTagList = tempTagList;
    }

    /// <summary>
    /// IndirectSelection method to enable controllers to touch indirectly
    /// </summary>
    void IndirectSelection() {
        leftController = GameObject.Find("LeftController");
        rightController = GameObject.Find("RightController");

        if (leftController != null && rightController != null) {
            if (indirectRayCast)
            {
                leftController.GetComponent<VRTK_InteractUse>().enabled = false;
                leftController.GetComponent<VRTK_Pointer>().enabled = true;
                leftController.GetComponent<VRTK_StraightPointerRenderer>().enabled = true;

                VRTK_Pointer leftPointer = leftController.GetComponent<VRTK_Pointer>();
                leftPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                leftPointer.activateOnEnable = true;
                leftPointer.holdButtonToActivate = false;
                leftPointer.selectOnPress = false;
                leftPointer.interactWithObjects = false;
                leftPointer.grabToPointerTip = false;
                VRTK_StraightPointerRenderer leftPRenderer = leftController.GetComponent<VRTK_StraightPointerRenderer>();
                leftPRenderer.cursorScaleMultiplier = 1;


                rightController.GetComponent<VRTK_InteractUse>().enabled = false;
                rightController.GetComponent<VRTK_Pointer>().enabled = true;
                rightController.GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                VRTK_Pointer rightPointer = rightController.GetComponent<VRTK_Pointer>();
                rightPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.Undefined;
                rightPointer.activateOnEnable = true;
                rightPointer.holdButtonToActivate = false;
                rightPointer.selectOnPress = false;
                rightPointer.interactWithObjects = false;
                rightPointer.grabToPointerTip = false;
                VRTK_StraightPointerRenderer rightPRenderer = rightController.GetComponent<VRTK_StraightPointerRenderer>();
                rightPRenderer.cursorScaleMultiplier = 1;
            }
            else {
                leftController.GetComponent<VRTK_InteractUse>().enabled = false;
                leftController.GetComponent<VRTK_Pointer>().enabled = false;
                leftController.GetComponent<VRTK_StraightPointerRenderer>().enabled = false;

                rightController.GetComponent<VRTK_InteractUse>().enabled = false;
                rightController.GetComponent<VRTK_Pointer>().enabled = false;
                rightController.GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
            }
            Transform tooltipCanvas = leftController.transform.GetChild(0).GetChild(0).Find("TooltipCanvas");

            Text frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
            Text backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
            frontText.text = "Interact with touch point";
            backText.text = "Interact with touch point";

            tooltipCanvas = rightController.transform.GetChild(0).GetChild(0).Find("TooltipCanvas");

            frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
            backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
            frontText.text = "Interact with touch point";
            backText.text = "Interact with touch point";
        }
    }

    void ChangeToDirectSelection()
    {
        leftController = GameObject.Find("LeftController");
        rightController = GameObject.Find("RightController");

        if (leftController != null && rightController != null)
        {
            if (!indirectRayCast)
            {
                leftController.GetComponent<VRTK_Pointer>().enabled = true;
                leftController.GetComponent<VRTK_StraightPointerRenderer>().enabled = true;

                rightController.GetComponent<VRTK_Pointer>().enabled = true;
                rightController.GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
            }

            indirectRayCast = false;

            leftController.GetComponent<VRTK_InteractUse>().enabled = true;
            VRTK_Pointer leftPointer = leftController.GetComponent<VRTK_Pointer>();
            leftPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerTouch;
            leftPointer.activateOnEnable = false;
            leftPointer.holdButtonToActivate = true;
            leftPointer.selectOnPress = true;
            leftPointer.interactWithObjects = true;
            leftPointer.grabToPointerTip = true;
            VRTK_StraightPointerRenderer leftPRenderer = leftController.GetComponent<VRTK_StraightPointerRenderer>();
            leftPRenderer.cursorScaleMultiplier = 25;


            rightController.GetComponent<VRTK_InteractUse>().enabled = true;
            VRTK_Pointer rightPointer = rightController.GetComponent<VRTK_Pointer>();
            rightPointer.activationButton = VRTK_ControllerEvents.ButtonAlias.TriggerTouch;
            rightPointer.activateOnEnable = false;
            rightPointer.holdButtonToActivate = true;
            rightPointer.selectOnPress = true;
            rightPointer.interactWithObjects = true;
            rightPointer.grabToPointerTip = true;
            VRTK_StraightPointerRenderer rightPRenderer = rightController.GetComponent<VRTK_StraightPointerRenderer>();
            rightPRenderer.cursorScaleMultiplier = 25;

            

            Transform tooltipCanvas = leftController.transform.GetChild(0).GetChild(0).Find("TooltipCanvas");

            Text frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
            Text backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
            frontText.text = "Interact with pillars";
            backText.text = "Interact with pillars";

            tooltipCanvas = rightController.transform.GetChild(0).GetChild(0).Find("TooltipCanvas");

            frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
            backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
            frontText.text = "Interact with pillars";
            backText.text = "Interact with pillars";
        }
    }

    public GameObject CalculateNearestTouchPoint(Transform controllerT) {
        Vector3 controllerDir = controllerT.forward;
        float smallestAngle = 180;
        GameObject nearestObject = touchableObjects[0];
        
        foreach (GameObject touchableobject in touchableObjects) {
            Vector3 controllerObjectV = touchableobject.transform.position - controllerT.position;

            if (Vector3.Angle(controllerDir, controllerObjectV) < smallestAngle) {
                nearestObject = touchableobject;
                smallestAngle = Vector3.Angle(controllerDir, controllerObjectV);
            }
        }

        foreach (GameObject go in touchableObjects) {
            VRTK_InteractableObject vio = go.GetComponent<VRTK_InteractableObject>();
            if (vio.IsGrabbed() && vio.GetGrabbingObject() == controllerT.gameObject) {
                return null;
            }
        }

        return nearestObject;

    }

    void ToggleLight(GameObject go, bool lightOn) {
        GameObject haloLight = go.transform.Find("Halo Light").gameObject;
        if (lightOn)
        {
            haloLight.SetActive(true);
            Color grabColor = go.GetComponent<Renderer>().material.color;
            haloLight.GetComponent<Light>().color = grabColor;
        }
        else {
            haloLight.SetActive(false);
        }
    }


    // update functions

    void CheckGrabbed(){

        // check middle IO 
        VRTK_InteractableObject leftIO = pillarMiddleIOs[0].GetComponent<VRTK_InteractableObject>();
        VRTK_InteractableObject rightIO = pillarMiddleIOs[1].GetComponent<VRTK_InteractableObject>();

        if (leftIO.IsGrabbed())
        {
            leftMiddleIOGrabbed = true;
            grabbedObjects.Add(pillarMiddleIOs[0]);
            faceToCurve = true;
            ToggleFaceCurve();
            
            ToggleLight(pillarMiddleIOs[0], true);
        }
        else
        {
            if (leftMiddleIOGrabbed) {
                grabbedObjects.Remove(pillarMiddleIOs[0]);
                faceToCurve = true;
                ToggleFaceCurve();
                
                ToggleLight(pillarMiddleIOs[0], false);
            }
            leftMiddleIOGrabbed = false;
        }
        if (rightIO.IsGrabbed())
        {
            rightMiddleIOGrabbed = true;
            grabbedObjects.Add(pillarMiddleIOs[1]);
            faceToCurve = true;
            ToggleFaceCurve();
            
            ToggleLight(pillarMiddleIOs[1], true);
        }
        else
        {
            if (rightMiddleIOGrabbed)
            {
                grabbedObjects.Remove(pillarMiddleIOs[1]);
                faceToCurve = true;
                ToggleFaceCurve();
                
                ToggleLight(pillarMiddleIOs[1], false);
            }
            rightMiddleIOGrabbed = false;
        }

        // check if top IO can be grabbed
        VRTK_InteractableObject leftTopIO = pillarTopIOs[0].GetComponent<VRTK_InteractableObject>();
        VRTK_InteractableObject rightTopIO = pillarTopIOs[1].GetComponent<VRTK_InteractableObject>();

        
        
        if (leftTopIO.IsGrabbed())
        {
            leftTopIOGrabbed = true;
            grabbedObjects.Add(pillarTopIOs[0]);
            ToggleLight(pillarTopIOs[0], true);
        }
        else
        {
            if (leftTopIOGrabbed) {
                grabbedObjects.Remove(pillarTopIOs[0]);
                ToggleLight(pillarTopIOs[0], false);
            }
            leftTopIOGrabbed = false;
        }
        if (rightTopIO.IsGrabbed())
        {
            rightTopIOGrabbed = true;
            grabbedObjects.Add(pillarTopIOs[1]);
            ToggleLight(pillarTopIOs[1], true);
        }
        else
        {
            if (rightTopIOGrabbed) {
                grabbedObjects.Remove(pillarTopIOs[1]);
                ToggleLight(pillarTopIOs[1], false);
            }
            rightTopIOGrabbed = false;
        }

        if (dataset == 1)
        {
            BuildingScript bs = dataSM[0].transform.GetChild(0).GetComponent<BuildingScript>();

            if (bs.IsExploded())
            {
                // add top pillar to touchable list

                if (!touchableObjects.Contains(pillarTopIOs[0]))
                {
                    touchableObjects.Add(pillarTopIOs[0]);
                }
                if (!touchableObjects.Contains(pillarTopIOs[1]))
                {
                    touchableObjects.Add(pillarTopIOs[1]);
                }


                leftTopIO.isGrabbable = true;
                rightTopIO.isGrabbable = true;

                //colorScheme.SetActive(true);

                leftController = GameObject.Find("LeftController");
                // get left controller and find grip tooltip
                if (leftController != null)
                {
                    Transform tooltipCanvas = leftController.transform.GetChild(0).GetChild(2).Find("TooltipCanvas");

                    Text frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
                    Text backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
                    frontText.text = "Switch explosion level";
                    backText.text = "Switch explosion level";
                }

                foreach (GameObject pIO in pillarTopIOs)
                {

                    Transform objectTT = pIO.transform.GetChild(0);
                    objectTT.gameObject.SetActive(true);

                    Transform tooltipCanvas = objectTT.Find("TooltipCanvas");

                    RectTransform rt = tooltipCanvas.GetComponent<RectTransform>();
                    RectTransform rtContainer = tooltipCanvas.Find("UIContainer").GetComponent<RectTransform>();

                    rt.sizeDelta = new Vector2(170, 30);
                    rtContainer.sizeDelta = new Vector2(170, 30);

                    if (pIO.name.Equals("Left Pillar Top IO"))
                    {
                        pIO.transform.GetChild(0).Find("LineStart").localPosition = new Vector3(-0.085f, 0, 0);
                    }
                    else if (pIO.name.Equals("Right Pillar Top IO"))
                    {
                        pIO.transform.GetChild(0).Find("LineStart").localPosition = new Vector3(0.085f, 0, 0);
                    }

                    Text frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
                    Text backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
                    frontText.text = "Increase Floor Height";
                    backText.text = "Increase Floor Height";
                }
            }
            else
            {
                VRTK_InteractableObject leftTopPillarIO = pillarTopIOs[0].GetComponent<VRTK_InteractableObject>();
                VRTK_InteractableObject rightTopPillarIO = pillarTopIOs[1].GetComponent<VRTK_InteractableObject>();

                leftTopPillarIO.ForceStopInteracting();
                rightTopPillarIO.ForceStopInteracting();
                // remove top pillar ios from list
                // add top pillar to touchable list
                if (touchableObjects.Contains(pillarTopIOs[0]))
                {
                    touchableObjects.Remove(pillarTopIOs[0]);
                }
                if (touchableObjects.Contains(pillarTopIOs[1]))
                {
                    touchableObjects.Remove(pillarTopIOs[1]);
                }


                leftTopIO.isGrabbable = false;
                rightTopIO.isGrabbable = false;


                // get left controller and find grip tooltip
                if (leftController != null)
                {
                    Transform tooltipCanvas = leftController.transform.GetChild(0).GetChild(2).Find("TooltipCanvas");

                    Text frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
                    Text backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
                    frontText.text = "Switch explosion level";
                    backText.text = "Switch explosion level";
                }

                foreach (GameObject pIO in pillarTopIOs)
                {
                    Transform objectTT = pIO.transform.GetChild(0);
                    objectTT.gameObject.SetActive(false);
                }

                //colorScheme.SetActive(false);
                //reset vertical difference
                currentVerticalDiff = 0;
            }
        }

        // indirect touch check control ball touched
        //bool cbLeftTouched = false;
        //bool cbRightTouched = false;

        //leftController = GameObject.Find("LeftController");
        //rightController = GameObject.Find("RightController");

        //if (leftController != null) {
        //    SteamVR_TrackedController ltc = leftController.transform.parent.GetComponent<SteamVR_TrackedController>();
        //    cbLeftTouched = ltc.controlBallTouched;
        //}
        //if (rightController != null) {
        //    SteamVR_TrackedController rtc = rightController.transform.parent.GetComponent<SteamVR_TrackedController>();
        //    cbRightTouched = rtc.controlBallTouched;
        //}

        //VRTK_InteractableObject controllBallIO = controllBall.GetComponent<VRTK_InteractableObject>();
        
        //if (cbLeftTouched || cbRightTouched || controllBallIO.IsGrabbed())
        //{
        //    controllBall.SetActive(true);
        //}
        //else {
        //    controllBall.SetActive(false);
        //}
        
    }

    public void ToggleFaceCurve() {
        
        if (faceToCurve)
        {
            foreach (GameObject board in shelfBoards) {
                Bezier3PointCurve bpc = board.transform.GetChild(0).gameObject.GetComponent<Bezier3PointCurve>();
                bpc.FaceToCurve();
            }
            
            //rotationReset = false;
            faceToCurve = false;
        }
        else {
            //if (!rotationReset) {
                foreach (GameObject sm in dataSM)
                {
                    sm.transform.localRotation = Quaternion.identity;
                }

            foreach (GameObject board in shelfBoards)
            {
                Bezier3PointCurve bpc = board.transform.GetChild(0).gameObject.GetComponent<Bezier3PointCurve>();
                bpc.BoardPieceToNormal();
            }
            // rotationReset = true;
            //}
            faceToCurve = true;
        }
    }

    void FollowBall(){
        VRTK_InteractableObject controllBallIO = controllBall.GetComponent<VRTK_InteractableObject>();

        if (controllBallIO.IsGrabbed())
        {
            if (!controlBallGrabbed) {
                Debug.Log("offset: " + shelfPositionoffset);
                shelf.transform.position += shelfPositionoffset;
                //Debug.Log("pressed: " + controllBall.transform.position);
            }
            
            controlBallGrabbed = true;
            grabbedObjects.Add(controllBall);
            ToggleLight(controllBall, true);
            // fix grabbing
            if (Vector3.Distance(oldControlBallPosition, controllBall.transform.position) > 2)
            {
                Debug.Log("Bug!!!");
                //controllBall.transform.position = oldControlBallPosition;
                controllBallIO.ForceStopInteracting();
            }
            else
            {
                foreach (GameObject sm in dataSM) {
                    PositionLocalConstraints plc = sm.GetComponent<PositionLocalConstraints>();
                    plc.UpdateZ (sm.transform.localPosition.z);
                    plc.z = true;
                }

                
                


                if (!indirectTouch)
                {
                    if (leftController != null)
                    {
                        GameObject lbugObj = GameObject.Find("[VRTK][AUTOGEN][LeftController][StraightPointerRenderer_Container]");
                        if (lbugObj.transform.GetChild(1).gameObject.activeSelf)
                        {
                            lbugObj.transform.GetChild(1).position -= Vector3.forward * 2;
                        }
                    }
                    if (rightController != null)
                    {
                        GameObject rbugObj = GameObject.Find("[VRTK][AUTOGEN][RightController][StraightPointerRenderer_Container]");
                        if (rbugObj.transform.GetChild(1).gameObject.activeSelf)
                        {
                            rbugObj.transform.GetChild(1).position -= Vector3.forward * 2;
                        }
                    }

                }
                MoveShelfToCenter();
                shelf.transform.position = controllBall.transform.position;
                


                foreach (GameObject sm in dataSM)
                {
                    PositionLocalConstraints plc = sm.GetComponent<PositionLocalConstraints>();
                    plc.UpdateZ(sm.transform.localPosition.z);
                    plc.z = false;
                }

                GameObject viveCamera = GameObject.Find("Camera (eye)");
                Vector3 camPos = viveCamera.transform.position;
                Vector3 finalPos = new Vector3(camPos.x, shelf.transform.position.y, camPos.z);
                Vector3 offset = shelf.transform.position - finalPos;
                shelf.transform.LookAt(shelf.transform.position + offset);



            }
        }
        else
        {
            if (controlBallGrabbed)
            {
                controlBallRepositionSwitch = true;
                shelf.transform.position = controllBall.transform.position;
                //Debug.Log("released: " + controllBall.transform.position);
                //Debug.Log("offset: " + shelfPositionoffset);
                grabbedObjects.Remove(controllBall);
                //float zDiff = shelf.transform.localPosition.z - controllBall.transform.localPosition.z;
                //Debug.Log("zDiff: " + zDiff);
                //shelf.transform.position += shelfPositionoffset;
                ToggleLight(controllBall, false);
            }
            controlBallGrabbed = false;


            foreach (GameObject sm in dataSM)
            {
                if (dataset == 1) {
                    PositionLocalConstraints plc = sm.GetComponent<PositionLocalConstraints>();
                    plc.z = false;
                }
                
            }

            controllBall.transform.position = centroidGO.transform.position;

            
            

            if (shelf.transform.localPosition != Vector3.zero)
            {
                shelfPositionoffset = shelf.transform.position - controllBall.transform.position;
            }
            else
            {
                shelfPositionoffset = Vector3.zero;
            }

        }
        oldControlBallPosition = controllBall.transform.position;

        Vector3 diff = shelf.transform.localPosition - controllBall.transform.localPosition;
        if (controlBallRepositionSwitch) {
            shelf.transform.localPosition += diff;
            controlBallRepositionSwitch = false;
        }
    }

    void UpdatePillar()
    {
        currentY = baseY + currentVerticalDiff / 2;

        VRTK_InteractableObject leftIO = pillarMiddleIOs[0].GetComponent<VRTK_InteractableObject>();
        VRTK_InteractableObject rightIO = pillarMiddleIOs[1].GetComponent<VRTK_InteractableObject>();

        if (leftMiddleIOGrabbed && rightMiddleIOGrabbed)
        {

            //if (Vector3.Distance(pillarMiddleIOs[0].transform.position, oldLeftMiddleIOPosition) < 1 && Vector3.Distance(pillarMiddleIOs[1].transform.position, oldRightMiddleIOPosition) < 1)
            //{
                bothMiddleGrabbed = true;

                // change canvas text
                foreach (GameObject pIO in pillarMiddleIOs)
                {
                    Transform tooltipCanvas = pIO.transform.GetChild(0).Find("TooltipCanvas");

                    RectTransform rt = tooltipCanvas.GetComponent<RectTransform>();
                    RectTransform rtContainer = tooltipCanvas.Find("UIContainer").GetComponent<RectTransform>();

                    rt.sizeDelta = new Vector2(150, 30);
                    rtContainer.sizeDelta = new Vector2(150, 30);

                    if (pIO.name.Equals("Left Pillar Middle IO"))
                    {
                        pIO.transform.GetChild(0).Find("LineStart").localPosition = new Vector3(-0.075f, 0, 0);
                    }
                    else if (pIO.name.Equals("Right Pillar Middle IO"))
                    {
                        pIO.transform.GetChild(0).Find("LineStart").localPosition = new Vector3(0.075f, 0, 0);
                    }

                    Text frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
                    Text backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
                    frontText.text = "Change Curvature";
                    backText.text = "Change Curvature";
                }

            // interactive change curvature
            GameObject leftController = GameObject.Find("Controller (left)");
            GameObject rightController = GameObject.Find("Controller (right)");
            if (leftController != null && rightController != null)
            {
                Vector3 leftControllerDir = leftController.transform.forward;
                Vector3 rightControllerDir = rightController.transform.forward;

                float currentAngle = Vector3.Angle(leftControllerDir, rightControllerDir);

                if (currentAngle < lastIODistance)
                {
                    PushShelf();
                }
                else {
                    PullShelf();
                }
             }       

                //float currentIODistance = Mathf.Abs(pillarMiddleIOs[0].transform.localPosition.x - pillarMiddleIOs[1].transform.localPosition.x);
                //if (currentIODistance - lastIODistance > 0.01f)
                //{
                //    PullShelf();
                //}
                //else if (lastIODistance - currentIODistance > 0.01f)
                //{
                //    PushShelf();
                //}
            //}
            //else {
            //    leftIO.ForceStopInteracting();
            //    rightIO.ForceStopInteracting();
            //}
        }
        else
        {
            // change canvas text
            foreach (GameObject pIO in pillarMiddleIOs)
            {
                Transform tooltipCanvas = pIO.transform.GetChild(0).Find("TooltipCanvas");

                RectTransform rt = tooltipCanvas.GetComponent<RectTransform>();
                RectTransform rtContainer = tooltipCanvas.Find("UIContainer").GetComponent<RectTransform>();

                rt.sizeDelta = new Vector2(100, 30);
                rtContainer.sizeDelta = new Vector2(100, 30);

                if (pIO.name.Equals("Left Pillar Middle IO"))
                {
                    pIO.transform.GetChild(0).Find("LineStart").localPosition = new Vector3(-0.05f, 0, 0);
                }
                else if (pIO.name.Equals("Right Pillar Middle IO"))
                {
                    pIO.transform.GetChild(0).Find("LineStart").localPosition = new Vector3(0.05f, 0, 0);
                }

                Text frontText = tooltipCanvas.Find("UITextFront").GetComponent<Text>();
                Text backText = tooltipCanvas.Find("UITextReverse").GetComponent<Text>();
                frontText.text = "Grab to Move";
                backText.text = "Grab to Move";
            }

            // stop interaction to stop pillar movement
            if (bothMiddleGrabbed)
            {
                bothMiddleGrabbed = false;
                leftIO.ForceStopInteracting();
                rightIO.ForceStopInteracting();
            }
            else
            {
                // move pillar
                if (leftMiddleIOGrabbed)
                {
                    if (Vector3.Distance(pillarMiddleIOs[0].transform.position, oldLeftMiddleIOPosition) < 1)
                    {
                        shelfPillars[0].transform.position = pillarMiddleIOs[0].transform.position;
                    }
                    else {
                        leftIO.ForceStopInteracting();
                    }

                    
                }
                if (rightMiddleIOGrabbed)
                {
                    if (Vector3.Distance(pillarMiddleIOs[1].transform.position, oldRightMiddleIOPosition) < 1)
                    {
                        shelfPillars[1].transform.position = pillarMiddleIOs[1].transform.position;
                    }
                    else {
                        rightIO.ForceStopInteracting();
                    }
                }
            }
        }
        //lastIODistance = Mathf.Abs(pillarMiddleIOs[0].transform.localPosition.x - pillarMiddleIOs[1].transform.localPosition.x);
        
        if (leftController != null && rightController != null)
        {
            Vector3 leftControllerDir = leftController.transform.forward;
            Vector3 rightControllerDir = rightController.transform.forward;
            lastIODistance = Vector3.Angle(leftControllerDir, rightControllerDir);
        }

        // change height of the shelf
        if (leftTopIOGrabbed)
        {
            currentVerticalDiff = pillarTopIOs[0].transform.localPosition.y - shelfBoards[0].transform.localPosition.y - 0.4f * shelfRows;
            if (dataset == 2)
            {
                currentVerticalDiff = pillarTopIOs[0].transform.localPosition.y - shelfBoards[0].transform.localPosition.y - 0.5f * shelfRows;
            }
        }
        if (rightTopIOGrabbed)
        {
            currentVerticalDiff = pillarTopIOs[1].transform.localPosition.y - shelfBoards[0].transform.localPosition.y - 0.4f * shelfRows;
            if (dataset == 2)
            {
                currentVerticalDiff = pillarTopIOs[1].transform.localPosition.y - shelfBoards[0].transform.localPosition.y - 0.5f * shelfRows;
            }
        }

        if (currentVerticalDiff >= 0)
        {
            if (currentVerticalDiff <= 1.5f)
            {
                
                shelfPillars[0].transform.localScale = new Vector3(shelfPillars[0].transform.localScale.x, 0.2f * shelfRows + currentVerticalDiff / 2, shelfPillars[0].transform.localScale.z);
                shelfPillars[1].transform.localScale = new Vector3(shelfPillars[1].transform.localScale.x, 0.2f * shelfRows + currentVerticalDiff / 2, shelfPillars[1].transform.localScale.z);

                if (dataset == 2)
                {
                    shelfPillars[0].transform.localScale = new Vector3(shelfPillars[0].transform.localScale.x, 0.25f * shelfRows + currentVerticalDiff / 2, shelfPillars[0].transform.localScale.z);
                    shelfPillars[1].transform.localScale = new Vector3(shelfPillars[1].transform.localScale.x, 0.25f * shelfRows + currentVerticalDiff / 2, shelfPillars[1].transform.localScale.z);
                }
            }
            else
            {
                currentVerticalDiff = 1.5f;
            }
        }
        else
        {
            currentVerticalDiff = 0;
        }

        Vector3 leftPillarPosition = shelfPillars[0].transform.localPosition;
        Vector3 rightPillarPosition = shelfPillars[1].transform.localPosition;
        //oldPillarY = shelfPillars [2].transform.localPosition.y;
        float distance = rightPillarPosition.x - leftPillarPosition.x;

        if (distance < delta)
        {
            if (oldLeftPosition != leftPillarPosition)
            {
                shelfPillars[0].transform.localPosition = rightPillarPosition - Vector3.right * delta;
            }
            else if (oldRightPosition != rightPillarPosition)
            {
                shelfPillars[1].transform.localPosition = leftPillarPosition + Vector3.right * delta;
            }
            else
            {
                shelfPillars[0].transform.localPosition = rightPillarPosition - Vector3.right * delta;
            }

            GameObject leftController = GameObject.Find("Controller (left)");
            if (leftController != null) {
                SteamVR_TrackedController ltc = leftController.GetComponent<SteamVR_TrackedController>();
                SteamVR_Controller.Input((int)ltc.controllerIndex).TriggerHapticPulse(500);
            }
 
            GameObject rightController = GameObject.Find("Controller (right)");
            if (rightController != null)
            {
                SteamVR_TrackedController rtc = rightController.GetComponent<SteamVR_TrackedController>();
                SteamVR_Controller.Input((int)rtc.controllerIndex).TriggerHapticPulse(500);
            }
        }
        else if (distance > smallMultiplesNumber * delta * 1.5f)
        {
            shelfPillars[0].transform.localPosition = oldLeftPosition;
            shelfPillars[1].transform.localPosition = oldRightPosition;
        }
        oldLeftPosition = shelfPillars[0].transform.localPosition;
        oldRightPosition = shelfPillars[1].transform.localPosition;

        if (pillarMiddleIOs[0].transform.position != shelfPillars[0].transform.position)
        {
            pillarMiddleIOs[0].transform.position = shelfPillars[0].transform.position;
        }
        if (pillarMiddleIOs[1].transform.position != shelfPillars[1].transform.position)
        {
            pillarMiddleIOs[1].transform.position = shelfPillars[1].transform.position;
        }


        // keep back pillars same as front pillars
        shelfPillars[2].transform.localPosition = shelfPillars[0].transform.localPosition + Vector3.forward * delta;
        shelfPillars[3].transform.localPosition = shelfPillars[1].transform.localPosition + Vector3.forward * delta;

        shelfPillars[2].transform.localScale = shelfPillars[0].transform.localScale;
        shelfPillars[3].transform.localScale = shelfPillars[1].transform.localScale;

        foreach (GameObject pillar in shelfPillars)
        {
            pillar.transform.localPosition = new Vector3(pillar.transform.localPosition.x, currentY, pillar.transform.localPosition.z);
        }
        currentPillarCenter = Vector3.Lerp(shelfPillars[0].transform.position, shelfPillars[1].transform.position, 0.5f);

        Vector3 bottomBoardPoint2 = shelfBoards[0].transform.GetChild(0).GetChild(1).position;

        colorScheme.transform.position = bottomBoardPoint2;
        colorScheme.transform.localPosition = new Vector3(colorScheme.transform.localPosition.x, shelfBoards[0].transform.localPosition.y - 0.13f, colorScheme.transform.localPosition.z - (delta / 2));

        oldLeftMiddleIOPosition = pillarMiddleIOs[0].transform.position;
        oldRightMiddleIOPosition = pillarMiddleIOs[1].transform.position;
    }

    void UpdateBoards()
    {
        GameObject leftPillar = shelfPillars[0];
        GameObject rightPillar = shelfPillars[1];

        float newPositionX = (leftPillar.transform.localPosition.x + rightPillar.transform.localPosition.x) / 2;
        float newScaleX = Mathf.Abs(rightPillar.transform.localPosition.x - leftPillar.transform.localPosition.x);
        float newScaleZ = shelfBoards[0].transform.localScale.z;
        Quaternion newRotation = shelfBoards[0].transform.localRotation;

        float division = newScaleX / delta;
        int newShelfItemPerRow = (int)division;


        if (newShelfItemPerRow > 0)
        {
            if (newShelfItemPerRow != shelfItemPerRow)
            {
                shelfItemPerRow = newShelfItemPerRow;
            }
        }
        else
        {
            if (newShelfItemPerRow != shelfItemPerRow)
            {
                shelfItemPerRow = 1;
            }
        }


        int reminder = smallMultiplesNumber % shelfItemPerRow;
        int newShelfRow;
        if (reminder != 0)
        {
            newShelfRow = smallMultiplesNumber / shelfItemPerRow + 1;
        }
        else
        {
            newShelfRow = smallMultiplesNumber / shelfItemPerRow;
        }
        bool rowChanged = false;
        while (newShelfRow != shelfRows)
        {
            if (newShelfRow > shelfRows && newShelfRow <= smallMultiplesNumber)
            {
                GameObject board;

                board = (GameObject)Instantiate(shelfBoardPrefab, new Vector3(0, 0, 0), newRotation);

                board.transform.SetParent(shelf.transform);
                board.transform.localScale = new Vector3(newScaleX, 0.003f, newScaleZ);
                board.transform.localRotation = newRotation;
                board.transform.localPosition = new Vector3(newPositionX, shelfBoards[shelfRows - 1].transform.localPosition.y + 0.4f, shelfBoards[shelfRows - 1].transform.localPosition.z);
                if (dataset == 2) {
                    board.transform.localPosition = new Vector3(newPositionX, shelfBoards[shelfRows - 1].transform.localPosition.y + 0.5f, shelfBoards[shelfRows - 1].transform.localPosition.z);
                }
                board.name = "ShelfRow " + (shelfBoards.Count + 1);
                shelfBoards.Add(board);
                shelfRows++;

                GameObject curveRenderer = (GameObject)Instantiate(curveRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                curveRenderer.transform.SetParent(board.transform);
                curveRenderer.name = "Top Curve Renderer";
                curveRenderer.transform.localPosition = new Vector3(0, 0, 0);
                curveRenderer.transform.localRotation = Quaternion.identity;
                curveRenderer.transform.localScale = new Vector3(1, 1, 1);
                curveRenderers.Add(curveRenderer);


                if (dataset == 1)
                {
                    baseY += 0.2f;

                    foreach (GameObject pillar in shelfPillars)
                    {
                        pillar.transform.localPosition += Vector3.up * 0.2f;
                        pillar.transform.localScale += Vector3.up * 0.2f;
                    }
                }else if (dataset == 2) {
                    baseY += 0.25f;

                    foreach (GameObject pillar in shelfPillars)
                    {
                        pillar.transform.localPosition += Vector3.up * 0.25f;
                        pillar.transform.localScale += Vector3.up * 0.25f;
                    }
                }
                if (dataset == 1)
                {
                    roofBoard.transform.localPosition = new Vector3(newPositionX, roofBoard.transform.localPosition.y + 0.4f, shelfBoards[shelfRows - 1].transform.localPosition.z);
                }
                if (dataset == 2) {
                    roofBoard.transform.localPosition = new Vector3(newPositionX, roofBoard.transform.localPosition.y + 0.5f, shelfBoards[shelfRows - 1].transform.localPosition.z);
                }
                roofBoard.transform.localScale = new Vector3(newScaleX, 0.003f, newScaleZ);

                rowChanged = true;
            }

            if (newShelfRow < shelfRows && newShelfRow > 0)
            {
                GameObject lastBoard = shelfBoards[shelfBoards.Count - 1];
                
                GameObject toBeDestroyedBoardPieces = GameObject.Find(lastBoard.name + " Pieces");

                
                Destroy(lastBoard);

                shelfBoards.RemoveAt(shelfBoards.Count - 1);
                shelfRows--;

                //Debug.Log(curveRenderers[shelfBoards.Count].name);
                Destroy(curveRenderers[shelfBoards.Count].GetComponent<Bezier3PointCurve>().emptyParent);

                curveRenderers.RemoveAt(shelfBoards.Count);
                if (toBeDestroyedBoardPieces != null)
                {
                    //Debug.Log(toBeDestroyedBoardPieces.name);
                    Destroy(toBeDestroyedBoardPieces);
                    toBeDestroyedBoardPieces = GameObject.Find(lastBoard.name + " Pieces");
                }

                if (dataset == 1)
                {
                    baseY -= 0.2f;

                    foreach (GameObject pillar in shelfPillars)
                    {
                        pillar.transform.localPosition -= Vector3.up * 0.2f;
                        pillar.transform.localScale -= Vector3.up * 0.2f;
                    }
                }

                if (dataset == 2)
                {
                    baseY -= 0.25f;

                    foreach (GameObject pillar in shelfPillars)
                    {
                        pillar.transform.localPosition -= Vector3.up * 0.25f;
                        pillar.transform.localScale -= Vector3.up * 0.25f;
                    }
                }
                if (dataset == 1)
                {
                    roofBoard.transform.localPosition = new Vector3(newPositionX, roofBoard.transform.localPosition.y - 0.4f, shelfBoards[shelfRows - 1].transform.localPosition.z);
                }
                if (dataset == 2)
                {
                    roofBoard.transform.localPosition = new Vector3(newPositionX, roofBoard.transform.localPosition.y - 0.5f, shelfBoards[shelfRows - 1].transform.localPosition.z);
                }
                roofBoard.transform.localScale = new Vector3(newScaleX, 0.003f, newScaleZ);

                rowChanged = true;
            }
        }

        // update pillar middle IO local y
        pillarMiddleIOs[0].transform.localPosition = new Vector3(pillarMiddleIOs[0].transform.localPosition.x, currentY, pillarMiddleIOs[0].transform.localPosition.z);
        pillarMiddleIOs[1].transform.localPosition = new Vector3(pillarMiddleIOs[1].transform.localPosition.x, currentY, pillarMiddleIOs[1].transform.localPosition.z);

        pillarTopIOs[0].transform.localPosition = new Vector3(pillarMiddleIOs[0].transform.localPosition.x, baseY + 0.2f * shelfRows + currentVerticalDiff, pillarMiddleIOs[0].transform.localPosition.z);
        pillarTopIOs[1].transform.localPosition = new Vector3(pillarMiddleIOs[1].transform.localPosition.x, baseY + 0.2f * shelfRows + currentVerticalDiff, pillarMiddleIOs[1].transform.localPosition.z);
        if (dataset == 2) {
            pillarTopIOs[0].transform.localPosition = new Vector3(pillarMiddleIOs[0].transform.localPosition.x, baseY + 0.25f * shelfRows + currentVerticalDiff, pillarMiddleIOs[0].transform.localPosition.z);
            pillarTopIOs[1].transform.localPosition = new Vector3(pillarMiddleIOs[1].transform.localPosition.x, baseY + 0.25f * shelfRows + currentVerticalDiff, pillarMiddleIOs[1].transform.localPosition.z);
        }

        for (int i = 0; i < shelfBoards.Count; i++)
        {

            shelfBoards[i].transform.localPosition = new Vector3(newPositionX, shelfBoards[0].transform.localPosition.y + (pillarTopIOs[0].transform.localPosition.y - shelfBoards[0].transform.localPosition.y) / shelfRows * i, currentBoardPositionZ);

            shelfBoards[i].transform.localScale = new Vector3(newScaleX, shelfBoards[i].transform.localScale.y, shelfBoards[i].transform.localScale.z);

            Transform renderer = shelfBoards[i].transform.GetChild(0);
            //renderer.localPosition = new Vector3(renderer.localPosition.x, renderer.localPosition.y, currentCurveRendererZ); ;
            Bezier3PointCurve bpc = renderer.GetComponent<Bezier3PointCurve>();
            if (i != 0) {
                bpc.SetCenterZDelta(uniqueCenterZDelta);
            }
            

            if (rowChanged) {
                //Debug.Log("row changed");
                
                faceToCurve = true;
                ToggleFaceCurve();
            }
            


            Transform point1 = shelfBoards[i].transform.GetChild(0).Find("Point1");
            Transform point2 = shelfBoards[i].transform.GetChild(0).Find("Point2");
            Transform point3 = shelfBoards[i].transform.GetChild(0).Find("Point3");
            // recalculate point2 position
            float middleX = (point1.localPosition.x + point3.localPosition.x) / 2;
        }


        roofBoard.transform.localPosition = new Vector3(newPositionX, pillarTopIOs[0].transform.localPosition.y, currentBoardPositionZ);
        roofBoard.transform.localScale = new Vector3(newScaleX, roofBoard.transform.localScale.y, roofBoard.transform.localScale.z);
    }

    void UpdateSM (){
		GameObject leftPillar = shelfPillars [0];
		float newLeftMostItemX = leftPillar.transform.localPosition.x + (delta / 2);
		float newTopMostItemY = shelfBoards [shelfBoards.Count - 1].transform.localPosition.y;
		int i = 0;
		for(int j = 0; j < shelfBoards.Count; j ++){
			int k = 0;
			while (k < shelfItemPerRow) {
				if (i >= smallMultiplesNumber) {
					break;
				}
                Vector3 targetPosition = new Vector3(newLeftMostItemX + (k * delta), newTopMostItemY - (j * (0.4f + currentVerticalDiff / shelfRows)), dataSM[i].transform.localPosition.z);
                //dataSM[i].transform.localPosition = Vector3.MoveTowards(dataSM[i].transform.localPosition, targetPosition, Time.deltaTime * 2);
                
                if(dataset == 1)
                {
                    dataSM[i].transform.GetChild(1).localPosition = new Vector3(0, 0.25f + currentVerticalDiff / 5, 0);
                }
                

                k++;
				i++;
			}
		}
    }

    void FindCenter()
    {
        Vector3 centroid = Vector3.zero;
        if (shelf.transform.childCount > 0)
        {
            Transform[] transforms;
            transforms = shelf.GetComponentsInChildren<Transform>();
            foreach (Transform t in transforms)
            {
                centroid += t.position;
            }
            centroid /= transforms.Length;

            Vector3 leftPillar = shelfPillars[0].transform.localPosition;
            Vector3 rightPillar = shelfPillars[1].transform.localPosition;

            GameObject pillarCenter = new GameObject();
            pillarCenter.transform.SetParent(shelf.transform);

            GameObject outerPillarCenter = new GameObject();
            outerPillarCenter.transform.SetParent(this.transform);

            pillarCenter.transform.localPosition = (leftPillar + rightPillar) / 2;
            //Debug.Log(pillarCenter.transform.localPosition + " " + leftPillar + " " + rightPillar);
            outerPillarCenter.transform.localRotation = shelf.transform.localRotation;
            outerPillarCenter.transform.position = pillarCenter.transform.position;

            
            centroidGO.transform.localRotation = shelf.transform.localRotation;
            
            centroidGO.transform.localPosition = new Vector3(outerPillarCenter.transform.localPosition.x, centroidGO.transform.localPosition.y, centroidGO.transform.localPosition.z);
            centroidGO.transform.position = centroid;
            //centroidGO.transform.localPosition = new Vector3(outerPillarCenter.transform.localPosition.x, centroidGO.transform.localPosition.y, shelf.transform.localPosition.z);
            //Debug.Log(centroidGO.transform.localPosition.x + " " + outerPillarCenter.transform.localPosition.x);
            Destroy(pillarCenter);
            Destroy(outerPillarCenter);
        }
    }

    void ZoomFloor()
    {
        foreach (GameObject building in dataSM)
        {
            Transform firstFloor = building.transform.GetChild(0).GetChild(2);
            firstFloor.localPosition = new Vector3(0, currentVerticalDiff * 5, 0);
        }
    }

    void FixGrabbing()
    {
        GameObject lbugObj = GameObject.Find("[VRTK][AUTOGEN][LeftController][StraightPointerRenderer_Container]");
        if (lbugObj != null && !lbugObj.transform.GetChild(1).gameObject.activeSelf)
        {
            lbugObj.transform.GetChild(1).position = new Vector3(-100, -100, -100);
        }
        GameObject rbugObj = GameObject.Find("[VRTK][AUTOGEN][RightController][StraightPointerRenderer_Container]");
        if (rbugObj != null && !rbugObj.transform.GetChild(1).gameObject.activeSelf)
        {
            rbugObj.transform.GetChild(1).position = new Vector3(-100, -100, -100);
        }
    }

    // general functions

    public void IncreaseRow()
    {
        if (shelfRows < smallMultiplesNumber)
        {
            int newMaxNoItemPerRow = 0;

            tmpShelfRows = shelfRows + 1;

            int tmpItemPerRow = shelfItemPerRow - 1;

            while (tmpShelfRows * tmpItemPerRow < smallMultiplesNumber) {
                tmpShelfRows++;
            }


            if (smallMultiplesNumber % tmpShelfRows != 0)
            {
                newMaxNoItemPerRow = smallMultiplesNumber / tmpShelfRows + 1;
            }
            else
            {
                newMaxNoItemPerRow = smallMultiplesNumber / tmpShelfRows;
            }
            if (newMaxNoItemPerRow > 0)
            {
                shelfPillars[0].transform.localPosition = new Vector3(-newMaxNoItemPerRow * delta / 2, shelfPillars[0].transform.localPosition.y, shelfPillars[0].transform.localPosition.z);
                shelfPillars[1].transform.localPosition = new Vector3(newMaxNoItemPerRow * delta / 2 + 0.1f, shelfPillars[1].transform.localPosition.y, shelfPillars[1].transform.localPosition.z);
            }
            //shelfRows++;
        }
    }

    public void DecreaseRow()
    {
        if (shelfRows > 1)
        {
            int newMaxNoItemPerRow = 0;
            if (smallMultiplesNumber % (shelfRows - 1) != 0)
            {
                newMaxNoItemPerRow = smallMultiplesNumber / (shelfRows - 1) + 1;
            }
            else
            {
                newMaxNoItemPerRow = smallMultiplesNumber / (shelfRows - 1);
            }
            if (newMaxNoItemPerRow > 0)
            {
                shelfPillars[0].transform.localPosition = new Vector3(-newMaxNoItemPerRow * delta / 2, shelfPillars[0].transform.localPosition.y, shelfPillars[0].transform.localPosition.z);
                shelfPillars[1].transform.localPosition = new Vector3(newMaxNoItemPerRow * delta / 2 + 0.1f, shelfPillars[1].transform.localPosition.y, shelfPillars[1].transform.localPosition.z);
            }
            //shelfRows--;
        }
    }

    //public void increasepointerlength()
    //{
    //    vrtk_straightpointerrenderer vsp = gameobject.find("leftcontroller").getcomponent<vrtk_straightpointerrenderer>();
    //    vrtk_pointer vp = gameobject.find("leftcontroller").getcomponent<vrtk_pointer>();

    //    float currentlength = vsp.getdestinationhit().distance;
    //    float newlength = currentlength + 0.5f;
    //    vsp.changebeamlength(newlength);


    //}

    //public void decreasepointerlength()
    //{
    //    vrtk_straightpointerrenderer vsp = gameobject.find("leftcontroller").getcomponent<vrtk_straightpointerrenderer>();
    //    vrtk_pointer vp = gameobject.find("leftcontroller").getcomponent<vrtk_pointer>();

    //    float currentlength = vsp.getdestinationhit().distance;
    //    float newlength = currentlength - 0.5f;
    //    vsp.changebeamlength(newlength);
    //}

    public void MoveShelfToCenter()
    {
        Vector3 centrePosition = centroidGO.transform.position;
        GameObject tmp = new GameObject();
        tmp.transform.SetParent(this.transform);
        tmp.transform.position = shelf.transform.position;
        int childrenLength = shelf.transform.childCount;
        for (int i = 0; i < childrenLength; i++)
        {
            shelf.transform.GetChild(0).SetParent(tmp.transform);
        }
        if (shelf.transform.childCount == 0) {
            shelf.transform.position = centrePosition;
        }
        
        for (int i = 0; i < childrenLength; i++)
        {
            tmp.transform.GetChild(0).SetParent(shelf.transform);
        }

        baseY = shelfBoards[0].transform.localPosition.y + 0.2f * shelfRows;

        if (dataset == 2) {
            baseY = shelfBoards[0].transform.localPosition.y + 0.25f * shelfRows;
        }

        Destroy(tmp);
    }

    public void MoveShelfToPillarCenter()
    {
        Vector3 centrePosition = currentPillarCenter;
        GameObject tmp = new GameObject();
        tmp.transform.SetParent(this.transform);
        tmp.transform.localPosition = shelf.transform.localPosition;
        int childrenLength = shelf.transform.childCount;
        for (int i = 0; i < childrenLength; i++)
        {
            shelf.transform.GetChild(0).SetParent(tmp.transform);

        }
        Vector3 tmpLocalPosition = shelf.transform.localPosition;
        shelf.transform.position = centrePosition;
        shelf.transform.localPosition = new Vector3(shelf.transform.localPosition.x, tmpLocalPosition.y, tmpLocalPosition.z);
        for (int i = 0; i < childrenLength; i++)
        {
            tmp.transform.GetChild(0).SetParent(shelf.transform);
        }

        baseY = shelfPillars[0].transform.localPosition.y;

        Destroy(tmp);
    }

    public void CanPush(bool changeFlag) {
        canPush = changeFlag;
    }
    public void CanPull(bool changeFlag)
    {
        canPull = changeFlag;
    }

    public void PushShelf() {
        //int rowLimit;
        //if (smallMultiplesNumber % 2 == 0)
        //{
        //    rowLimit = smallMultiplesNumber / 2;
        //}
        //else {
        //    rowLimit = smallMultiplesNumber / 2 + 1;
        //}

        //currentZDistance += 0.01f;
        if (canPush) {
            //if (Mathf.Abs(currentZDistance) <= (rowLimit * delta))
            //{
            //    if (Mathf.Abs(currentZDistance) >= delta)
            //    {
            //        if (currentZDistance >= 0)
            //        {
                        ExpandShelf();

            //        }
            //        else
            //        {
            //            ShrinkShelf();
            //            currentZDistance -= 0.01f;
            //        }
            //    }
            }
        //else
        //{
        //    currentZDistance -= 0.01f;
        //}
        //Vector3 oldPosition = shelf.transform.position;
        //MoveShelfToCenter();
        //shelf.transform.position = oldPosition;

    }

    public void PullShelf() {
        //     int rowLimit;
        //     if (smallMultiplesNumber % 2 == 0)
        //     {
        //         rowLimit = smallMultiplesNumber / 2;
        //     }
        //     else
        //     {
        //         rowLimit = smallMultiplesNumber / 2 + 1;
        //     }

        //     currentZDistance -= 0.01f;

        //     if (Mathf.Abs(currentZDistance) <= (rowLimit * delta))
        //     {
        //if (Mathf.Abs (currentZDistance) >= delta) {
        //	if (currentZDistance >= 0) {
        if (canPull)
        {
            
            ShrinkShelf();
        }
        
					
			//	} else {
			//		//ExpandShelf();
			//	}
			//} else {
			//	currentZDistance += 0.01f;
			//}
   //     }
   //     else
   //     {
   //         currentZDistance += 0.01f;
   //     }
        //Vector3 oldPosition = shelf.transform.position;
        //MoveShelfToCenter();
        //shelf.transform.position = oldPosition;
        
        
    }

    void ExpandShelf() {
		Transform leftTransform = shelfPillars[2].transform;
		PositionLocalConstraints plcl = shelfPillars[2].gameObject.GetComponent<PositionLocalConstraints>();

		Transform rightTransform = shelfPillars[3].transform;
		PositionLocalConstraints plcr = shelfPillars[3].gameObject.GetComponent<PositionLocalConstraints>();

		plcl.UpdateZ(leftTransform.localPosition.z + curveScaleZDelta);
		plcr.UpdateZ(rightTransform.localPosition.z + curveScaleZDelta);

		currentBoardPositionZ += boardPositionZDelta;
        //curveFlagFloat++;
        currentCurvature += curvatureDelta;

		foreach (GameObject go in shelfBoards) {
			Bezier3PointCurve bpc = go.transform.GetChild(0).GetComponent<Bezier3PointCurve>();
			if (bpc != null) {
				bpc.ShelfPushed ();
			}
		}

		currentCurveRendererZ -= curveRendererZDelta;

		foreach (GameObject board in shelfBoards) {
			Transform boardTransform = board.transform;
            //boardTransform.localScale += Vector3.forward * curveScaleZDelta;
        }

        Transform roofBoardTran = roofBoard.transform;
        //roofBoardTran.localScale += Vector3.forward * curveScaleZDelta;
    }

    void ShrinkShelf() {
		Transform leftTransform = shelfPillars [2].transform;
		PositionLocalConstraints plcl = shelfPillars [2].gameObject.GetComponent<PositionLocalConstraints> ();

		Transform rightTransform = shelfPillars [3].transform;
		PositionLocalConstraints plcr = shelfPillars [3].gameObject.GetComponent<PositionLocalConstraints> ();

		plcl.UpdateZ (leftTransform.localPosition.z - curveScaleZDelta);
		plcr.UpdateZ (rightTransform.localPosition.z - curveScaleZDelta);

		currentBoardPositionZ -= boardPositionZDelta;
        //curveFlagFloat--;
        currentCurvature -= curvatureDelta;

		foreach (GameObject go in shelfBoards) {
			Bezier3PointCurve bpc = go.transform.GetChild(0).GetComponent<Bezier3PointCurve>();
			if (bpc != null) {
				bpc.ShelfPulled ();
			}
		}

		currentCurveRendererZ += curveRendererZDelta;

		foreach (GameObject board in shelfBoards) {
			Transform boardTransform = board.transform;
            //boardTransform.localScale -= Vector3.forward * curveScaleZDelta;
        }

        Transform roofBoardTran = roofBoard.transform;
        //roofBoardTran.localScale -= Vector3.forward * curveScaleZDelta;
    }

    // create objects

    void CreateShelf()
    {
        float iniLeftx = -(delta * smallMultiplesNumber / 2);
        float iniRightx = delta * smallMultiplesNumber / 2 + 0.1f;
        float iniy = 1;
        if (dataset == 2) {
            iniy = 1.05f;
        }
        float iniFrontz = -delta / 2;
        float iniBackz = delta / 2;

        GameObject pillar = (GameObject)Instantiate(frontPillarPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pillar.transform.SetParent(shelf.transform);
        pillar.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
        pillar.transform.localPosition = new Vector3(iniLeftx, iniy, iniFrontz);
        oldLeftPosition = pillar.transform.localPosition;
        pillar.name = "Left Pillar";
        shelfPillars.Add(pillar);

        //GameObject bezierPointLocaterLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //bezierPointLocaterLeft.SetActive(false);
        //bezierPointLocaterLeft.transform.SetParent(pillar.transform);
        //bezierPointLocaterLeft.transform.localPosition = new Vector3(delta * 10, 0, delta * 10);

        GameObject pillarIO = (GameObject)Instantiate(pillarIOPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pillarIO.transform.SetParent(shelf.transform);
        pillarIO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        pillarIO.transform.localPosition = pillar.transform.localPosition;
        pillarIO.name = "Left Pillar Middle IO";
        pillarIO.transform.Find("ObjectTooltip").localPosition = Vector3.left * 5;
        pillarIO.transform.GetChild(0).Find("LineStart").localPosition = Vector3.left * 0.05f;
        pillarMiddleIOs.Add(pillarIO);


        touchableObjects.Add(pillarIO);

        pillarIO = (GameObject)Instantiate(pillarIOPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pillarIO.transform.SetParent(shelf.transform);
        pillarIO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        pillarIO.transform.localPosition = pillar.transform.localPosition + Vector3.up * pillar.transform.localScale.y;
        pillarIO.name = "Left Pillar Top IO";
        pillarIO.transform.Find("ObjectTooltip").localPosition = Vector3.left * 6;
        pillarIO.transform.GetChild(0).Find("LineStart").localPosition = Vector3.left * 0.06f;
        pillarTopIOs.Add(pillarIO);

        pillar = (GameObject)Instantiate(frontPillarPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pillar.transform.SetParent(shelf.transform);
        pillar.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
        pillar.transform.localPosition = new Vector3(iniRightx, iniy, iniFrontz);
        oldRightPosition = pillar.transform.localPosition;
        pillar.name = "Right Pillar";
        shelfPillars.Add(pillar);

        pillar.transform.GetChild(0).localPosition = new Vector3(-delta * 10, 0, pillar.transform.GetChild(0).localPosition.z);
        PositionLocalConstraints plc = pillar.transform.GetChild(0).gameObject.GetComponent<PositionLocalConstraints>();
        plc.UpdateX(-delta * 10);

        //GameObject bezierPointLocaterRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //bezierPointLocaterRight.SetActive(false);
        //bezierPointLocaterRight.transform.SetParent(pillar.transform);
        //bezierPointLocaterRight.transform.localPosition = new Vector3(-delta * 10, 0, delta * 10);

        pillarIO = (GameObject)Instantiate(pillarIOPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pillarIO.transform.SetParent(shelf.transform);
        pillarIO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        pillarIO.transform.localPosition = pillar.transform.localPosition;
        pillarIO.name = "Right Pillar Middle IO";
        pillarIO.transform.Find("ObjectTooltip").localPosition = -Vector3.left * 5;
        pillarIO.transform.GetChild(0).Find("LineStart").localPosition = -Vector3.left * 0.05f;
        pillarMiddleIOs.Add(pillarIO);

        touchableObjects.Add(pillarIO);

        pillarIO = (GameObject)Instantiate(pillarIOPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pillarIO.transform.SetParent(shelf.transform);
        pillarIO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        pillarIO.transform.localPosition = pillar.transform.localPosition + Vector3.up * pillar.transform.localScale.y;
        pillarIO.name = "Right Pillar Top IO";
        pillarIO.transform.Find("ObjectTooltip").localPosition = -Vector3.left * 6;
        pillarIO.transform.GetChild(0).Find("LineStart").localPosition = -Vector3.left * 0.06f;
        pillarTopIOs.Add(pillarIO);

        foreach (GameObject topIO in pillarTopIOs)
        {
            Physics.IgnoreCollision(topIO.GetComponent<Collider>(), shelfPillars[0].GetComponent<Collider>());
            Physics.IgnoreCollision(topIO.GetComponent<Collider>(), shelfPillars[1].GetComponent<Collider>());
            foreach (GameObject middleIO in pillarMiddleIOs)
            {
                Physics.IgnoreCollision(middleIO.GetComponent<Collider>(), topIO.GetComponent<Collider>());
                Physics.IgnoreCollision(middleIO.GetComponent<Collider>(), shelfPillars[0].GetComponent<Collider>());
                Physics.IgnoreCollision(middleIO.GetComponent<Collider>(), shelfPillars[1].GetComponent<Collider>());
            }
        }



        pillar = (GameObject)Instantiate(BpillarPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pillar.transform.SetParent(shelf.transform);
        pillar.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
        pillar.transform.localPosition = new Vector3(iniLeftx, iniy, iniBackz);
        pillar.name = "Left Back Pillar ";
        shelfPillars.Add(pillar);

        pillar = (GameObject)Instantiate(BpillarPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        pillar.transform.SetParent(shelf.transform);
        pillar.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
        pillar.transform.localPosition = new Vector3(iniRightx, iniy, iniBackz);
        pillar.name = "Right Back Pillar";
        shelfPillars.Add(pillar);

        baseY = iniy;
        currentY = iniy;

        controllBall = (GameObject)Instantiate(controlBallPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        controllBall.transform.SetParent(this.transform);
        controllBall.transform.position = shelf.transform.position;
        controllBall.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        controllBall.name = "Control Ball";

        touchableObjects.Add(controllBall);

        colorScheme = (GameObject)Instantiate(colorSchemePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        colorScheme.transform.SetParent(shelf.transform);
        colorScheme.transform.localPosition = new Vector3(0, 0.74f, -delta / 2);
		colorScheme.transform.localScale = new Vector3 (2, 2, 2);
        colorScheme.name = "Color Scheme";

        if (dataset == 2) {
            colorScheme.SetActive(false);
        }

        GameObject board;

        board = (GameObject)Instantiate(shelfBoardPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        board.transform.SetParent(shelf.transform);
        board.transform.localScale = new Vector3(delta * smallMultiplesNumber + 0.1f, 0.003f, delta);
        board.transform.localPosition = new Vector3(0, 0.8f, 0);
        board.name = "Bottom Board";
        shelfBoards.Add(board);

        GameObject curveRenderer = (GameObject)Instantiate(curveRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        curveRenderer.transform.SetParent(board.transform);
        curveRenderer.transform.localPosition = new Vector3(0, 0, 0);
        curveRenderer.transform.localScale = new Vector3(1, 1, 1);
        curveRenderer.name = "Bottom Curve Renderer";
        curveRenderers.Add(curveRenderer);


        roofBoard = (GameObject)Instantiate(shelfBoardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        roofBoard.transform.SetParent(shelf.transform);
        roofBoard.transform.localScale = new Vector3(delta * smallMultiplesNumber + 0.1f, 0.003f, delta);
        roofBoard.transform.localPosition = new Vector3(0, 1.2f, 0);
        if (dataset == 2) {
            roofBoard.transform.localPosition = new Vector3(0, 1.25f, 0);
        }
        roofBoard.name = "Roof Board";

        shelfRows = 1;
        shelfItemPerRow = smallMultiplesNumber;

        centroidGO = (GameObject)Instantiate(centroidPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        centroidGO.name = "Centroid";
        centroidGO.transform.SetParent(this.transform);
        centroidGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    void CreateSM()
    {
        GameObject bottomBoard = shelfBoards[0];
        float iniLeftEdge = shelfPillars[0].transform.localPosition.x;
        float iniLeftMostItemX = iniLeftEdge + (delta / 2);
        float iniItemY = bottomBoard.transform.localPosition.y;

        if (dataset == 1) {
            for (int i = 0; i < smallMultiplesNumber; i++)
            {
                GameObject dataObj = new GameObject();

                dataObj = (GameObject)Instantiate(DataPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                dataObj.name = "Building " + (i + 1);


                dataObj.transform.SetParent(shelf.transform);
                dataObj.transform.localPosition = new Vector3(iniLeftMostItemX + (i * delta), iniItemY, 0);
                dataSM.Add(dataObj);

                string tooltipText = "";

                if (dateType == DateType.Monthly)
                {
                    tooltipText = IntToMonth(i + 1);
                }
                else if (dateType == DateType.Fornightly)
                {
                    tooltipText = (i + 1) + ToOrdinal(i + 1) + " two weeks";
                }
                else
                {
                    tooltipText = (i + 1) + ToOrdinal(i + 1) + " week";
                }

                Transform tooltip = dataObj.transform.GetChild(1);
                VRTK_ObjectTooltip ot = tooltip.GetComponent<VRTK_ObjectTooltip>();
                ot.displayText = tooltipText;


                Collider[] colliders = dataObj.GetComponentsInChildren<Collider>();

                foreach (Collider collider in colliders)
                {
                    Physics.IgnoreCollision(controllBall.GetComponent<Collider>(), collider);
                    Physics.IgnoreCollision(pillarTopIOs[0].GetComponent<Collider>(), collider);
                    Physics.IgnoreCollision(pillarTopIOs[1].GetComponent<Collider>(), collider);
                    Physics.IgnoreCollision(pillarMiddleIOs[0].GetComponent<Collider>(), collider);
                    Physics.IgnoreCollision(pillarMiddleIOs[1].GetComponent<Collider>(), collider);
                }

                foreach (Collider collider in colliders)
                {
                    foreach (GameObject pillar in shelfPillars)
                    {
                        Physics.IgnoreCollision(collider, pillar.GetComponent<Collider>());
                    }
                }
            }
        }
       
        if (dataset == 2) {



            bool smNumberEqualBar = false;

            GameObject barChartManager = GameObject.Find("BarChartManagement");
            if (barChartManager.transform.childCount == smallMultiplesNumber)
            {
                smNumberEqualBar = true;
            }

            for (int i = 0; i < smallMultiplesNumber; i++)
            {
                GameObject dataObj = new GameObject();

                if (smNumberEqualBar)
                {
                    barChartManager.transform.GetChild(0).SetParent(dataObj.transform);
                    dataObj.name = "Building " + (i + 1);
                    dataObj.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                    dataObj.transform.GetChild(0).localPosition = new Vector3(-0.5f, 0.3f, -0.5f);
                }
                else {
                    Debug.Log("Small multiple number not equals barChart number");
                    return;
                }


                dataObj.transform.SetParent(shelf.transform);
                dataObj.transform.localPosition = new Vector3(iniLeftMostItemX + (i * delta), iniItemY, 0);
                dataSM.Add(dataObj);

                Collider[] colliders = dataObj.GetComponentsInChildren<Collider>();

                foreach (Collider collider in colliders)
                {
                    Physics.IgnoreCollision(controllBall.GetComponent<Collider>(), collider);
                    Physics.IgnoreCollision(pillarTopIOs[0].GetComponent<Collider>(), collider);
                    Physics.IgnoreCollision(pillarTopIOs[1].GetComponent<Collider>(), collider);
                    Physics.IgnoreCollision(pillarMiddleIOs[0].GetComponent<Collider>(), collider);
                    Physics.IgnoreCollision(pillarMiddleIOs[1].GetComponent<Collider>(), collider);
                }

                foreach (Collider collider in colliders)
                {
                    foreach (GameObject pillar in shelfPillars)
                    {
                        Physics.IgnoreCollision(collider, pillar.GetComponent<Collider>());
                    }
                }
            }
        }
    }

    // static functions

    string IntToMonth(int i) {
        switch (i)
        {
            case 1:
                return "January";
            case 2:
                return "February";
            case 3:
                return "March";
            case 4:
                return "April";
            case 5:
                return "May";
            case 6:
                return "June";
            case 7:
                return "July";
            case 8:
                return "August";
            case 9:
                return "September";
            case 10:
                return "October";
            case 11:
                return "November";
            case 12:
                return "December";
            default:
                return "404 error";
        }
        
    }

	public static string ToOrdinal(int value)
	{
		// Start with the most common extension.
		string extension = "th";

		// Examine the last 2 digits.
		int last_digits = value % 100;

		// If the last digits are 11, 12, or 13, use th. Otherwise:
		if (last_digits < 11 || last_digits > 13)
		{
			// Check the last digit.
			switch (last_digits % 10)
			{
			case 1:
				extension = "st";
				break;
			case 2:
				extension = "nd";
				break;
			case 3:
				extension = "rd";
				break;
			}
		}

		return extension;
	}

	public Vector3 FindLeftPoint(){
		return new Vector3 (shelfPillars[0].transform.localPosition.x, shelfBoards[0].transform.localPosition.y, shelfPillars[0].transform.localPosition.z + delta / 2);
	}

	public Vector3 FindRightPoint(){
		return new Vector3 (shelfPillars[1].transform.localPosition.x, shelfBoards[0].transform.localPosition.y, shelfPillars[1].transform.localPosition.z + delta / 2);
	}

	public List<GameObject> GetSMList(){
		return this.dataSM;
	}

    public int GetRows() {
        return shelfRows;
    }

    public int GetItemPerRow() {
        return shelfItemPerRow;
    }

    //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
    //to each other. This function finds those two points. If the lines are not parallel, the function 
    //outputs true, otherwise false.
    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else
        {
            return false;
        }
    }

    void ReadACFile()
    {
        string[] lines = ACFile.text.Split(lineSeperater);
        foreach (string line in lines)
        {
            string[] fields = line.Split(fieldSeperator);
            bool header = false;
            foreach (string field in fields)
            {
                if (field == "\"\"")
                {
                    header = true;
                }
            }
            if (!header)
            {
                string[] formats = {"d/MM/yyyy h:mm:ss tt", "d/MM/yyyy h:mm tt",
                    "dd/MM/yyyy hh:mm:ss tt", "d/M/yyyy h:mm:ss",
                    "d/M/yyyy hh:mm tt", "d/M/yyyy hh tt",
                    "d/M/yyyy h:mm", "d/M/yyyy h:mm",
                    "dd/MM/yyyy hh:mm", "d/M/yyyy hh:mm"};
                string name = (fields[8]).Replace("\"", "").Trim();
                DateTime dt;
                bool isSuccess = DateTime.TryParseExact(fields[1].Replace("\"", "").Trim(), formats,
                    new CultureInfo("en-US"),
                    DateTimeStyles.None,
                    out dt);
                if (isSuccess)
                {
                    float temp = -1;
                    float spHi = -1;
                    float spLo = -1;
                    string status = "Off";
                    string acUnit = "Off";
                    float roofTemp = -1;
                    if (fields[2] != "\"-\"")
                    {
                        temp = float.Parse((fields[2]).Replace("\"", "").Trim());
                    }
                    if (fields[3] != "\"-\"")
                    {
                        spHi = float.Parse((fields[3]).Replace("\"", "").Trim());
                    }
                    if (fields[4] != "\"-\"")
                    {
                        spLo = float.Parse((fields[4]).Replace("\"", "").Trim());
                    }
                    if (fields[5] != "\"-\"")
                    {
                        status = (fields[5]).Replace("\"", "").Trim();
                    }
                    if (fields[6] != "\"-\"")
                    {
                        acUnit = (fields[6]).Replace("\"", "").Trim();
                    }
                    if (fields[7] != "\"-\"")
                    {
                        roofTemp = float.Parse((fields[7]).Replace("\"", "").Trim());
                    }

					SensorReading sr = new SensorReading ();
					sr.dt = dt;
					sr.temp = temp;
					sr.spHi = spHi;
					sr.spLo = spLo;
					sr.status = status;
					sr.acUnit = acUnit;
					sr.roofTemp = roofTemp;

                    if (sensorsInfoList.ContainsKey(name))
                    {
                        sensorsInfoList[name].Add(sr);
                    }
                    else {
                        sensorsInfoList.Add(name, new HashSet<SensorReading>());
                        sensorsInfoList[name].Add(sr);
                    }
                }
            }
        }
    }
}
