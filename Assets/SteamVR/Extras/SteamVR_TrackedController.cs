//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using Valve.VR;
using VRTK;

public struct ClickedEventArgs
{
    public uint controllerIndex;
    public uint flags;
    public float padX, padY;
}

public delegate void ClickedEventHandler(object sender, ClickedEventArgs e);

public class SteamVR_TrackedController : MonoBehaviour
{
    public uint controllerIndex;
    public VRControllerState_t controllerState;
    public bool triggerPressed = false;
    public bool steamPressed = false;
    public bool menuPressed = false;
    public bool padPressed = false;
    public bool padTouched = false;
    public bool gripped = false;

    //public bool controlBallTouched = false;

    //public GameObject building;
    BuildingScript buildS;
    int smallMultipleNumbers;
    SmallMultiplesManagerScript mrs;
    // variable to store the last y value;
    float z = 0.0f;
    float zBeta = 0.0f;

    int farSight = 0;
    bool indirectTouched = false;

    float uniqueCenterZDelta = -100;


    public event ClickedEventHandler MenuButtonClicked;
    public event ClickedEventHandler MenuButtonUnclicked;
    public event ClickedEventHandler TriggerClicked;
    public event ClickedEventHandler TriggerUnclicked;
    public event ClickedEventHandler SteamClicked;
    public event ClickedEventHandler PadClicked;
    public event ClickedEventHandler PadUnclicked;
    public event ClickedEventHandler PadTouched;
    public event ClickedEventHandler PadUntouched;
    public event ClickedEventHandler Gripped;
    public event ClickedEventHandler Ungripped;

    GameObject MultipleManager;

    // rotate switch
    bool swipeToRotate = false;

