using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using VRTK;

public class SensorScript : MonoBehaviour
{
    private Shader silhouetteShader;

    //    public GameObject tag;
    //    public Text textSensor;
    //	  public Camera cam;
    //    TextMesh nameT;
    //    TextMesh temp;
    //    TextMesh state;
    //    TextMesh sp;
    //	  GameObject tempBlue;
    //	  GameObject tempRed;
    //	  GameObject tempGreen;
    //	  int stateTemp;

    private GameObject ToolTipPrefab;

    private SensorInfo si;
    private BuildingScript bs;

    private GameObject tooltip;

    private bool hasInfo = false;
    private bool initiated = false;


    // Use this for initialization
    void Start () {
		

        SmallMultiplesManagerScript smms = GameObject.Find("SmallMultiplesManager").GetComponent<SmallMultiplesManagerScript>();
        ToolTipPrefab = smms.tooltipPrefab;
        silhouetteShader = smms.silhouetteShader;

        bs = this.transform.parent.parent.parent.GetComponent<BuildingScript>();

        si = transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
		ReadDataFromManager ();
		if (hasInfo) {
			//si.getMonthlySensorReadings ();
			if(smms.dateType == DateType.Monthly){
				si.getMonthlySensorReadings ();
			}
			if (smms.dateType == DateType.Fornightly) {
				si.getSensorReadings (smms.smallMultiplesNumber, 14);
			}
			if(smms.dateType == DateType.Weekly) {
				si.getSensorReadings (smms.smallMultiplesNumber, 7);
			}
			ChangeColor ();
            SendTempTag();
		}
	}

    void SendTempTag() {
        if(si.tempTag != null)
        {
            SmallMultiplesManagerScript smms = GameObject.Find("SmallMultiplesManager").GetComponent<SmallMultiplesManagerScript>();
            smms.AssignTempTag(si.tempTag);
        }
        
    }

	// Update is called once per frame
	void Update () {
        if (hasInfo && initiated)
        {
            if (bs.IsExploded())
            {
				BuildingScript bs = this.transform.parent.parent.parent.gameObject.GetComponent<BuildingScript> ();
				if (bs.IsMagnified ()) {
					tooltip.SetActive (true);
				} else {
					tooltip.SetActive (false);
				}
                


                
            //Camera realCamera = Camera.main;
                //if (realCamera != null)
                //{
                //    Vector3 viewPos = realCamera.WorldToViewportPoint(this.transform.position);
                //    if (viewPos.x > 0 && viewPos.y > 0 && viewPos.z > 0)
                //    {
                //        tooltip.SetActive(true);
                //    }
                //    else {
                //        tooltip.SetActive(false);
                //    }
                //}
            }
            else
            {
                tooltip.SetActive(false);
            }

            
        }
	}

	void ReadDataFromManager() {
		HashSet<SensorReading> sensorHS;
		if (SmallMultiplesManagerScript.sensorsInfoList.TryGetValue(this.name.Trim(), out sensorHS))
		{
			hasInfo = true;
			foreach (SensorReading sr in sensorHS) {
				si.sendDataAC(sr.dt, sr.temp, sr.spHi, sr.spLo, sr.status, sr.acUnit, sr.roofTemp);
			}
		}
	}

	// calculate average temperature and change sensor coloring
	public void ChangeColor() {
		int buildingNumber = Int32.Parse (this.transform.parent.parent.parent.parent.name.Remove(0,9));

		if (si.tempAvg.Length > 0) { // if has sensor information for the current sensor
			// print sensor information
			//Debug.Log ("Building: " + buildingNumber + ", Sensor: " + this.name + ", Avg temp:" + si.tempAvg [buildingNumber - 1]);

			float tempValue = si.tempAvg [buildingNumber - 1];

			if (tempValue >= 27) {
				Color cl27 = new Color ();
				ColorUtility.TryParseHtmlString ("#d73027", out cl27);
				this.GetComponent<Renderer> ().material.color = cl27;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl27);

                this.transform.localScale = new Vector3 (1,1,2.6f);
			} else if (tempValue >= 26 & tempValue < 27) {
				Color cl26 = new Color ();
				ColorUtility.TryParseHtmlString ("#f46d43", out cl26);
				this.GetComponent<Renderer> ().material.color = cl26;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl26);

                this.transform.localScale = new Vector3 (1,1,2.4f);
			} else if (tempValue >= 25 & tempValue < 26) {
				Color cl25 = new Color ();
				ColorUtility.TryParseHtmlString ("#ffc800", out cl25);
				this.GetComponent<Renderer> ().material.color = cl25;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl25);
                

