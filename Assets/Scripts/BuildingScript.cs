using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.IO;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

public class BuildingScript : MonoBehaviour {


    GestureRecognizer gr = null;

    public Camera mainCamera;

    public GameObject viveCamera;

	public GameObject Ground;
	public GameObject Floor1;
	public GameObject Roof;


	public TextAsset ACFile;
	public TextAsset OAPUFile;


	//Vector3 deltaCenterG;
	//Vector3 deltaCenter1;


	GameObject wallIntF1_1;
	GameObject wallIntF1_2;
	GameObject wallIntG;


	GameObject wallExtF1;
	GameObject wallExtG;


	//bool movingC = false;

	//bool moving = false;
	//GameObject toMove;
	//Vector3 targetTranslation;
	//Vector3 currentTransltation;
    //Vector3 targetPos;


	bool movingCamera=false;
	Vector3 targetTransCam;
	Vector3 currentTransCam;
	Vector3 whereToLook;

	List<GameObject> IntWallList;
	List<GameObject> ExtWallList;

	public Dictionary<string, GameObject> sensorsF1;
	public Dictionary<string, GameObject> sensorsG;

	private bool play;
	private DateTime present;
    //public GameObject playButton;

	private float nexTime;
	private float delta = 5;

    private bool isExploded;
	private bool isMagnified;


    public Text simulationText;

    private Vector3 realCentre;

	//private List<GameObject> infoLessSensors;

	private int explosionIndex = 0;



    // Use this for initialization
    void Start () {
		//infoLessSensors = new List<GameObject> ();

		play = false;
        isExploded = false;
		isMagnified = false;

        realCentre = this.GetComponent<BoxCollider>().center;

        //gr = new GestureRecognizer();
        //gr.TappedEvent += Tap;
        //gr.StartCapturingGestures();

        IntWallList = new List<GameObject> ();
		ExtWallList = new List<GameObject> ();
		sensorsF1 = new Dictionary<string, GameObject> ();
		sensorsG = new Dictionary<string, GameObject> ();

		GameObject childFloor = Floor1.transform.Find ("Floor").gameObject;
		GameObject childChildFloor = childFloor.transform.Find ("floor").gameObject;

		GameObject childFloorGround = Ground.transform.Find ("Plane001").gameObject;

		//deltaCenter1 = childChildFloor.transform.position;
		//deltaCenterG = childFloorGround.transform.position;

		wallIntF1_1 = Floor1.transform.Find ("wallInt1").gameObject.transform.Find("Internal Wall 1").gameObject;
		wallIntF1_2 = Floor1.transform.Find ("wallInt2").gameObject.transform.Find("Internal Wall 2").gameObject;
		wallIntG = Ground.transform.Find ("wallInt").gameObject.transform.Find("Internal walls").gameObject;

		IntWallList.Add (wallIntF1_1);
		IntWallList.Add (wallIntF1_2);
		IntWallList.Add (wallIntG);

		wallExtF1 = Floor1.transform.Find ("extWall").gameObject.transform.Find("External Wall").gameObject;
		wallExtG = Ground.transform.Find ("extWall").gameObject.transform.Find("External walls").gameObject;

		ExtWallList.Add (wallExtF1);
		ExtWallList.Add (wallExtG);

		Color darkGray = new Color();
		ColorUtility.TryParseHtmlString ("#525252", out darkGray);

		Color lightGray = new Color();
		ColorUtility.TryParseHtmlString ("#969696", out lightGray);

		Color blueSensor = new Color ();
		ColorUtility.TryParseHtmlString ("#1d91c0", out blueSensor);

		foreach (GameObject wall in IntWallList) {
			wall.GetComponent<Renderer> ().material.color = lightGray;
		}

		foreach (GameObject wall in ExtWallList) {
			wall.GetComponent<Renderer> ().material.color = darkGray;
		}

        sensorsG = getSensorG();
        sensorsF1 = getSensorF1();
        //StreamWriter outStream = System.IO.File.CreateText(".\\acPos.csv");
        //outStream.WriteLine("Name, Floor, x, y, z");
        
		
        //outStream.Close();


        //testSortedList ();

        //wallExtG.GetComponent<Renderer>().material.color = darkGray;
        //wallExtF1.GetComponent<Renderer>().material.color = darkGray;


        //wallIntF1_1.GetComponent<Renderer>().material.color = lightGray;
        //wallIntF1_2.GetComponent<Renderer>().material.color = lightGray;
        //wallIntG.GetComponent<Renderer>().material.color = lightGray;


        //print (deltaCenter1);
        //print (deltaCenterG);

        //transform.position = -1 * deltaCenterG;
        //Vector3 result = new Vector3(20, 20, -20); 
        //mainCamera.transform.position = result;
        //Vector3 midCD = new Vector3 (0, 0, 0);
        //mainCamera.transform.LookAt(midCD);

        //Put down all the sensors
        foreach (KeyValuePair<string, GameObject> de in sensorsG)
        {
            //print (de.Key);
            de.Value.transform.Translate(new Vector3(0,0,-0.05f));
        }
        foreach (KeyValuePair<string, GameObject> de in sensorsF1)
        {
            //print (de.Key);
            de.Value.transform.Translate(new Vector3(0, 0, -0.045f));
        }
			

        // find vive camera
        viveCamera = GameObject.Find("Camera (eye)");
    }