    // Use this for initialization
    protected virtual void Start()
    {
        MultipleManager = GameObject.Find("SmallMultiplesManager");
        mrs = GameObject.Find("SmallMultiplesManager").GetComponent<SmallMultiplesManagerScript>();

        smallMultipleNumbers = mrs.smallMultiplesNumber;

        if (this.GetComponent<SteamVR_TrackedObject>() == null)
        {
            gameObject.AddComponent<SteamVR_TrackedObject>();
        }

        if (controllerIndex != 0)
        {
            this.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)controllerIndex;
            if (this.GetComponent<SteamVR_RenderModel>() != null)
            {
                this.GetComponent<SteamVR_RenderModel>().index = (SteamVR_TrackedObject.EIndex)controllerIndex;
            }
        }
        else
        {
            controllerIndex = (uint)this.GetComponent<SteamVR_TrackedObject>().index;
        }


    }

    public void SetDeviceIndex(int index)
    {
        this.controllerIndex = (uint)index;
    }

    public virtual void OnTriggerClicked(ClickedEventArgs e)
    {
        if (TriggerClicked != null)
            TriggerClicked(this, e);


    }

    public virtual void OnTriggerUnclicked(ClickedEventArgs e)
    {
        if (TriggerUnclicked != null)
            TriggerUnclicked(this, e);
    }

    void RecentrePosition(float distance)
    {
        GameObject viveCamera = GameObject.Find("Camera (eye)");
        /*float x = viveCamera.transform.position.x;
        float y = viveCamera.transform.position.y;
        float z = viveCamera.transform.position.z;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "testCube";
        cube.transform.position = viveCamera.transform.position + viveCamera.transform.forward * 3;*/

        Transform shelf = MultipleManager.transform.Find("Shelf");
        mrs.MoveShelfToCenter();

        //Vector3 originalShelfPos = shelf.transform.position;
        //bool underground = false;
        shelf.transform.position = viveCamera.transform.position + (viveCamera.transform.forward * distance) - (viveCamera.transform.up * 0.4f);
        //foreach (Transform child in shelf)
        //{
        //    if (child.position.y < 0)
        //    {
        //        underground = true;
        //    }
        //}
        //if (underground)
        //{
        //    shelf.transform.position = originalShelfPos;
        //}


        //Debug.Log(MultipleManager.transform.localPosition + " & " + viveCamera.transform.localPosition);
        Vector3 camPos = viveCamera.transform.position;
        Vector3 finalPos = new Vector3(camPos.x, shelf.transform.position.y, camPos.z);
        Vector3 offset = shelf.transform.position - finalPos;
        shelf.transform.LookAt(shelf.transform.position + offset);



        //transform.rotation = Quaternion.LookRotation(transform.position - camPos);
        //MultipleManager.transform.LookAt(2* finalPos - MultipleManager.transform.position);
        //Debug.Log(2 * finalPos - MultipleManager.transform.position);
        //MultipleManager.transform.RotateAround(Vector3.zero, Vector3.left, 0.5f);
        //Debug.Log(finalPos);
        //Debug.Log(MultipleManager.transform.position + " & " + viveCamera.transform.position);
    }


    public virtual void OnMenuClicked(ClickedEventArgs e)
    {
        if (MenuButtonClicked != null)
            MenuButtonClicked(this, e);

        if (this.name.Equals("Controller (left)"))
        {
            if (farSight == 0)
            {
                if (uniqueCenterZDelta > -1)
                {
                    RecentrePosition(0.1f);
                }
                else
                {
                    RecentrePosition(1f);
                }
                farSight++;
            }
            else if (farSight == 1)
            {
                RecentrePosition(2f);
                if (uniqueCenterZDelta > -1)
                {
                    farSight = 0;
                }
                else
                {
                    farSight++;
                }

            }
            else if (farSight == 2)
            {
                RecentrePosition(3f);
                farSight = 0;
            }
            else
            {
                Debug.Log("farSight Bug");
            }


        }
        if (this.name.Equals("Controller (right)"))
        {
            mrs.ToggleFaceCurve();
        }


    }

    public virtual void OnMenuUnclicked(ClickedEventArgs e)
    {
        if (MenuButtonUnclicked != null)
            MenuButtonUnclicked(this, e);
    }

    public virtual void OnSteamClicked(ClickedEventArgs e)
    {
        if (SteamClicked != null)
            SteamClicked(this, e);
    }

    public virtual void OnPadClicked(ClickedEventArgs e)
    {
        if (PadClicked != null)
            PadClicked(this, e);
        if (this.name.Equals("Controller (left)"))
        {
            for (int i = 1; i <= smallMultipleNumbers; i++)
            {
                GameObject building = GameObject.Find("Building " + i);
                buildS = building.transform.GetChild(0).gameObject.GetComponent<BuildingScript>();
                buildS.ChangeExplosion();
            }
        }
    }

    public virtual void OnPadUnclicked(ClickedEventArgs e)
    {
        if (PadUnclicked != null)
            PadUnclicked(this, e);
        if (this.name.Equals("Controller (right)"))
        {
            mrs.faceToCurve = true;
            mrs.ToggleFaceCurve();
        }
    }

    public virtual void OnPadTouched(ClickedEventArgs e)
    {
        if (PadTouched != null)
            PadTouched(this, e);
    }

    public virtual void OnPadUntouched(ClickedEventArgs e)
    {
        if (PadUntouched != null)
            PadUntouched(this, e);
    }

    public virtual void OnGripped(ClickedEventArgs e)
    {
        if (Gripped != null)
            Gripped(this, e);
        if (swipeToRotate)
        {
            z = transform.localPosition.x;
            zBeta = transform.localPosition.z;

        }
        else
        {
            z = transform.localRotation.eulerAngles.z;
        }
    }

    public virtual void OnUngripped(ClickedEventArgs e)
    {
        if (Ungripped != null)
            Ungripped(this, e);
    }

    void TouchNearestObject()
    {

        GameObject nearestObject = mrs.CalculateNearestTouchPoint(this.transform);

        //if (nearestObject.name.Equals("Control Ball"))
        //{
        //    controlBallTouched = true;
        //}
        //else
        //{
        //    controlBallTouched = false;
        //}

        if (nearestObject != null)
        {
            VRTK_InteractTouch IT = this.transform.GetChild(1).GetComponent<VRTK_InteractTouch>();
            IT.ForceTouch(nearestObject);
        }
    }


    // Update is called once per frame
    protected virtual void Update()
    {
        if (Input.GetKeyDown("space")) {
            Debug.Log("space");
        }

        this.uniqueCenterZDelta = mrs.uniqueCenterZDelta;
        if (mrs.indirectTouch)
        {
            TouchNearestObject();
            indirectTouched = true;
        }
        else
        {
            if (indirectTouched)
            {
                VRTK_InteractTouch IT = this.transform.GetChild(1).GetComponent<VRTK_InteractTouch>();
                IT.ForceStopTouching();
                indirectTouched = false;
            }

        }


        swipeToRotate = mrs.swipeToRotate;
        //Debug.Log (mrs.smallMultiplesNumber);
        // rotate with the controller
        GameObject swipeTooltip = this.transform.Find("SwipeToolTip").gameObject;
        GameObject rotationTooltip = this.transform.Find("RotationToolTip").gameObject;
        GameObject mainCamera = GameObject.Find("Camera (eye)");
        float facing = mainCamera.transform.rotation.eulerAngles.y;
        if (gripped)
        {
            float lastZ = z;
            float lastZBeta = zBeta;
            if (swipeToRotate)
            {
                z = transform.localPosition.x;
                zBeta = transform.localPosition.z;
                swipeTooltip.SetActive(true);
            }
            else
            {
                z = transform.localRotation.eulerAngles.z;
                rotationTooltip.SetActive(true);
            }
            float diff = z - lastZ;
            float diffBeta = zBeta - lastZBeta;
            for (int i = 1; i <= smallMultipleNumbers; i++)
            {
                GameObject building = GameObject.Find("Building " + i);
                //building.transform.RotateAround(Vector3.up, transform.rotation.y * Time.deltaTime);
                BuildingScript buildS = building.transform.GetChild(0).gameObject.GetComponent<BuildingScript>();
                Vector3 realCentre = buildS.getCentreCoordinates();
                //building.transform.eulerAngles = new Vector3(0, transform.localRotation.eulerAngles.z, 0);
                if (swipeToRotate)
                {
                    float finalDiff = 0.0f;
                    if (Mathf.Abs(diff) > Mathf.Abs(diffBeta))
                    {
                        finalDiff = diff;
                    }
                    else
                    {
                        finalDiff = -diffBeta;
                    }
                    if (facing >= 0 && facing < 180)
                    {
                        building.transform.RotateAround(realCentre, building.transform.up, -finalDiff * 1000);
                    }
                    else
                    {
                        building.transform.RotateAround(realCentre, building.transform.up, finalDiff * 1000);
                    }

                }
                else
                {
                    building.transform.RotateAround(realCentre, building.transform.up, diff);
                }

                //building.transform.Rotate(diff * building.transform.up *  Time.deltaTime);
            }
        }
        else
        {
            if (swipeToRotate)
            {
                swipeTooltip.SetActive(false);
            }
            else
            {
                rotationTooltip.SetActive(false);
            }
        }

        if (padPressed)
        {
            if (this.name.Equals("Controller (right)"))
            {
                mrs.faceToCurve = true;
                mrs.ToggleFaceCurve();
            }
        }

        var system = OpenVR.System;
        if (system != null && system.GetControllerState(controllerIndex, ref controllerState, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t))))
        {
            ulong trigger = controllerState.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Trigger));
            if (trigger > 0L && !triggerPressed)
            {
                triggerPressed = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnTriggerClicked(e);

            }
            else if (trigger == 0L && triggerPressed)
            {
                triggerPressed = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnTriggerUnclicked(e);
            }

            ulong grip = controllerState.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_Grip));
            if (grip > 0L && !gripped)
            {
                gripped = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnGripped(e);

            }
            else if (grip == 0L && gripped)
            {
                gripped = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnUngripped(e);
            }

            ulong pad = controllerState.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Touchpad));
            if (pad > 0L && !padPressed)
            {
                padPressed = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnPadClicked(e);
            }
            else if (pad == 0L && padPressed)
            {
                padPressed = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnPadUnclicked(e);
            }

            ulong menu = controllerState.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_ApplicationMenu));
            if (menu > 0L && !menuPressed)
            {
                menuPressed = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnMenuClicked(e);
            }
            else if (menu == 0L && menuPressed)
            {
                menuPressed = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnMenuUnclicked(e);
            }

            pad = controllerState.ulButtonTouched & (1UL << ((int)EVRButtonId.k_EButton_SteamVR_Touchpad));
            if (pad > 0L && !padTouched)
            {
                padTouched = true;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnPadTouched(e);

            }
            else if (pad == 0L && padTouched)
            {
                padTouched = false;
                ClickedEventArgs e;
                e.controllerIndex = controllerIndex;
                e.flags = (uint)controllerState.ulButtonPressed;
                e.padX = controllerState.rAxis0.x;
                e.padY = controllerState.rAxis0.y;
                OnPadUntouched(e);
            }
        }
    }
}