                this.transform.localScale = new Vector3 (1,1,2.2f);
			} else if (tempValue >= 24 & tempValue < 25) {
				Color cl24 = new Color ();
				ColorUtility.TryParseHtmlString ("#ffffbf", out cl24);
				this.GetComponent<Renderer> ().material.color = cl24;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl24);

                this.transform.localScale = new Vector3 (1,1,2f);
			} else if (tempValue >= 23 & tempValue < 24) {
				Color cl23 = new Color ();
				ColorUtility.TryParseHtmlString ("#8aff00", out cl23);
				this.GetComponent<Renderer> ().material.color = cl23;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl23);

                this.transform.localScale = new Vector3 (1,1,1.8f);
			} else if (tempValue >= 22 & tempValue < 23) {
				Color cl22 = new Color ();
				ColorUtility.TryParseHtmlString ("#1a9850", out cl22);
				this.GetComponent<Renderer> ().material.color = cl22;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl22);

                this.transform.localScale = new Vector3 (1,1,1.6f);
			} else if (tempValue >= 21 & tempValue < 22) {
				Color cl21 = new Color ();
				ColorUtility.TryParseHtmlString ("#00fff4", out cl21);
				this.GetComponent<Renderer> ().material.color = cl21;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl21);

                this.transform.localScale = new Vector3 (1,1,1.4f);
			} else if (tempValue >= 20 & tempValue < 21) {
				Color cl20 = new Color ();
				ColorUtility.TryParseHtmlString ("#0064ff", out cl20);
				this.GetComponent<Renderer> ().material.color = cl20;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl20);

                this.transform.localScale = new Vector3 (1,1,1.2f);
			} else {
				Color cl19 = new Color ();
				ColorUtility.TryParseHtmlString ("#542788", out cl19);
				this.GetComponent<Renderer> ().material.color = cl19;
                this.GetComponent<Renderer>().material.SetColor("_OutlineColor", cl19);

                this.transform.localScale = new Vector3 (1,1,1);
			}

            this.GetComponent<Renderer>().material.shader = silhouetteShader;
            this.GetComponent<Renderer>().material.SetFloat("_Outline", 0.001f);

            //			if (tempValue <= tempLow) { // low temperature
            //				this.GetComponent<Renderer> ().material.color = Color.blue;
            //			} else if (tempValue <= tempHigh) { // medium temperature
            //				this.GetComponent<Renderer> ().material.color = Color.yellow;
            //			} else {// high temperature
            //				this.GetComponent<Renderer> ().material.color = Color.red;
            //			}
            tooltip = (GameObject)Instantiate(ToolTipPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            tooltip.transform.SetParent(this.transform);
            if (this.name.Contains("G"))
            {
                this.transform.localScale = new Vector3(1, 1, this.transform.localScale.z * 1.7f);
                tooltip.transform.localPosition = new Vector3(0, 0, 0.8f / 1.7f);
            }
            else {
                tooltip.transform.localPosition = new Vector3(0, 0, 0.8f);
            }
            tooltip.transform.localScale = new Vector3(30, 30 / this.transform.localScale.z, 30 );

            tooltip.name = this.name.Trim() + " tooltip";

            VRTK_ObjectTooltip ot = tooltip.GetComponent<VRTK_ObjectTooltip>();
            ot.displayText = this.name.Trim();
        }
        else{ // no info snesor
			Color lightGray = new Color();
			ColorUtility.TryParseHtmlString ("#969696", out lightGray);
//			Color blueSensor = new Color ();
//			ColorUtility.TryParseHtmlString ("#1d91c0", out blueSensor);
			this.GetComponent<Renderer> ().material.color = lightGray;
		}
        initiated = true;
	}