    public Dictionary<string, GameObject> getSensorG() {

		Color lightGray = new Color();
		ColorUtility.TryParseHtmlString ("#969696", out lightGray);

//        Color blueSensor = new Color();
//        ColorUtility.TryParseHtmlString("#1d91c0", out blueSensor);
        Dictionary<string, GameObject> temp = new Dictionary<string, GameObject>();

        foreach (Transform child in Ground.transform.Find("Sensor"))
        {
            temp.Add(child.gameObject.name.Trim(), child.gameObject);
			child.gameObject.GetComponent<Renderer>().material.color = lightGray;

            child.localScale = new Vector3(1,1,1.7f);
        }

        return temp;
    }

    public Dictionary<string, GameObject> getSensorF1()
    {
		Color lightGray = new Color();
		ColorUtility.TryParseHtmlString ("#969696", out lightGray);

//        Color blueSensor = new Color();
//        ColorUtility.TryParseHtmlString("#1d91c0", out blueSensor);

        Dictionary<string, GameObject> temp = new Dictionary<string, GameObject>();
        foreach (Transform child in Floor1.transform.Find("Sensor"))
        {
            temp.Add(child.gameObject.name.Trim(), child.gameObject);
			child.gameObject.GetComponent<Renderer>().material.color = lightGray;
        }
        return temp;
    }


    void Awake()
    {
        present = new DateTime(2017, 3, 6, 6, 30, 00);
        //simulationText.text = "Simulation \nPause: " + getDT()+ "\nOutside Temperature: 27°C";
    }

    private void Tap(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        this.explode();
    }

    public bool getPlay()
    {
        return play;
    }

    public DateTime getDT()
    {
        return this.present;
    }
		
    public Vector3 getCentreCoordinates()
    {
        return transform.TransformPoint(this.realCentre);
    }

	void to2DView(){

		Roof.SetActive (false);
		foreach (GameObject wall in IntWallList) {
			wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
		}

		foreach (GameObject wall in ExtWallList) {
			wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
		}


		
		GameObject childFloor = Floor1.transform.Find ("Floor").gameObject;
		GameObject childChildFloor = childFloor.transform.Find ("floor").gameObject;

		GameObject childFloorGround = Ground.transform.Find ("Plane001").gameObject;

		Vector3 posF1 = childChildFloor.transform.position;
		Vector3 posG = childFloorGround.transform.position;

		Vector3 targPos = posG + new Vector3 (0, 0, 20);

		Vector3 targetTrans = targPos - posF1;

		//moving= true;
		//targetTranslation = targetTrans;
		//currentTransltation = new Vector3 (0, 0, 0);
		//toMove = Floor1;

		//Vector3 transFloor1 = new Vector3 (10, 0, 0);
		//Vector3 transGround = new Vector3 (-10, 0, 0);
		//Floor1.transform .Translate(transFloor1);
		//Ground.transform .Translate(transGround);
		//removeRoof ();

		Vector3 midCD = posG + (posG+targPos)/2;
		Vector3 result = midCD + new Vector3(0, 40, 0); 
		//mainCamera.transform.position = result;
		//Vector3 midCD = new Vector3 (0, 0, 0);
		whereToLook = midCD;
		movingCamera = true;
		targetTransCam = result;
		currentTransCam = new Vector3 (0, 0, 0);
		//mainCamera.transform.LookAt(midCD);
	}