//    public void OnFocusEnter()
//    {
//        //tag.transform.position = transform.position + new Vector3(0,0.05f,0);
//        //var n = cam.transform.position - tag.transform.position;
//        //tag.transform.rotation = Quaternion.LookRotation(n)*Quaternion.Euler(0,180,0);
//        SensorInfo si = transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
//        //nameT.text = this.name;
//        //state.text = "State: "+si.getState();
//        //temp.text = "Temp: "+si.getTempS();
//        //sp.text = "SP: " + si.getSPLoS() + "-" + si.getSPHiS();
//        string entireText = this.name+"\n"+ "State: " + si.getState() + "\n" + "Temp: " + si.getTempS() + "\n" + "SP: " + si.getSPLoS() + "-" + si.getSPHiS();
//        textSensor.text = entireText;
//        //tag.SetActive(true); 
//
//    }
//
//    public void OnFocusExit()
//    {
//        //tag.SetActive(false);
//        textSensor.text = "";
//    }
//


//    // Use this for initialization
//    void Start () {
//        //nameT = tag.transform.Find("name").gameObject.GetComponent<TextMesh>();
//        //temp = tag.transform.Find("temp").gameObject.GetComponent<TextMesh>();
//        //state = tag.transform.Find("state").gameObject.GetComponent<TextMesh>();
//        //sp = tag.transform.Find("sp").gameObject.GetComponent<TextMesh>();
//        si = transform.Find("sensorInfo").gameObject.GetComponent<SensorInfo>();
//        if (si.getType() == "AC")
//        {
//            /*GameObject tempBlue = Resources.Load("logoBlue") as GameObject;
//            if(tempBlue == null)
//            {
//                print("TempBlue is Null");
//            }
//            GameObject tempLogo = Instantiate(tempBlue);
//            tempLogo.transform.position = transform.position;
//            tempLogo.transform.parent = transform;
//            tempLogo.transform.Rotate(new Vector3(90f,0f,0f));
//            tempLogo.transform.localScale = new Vector3(0.7f,0.05f,0.7f);            //tempLogo.transform.position
//            tempLogo.transform.Translate(new Vector3(0f,0f,-0.03f));*/
//            //initTempLogo(ref tempBlue, "logoBlue");
//            //initTempLogo(ref tempRed, "logoRed");
//            //initTempLogo(ref tempGreen, "logoGreen");
//            stateTemp = 0;
//            //tempGreen.SetActive(true);
//        }
//    }

//    void initTempLogo(ref GameObject temp, string name)
//    {
//        /*GameObject tempP = Resources.Load(name) as GameObject;
//        temp = Instantiate(tempP);
//        temp.transform.position = transform.position;
//        temp.transform.parent = transform;
//        temp.transform.Rotate(new Vector3(90f, 0f, 0f));
//        temp.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f);            //tempLogo.transform.position
//        temp.transform.Translate(new Vector3(0f, 0f, -0.03f));
//        temp.SetActive(false);*/
//    }

//    public void changeState(int newStateTemp)
//    {
//        /*if (newStateTemp != this.stateTemp)
//        {
//            this.stateTemp = newStateTemp;
//            switch (this.stateTemp)
//            {
//                case 0:
//                    tempBlue.SetActive(false);
//                    tempRed.SetActive(false);
//                    tempGreen.SetActive(false);
//                    break;
//                case 1:
//                    tempBlue.SetActive(true);
//                    tempRed.SetActive(false);
//                    tempGreen.SetActive(false);
//                    break;
//                case 2:
//                    tempBlue.SetActive(false);
//                    tempRed.SetActive(false);
//                    tempGreen.SetActive(true);
//                    break;
//                case 3:
//                    tempBlue.SetActive(false);
//                    tempRed.SetActive(true);
//                    tempGreen.SetActive(false);
//                    break;
//            }
//        }*/
//    }
}