	public void removeRoof(){
		Roof.SetActive (false);
		foreach (GameObject wall in IntWallList) {
			wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
		}

		foreach (GameObject wall in ExtWallList) {
			wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
		}

		GameObject childFloor = Floor1.transform.Find ("Floor").gameObject;
		GameObject childChildFloor = childFloor.transform.Find ("floor").gameObject;

		GameObject childFloorGround = Ground.transform.Find ("Plane001").gameObject;

		Vector3 posF1 = childChildFloor.transform.position;
		Vector3 posG = childFloorGround.transform.position; 

		Vector3 transFloor1 = new Vector3 (0, 0.1f, 0);

		Vector3 posFinalF1 = posF1 + transFloor1;

		//if (!movingC) {
		//	StartCoroutine (MoveObject(Floor1.transform, posF1, posFinalF1, childChildFloor.transform.rotation, childChildFloor.transform.rotation, 5f));
		//}

		//Floor1.transform .Translate(transFloor1);
		//moving= true;
		//targetTranslation = transFloor1;
        //targetPos = posFinalF1;
        //currentTransltation = new Vector3 (0, 0, 0);
		//toMove = Floor1;
        isExploded = true;
    }

    public void putBackRoof()
    {
        Roof.SetActive(true);
        foreach (GameObject wall in IntWallList)
        {
            wall.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        foreach (GameObject wall in ExtWallList)
        {
            wall.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        GameObject childFloor = Floor1.transform.Find("Floor").gameObject;
        GameObject childChildFloor = childFloor.transform.Find("floor").gameObject;

        GameObject childFloorGround = Ground.transform.Find("Plane001").gameObject;

        Vector3 posF1 = childChildFloor.transform.position;
        Vector3 posG = childFloorGround.transform.position;

        Vector3 transFloor1 = new Vector3(0, -0.1f, 0);

        Vector3 posFinalF1 = posF1 + transFloor1;

        //if (!movingC) {
        //	StartCoroutine (MoveObject(Floor1.transform, posF1, posFinalF1, childChildFloor.transform.rotation, childChildFloor.transform.rotation, 5f));
        //}

        //Floor1.transform .Translate(transFloor1);
        //moving = true;
        //targetTranslation = transFloor1;
        //targetPos = posFinalF1;
        //currentTransltation = new Vector3(0, 0, 0);
        //toMove = Floor1;
        isExploded = false;
    }

    public void explode()
    {
        if (this.isExploded)
        {
            putBackRoof();
        }
        else
        {
            removeRoof();
        }
    }

	public bool IsExploded()
	{
		return isExploded;
	}

	public void SetMagnify(bool magnify){
		this.isMagnified = magnify;
	}


	public bool IsMagnified(){
		return isMagnified;
	}
		

	public void ChangeExplosion(){

		if (explosionIndex == 0) {
			explosionIndex++;
			removeRoof ();
		} else if (explosionIndex == 1) {
			explosionIndex++;
			putBackRoof ();
			SensorOnly ();
		} else if (explosionIndex == 2) {
			explosionIndex = 0;
			ResetBuildingFromSensorOnly ();
		}
	}

	public int CurrentExplosion(){
		return explosionIndex;
	}

	void SensorOnly(){
		Color lightGray = new Color();
		ColorUtility.TryParseHtmlString ("#969696", out lightGray);

		Roof.SetActive (false);
		foreach (GameObject wall in IntWallList) {
			wall.SetActive (false);
		}

		foreach (GameObject wall in ExtWallList) {
			wall.SetActive (false);
		}

		GameObject childFloor = Floor1.transform.Find ("Floor").gameObject;
		childFloor.SetActive (false);

		GameObject block = Floor1.transform.Find ("Block:block 131").gameObject;
		block.SetActive (false);

		GameObject childFloorGround = Ground.transform.Find ("Plane001").gameObject;
		childFloorGround.SetActive (false);
        if (sensorsG != null && sensorsF1 != null) {
            foreach (KeyValuePair<string, GameObject> de in sensorsG)
            {
                if (de.Value.GetComponent<Renderer>().material.color == lightGray)
                {
                    de.Value.SetActive(false);
                }
            }

            foreach (KeyValuePair<string, GameObject> de in sensorsF1)
            {
                if (de.Value.GetComponent<Renderer>().material.color == lightGray)
                {
                    de.Value.SetActive(false);
                }
            }
        }
		

		isExploded = true;
	}

	void ResetBuildingFromSensorOnly(){
		Color lightGray = new Color();
		ColorUtility.TryParseHtmlString ("#969696", out lightGray);

		Roof.SetActive (true);
		foreach (GameObject wall in IntWallList) {
			wall.SetActive (true);
		}

		foreach (GameObject wall in ExtWallList) {
			wall.SetActive (true);
		}

		GameObject childFloor = Floor1.transform.Find ("Floor").gameObject;
		childFloor.SetActive (true);

		GameObject block = Floor1.transform.Find ("Block:block 131").gameObject;
		block.SetActive (true);

		GameObject childFloorGround = Ground.transform.Find ("Plane001").gameObject;
		childFloorGround.SetActive (true);

        if (sensorsG != null) {
            foreach (KeyValuePair<string, GameObject> de in sensorsG)
            {
                if (de.Value.GetComponent<Renderer>().material.color == lightGray)
                {
                    de.Value.SetActive(true);
                }
            }
        }

        if(sensorsF1 != null) {
            foreach (KeyValuePair<string, GameObject> de in sensorsF1)
            {
                if (de.Value.GetComponent<Renderer>().material.color == lightGray) {
                    de.Value.SetActive(true);
                }
            }
        }


		isExploded = false;
	}
    //    public void buttonPlay()
    //    {
    //        if (this.play)
    //        {
    //            Stop();
    //            //simulationText.text = "Simulation \nPause: " + getDT()+"\nOutside Temperature: 27°C";
    //        }
    //        else
    //        {
    //            playBS();
    //            //simulationText.text = "Simulation \nPlay: " + getDT()+"\nOutside Temperature: 27°C";
    //        }
    //    }


    //	void previsionMode ()
    //	{
    //		Roof.SetActive (false);
    //		foreach (GameObject wall in IntWallList) {
    //			wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
    //		}
    //
    //		foreach (GameObject wall in ExtWallList) {
    //			wall.transform.localScale = new Vector3(1f, 1f, 0.1f);
    //		}
    //
    //		GameObject childFloor = Floor1.transform.Find ("Floor").gameObject;
    //		GameObject childChildFloor = childFloor.transform.Find ("floor").gameObject;
    //
    //		GameObject childFloorGround = Ground.transform.Find ("Plane001").gameObject;
    //
    //		Vector3 posF1 = childChildFloor.transform.position;
    //		Vector3 posG = childFloorGround.transform.position; 
    //
    //		Vector3 transFloor1 = new Vector3 (0, posG.y - posF1.y, 0.2f);
    //
    //		Vector3 posFinalF1 = posF1 + transFloor1;
    //		moving= true;
    //		targetTranslation = transFloor1;
    //        targetPos = posFinalF1;
    //        currentTransltation = new Vector3 (0, 0, 0);
    //		toMove = Floor1;
    //		playAllSensors (this.present);
    //
    //
    //		foreach (KeyValuePair<string, GameObject> de in sensorsG) {
    //			//print (de.Key);
    //			SensorInfo si = ((GameObject) de.Value).transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
    //			si.predictionMode();
    //		}
    //		foreach (KeyValuePair<string, GameObject> de in sensorsF1) {
    //			//print (de.Key);
    //			SensorInfo si = ((GameObject) de.Value).transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
    //			si.predictionMode();
    //		}
    //
    //
    //	}

    //	void playAllSensors (DateTime present)
    //	{
    //		foreach (KeyValuePair<string, GameObject> de in sensorsG) {
    //			//print (de.Key);
    //			SensorInfo si = ((GameObject) de.Value).transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
    //			si.play (present);
    //		}
    //		foreach (KeyValuePair<string, GameObject> de in sensorsF1) {
    //			//print (de.Key);
    //			SensorInfo si = ((GameObject) de.Value).transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
    //			si.play (present);
    //		}
    //	}
    //
    //	void sendNewTime(DateTime present){
    //		foreach (KeyValuePair<string, GameObject> de in sensorsG) {
    //			//print (de.Key);
    //			SensorInfo si = ((GameObject) de.Value).transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
    //			si.newTime (present);
    //		}
    //		foreach (KeyValuePair<string, GameObject> de in sensorsF1) {
    //			//print (de.Key);
    //			SensorInfo si = ((GameObject) de.Value).transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
    //			si.newTime (present);
    //		}
    //	}

    //	public void playBS()
    //    {
    //        play = true;
    //        playAllSensors(present);
    //        this.nexTime = Time.time + this.delta;
    //    }
    //
    //    public void Stop()
    //    {
    //        play = false;
    //    }

//    void MagnifyBuilding()
//    {
//        GameObject leftController = GameObject.Find("Controller (left)");
//        GameObject rightController = GameObject.Find("Controller (right)");
//
//        Vector3 leftControllerDir = leftController.transform.forward;
//        Vector3 rightControllerDir = rightController.transform.forward;
//
//        Vector3 leftControllerObjectV = this.transform.parent.position - leftController.transform.position;
//        Vector3 rightControllerObjectV = this.transform.parent.position - rightController.transform.position;
//
//        if (Vector3.Angle(leftControllerDir, leftControllerObjectV) < 10 || Vector3.Angle(rightControllerDir, rightControllerObjectV) < 10)
//        {
//            if (!magnified)
//            {
//                this.transform.parent.localScale = new Vector3(this.transform.parent.localScale.x * 2, this.transform.parent.localScale.y * 2, this.transform.parent.localScale.z * 2);
//                magnified = true;
//            }
//        }
//        else
//        {
//            if (magnified)
//            {
//                this.transform.parent.localScale = new Vector3(this.transform.parent.localScale.x / 2, this.transform.parent.localScale.y / 2, this.transform.parent.localScale.z / 2);
//                magnified = false;
//            }
//        }
//    }

    // Update is called once per frame
    void Update () {
//		if (isExploded)
//		{
//			if (!magnified) {
//				this.transform.parent.localScale = Vector3.one;
//			}
//			//Debug.Log(Vector3.Distance(Camera.main.transform.position, this.transform.position));
//			MagnifyBuilding();
//		}
//		else {
//			this.transform.parent.localScale = Vector3.one;
//		}
        
        //		if (Input.GetKeyDown ("space")) {
        //			//to2DView ();
        //			play = true;
        //			playAllSensors (present);
        //			this.nexTime = Time.time + this.delta;
        //		}
        //
        //
        //		if (play) {
        //			if (Time.time > this.nexTime) {
        //				this.present = this.present.AddMinutes (15);
        //				print ("New Time: " + this.present);
        //				sendNewTime (this.present);
        //				this.nexTime = Time.time + this.delta;
        //
        //                //simulationText.text = "Simulation \nPlay: " + getDT()+ "\nOutside Temperature: 27°C";
        //                //playButton.GetComponent<CubeFocus>().changeText();
        //            }
        //		}
        //
        //		if (!moving) {
        //			if (Input.GetKeyDown (KeyCode.R)) {
        //                if (!isExploded)
        //                {
        //                    removeRoof();
        //                }else
        //                {
        //                    putBackRoof();
        //                }
        //			}
        //            if (Input.GetKeyDown(KeyCode.B))
        //            {
        //                putBackRoof();
        //            }
        //            if (Input.GetKeyDown (KeyCode.P)) {
        //				previsionMode ();
        //			}
        //
        //		}
        //		if (moving) {
        //			Vector3 actualTrans = targetTranslation * Time.deltaTime;
        //			toMove.transform.Translate (actualTrans);
        //			currentTransltation += actualTrans;
        //			float distance = Vector3.Distance (currentTransltation, targetTranslation);
        //			//print (distance);
        //			if ( distance < 0.05) {
        //				//toMove.transform.position = targetPos;
        //				moving = false;
        //			}
        //			//Floor1.transform.Translate(Vector3.up * Time.deltaTime);
        //		}
        //		if (movingCamera) {
        //			//print ("Moving Camera");
        //			//print (targetTransCam);
        //			Vector3 actualTrans = targetTransCam * Time.deltaTime;
        //			print (actualTrans);
        //			mainCamera.transform.Translate (actualTrans);
        //			currentTransCam += actualTrans;
        //			float distance = Vector3.Distance (currentTransCam, targetTransCam);
        //			mainCamera.transform.LookAt(whereToLook);
        //			print (distance);
        //			if ( distance < 0.5) {
        //				movingCamera = false;
        //				mainCamera.transform.LookAt(whereToLook);
        //			}
        //		}

    }

//    IEnumerator MoveObject (Transform thisTransform, Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot, float time){
//		movingC = true; // MoveObject started
//		float i = 0;
//		float rate = 1/time;
//		while (i < 1)         {
//			i += Time.deltaTime * rate;
//			thisTransform.position = Vector3.Lerp(startPos, endPos, i);
//			thisTransform.rotation = Quaternion.Slerp (startRot, endRot, i);
//			yield return 0;
//		}
//		movingC = false; // MoveObject ended
//	}

}
