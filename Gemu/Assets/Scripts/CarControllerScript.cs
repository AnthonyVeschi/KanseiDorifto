﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarControllerScript : MonoBehaviour
{
    float accel;
    float brake;
    float v;
    float v0;
    float x;
    float drag;
    float brakePlusDrag;
    public float accelCoef = 60f;
    public float brakeCoef = 40f;
    public float dragCoef = 0.005f;

    public Text vt;
    public Text at;
    public Text dt;
    public GameObject vSlider;
    public GameObject aSlider;
    public GameObject dSlider;
    GaugeSliderScript vSliderScript;
    GaugeSliderScript aSliderScript;
    GaugeSliderScript dSliderScript;
    public float maxV = 77f;
    public float maxA = 1f;
    public float maxD = 1f;

    float steering;
    float understeering;
    public float maxSteeringAngle = 35f;
    public float steeringCoef = 6f;
    public float minUndersteerV = 45f;
    public float minUndersteerAngle = 10f;
    public float understeerCoef = 1f;
    float maxUndersteerDiffV;
    float understeerDiffV;
    float understeerFrac;
    float carRotation;
    public Transform rearPivot;
    public Transform frontPivot;

    public GameObject forwardArrow;
    public GameObject steeringArrow;
    public GameObject understeeringArrow;
    public GameObject steeringManager;
    public Text st;
    public Text ust;
    SteeringManagerScript steeringManagerScript;
    public GameObject UndersteeringSlider;
    public GameObject UndersteeringMagnitudeSlider;
    GaugeSliderScript UndersteeringSliderScript;
    GaugeSliderScript UndersteeringMagnitudeSliderScript;
    Vector3 eulers;

    bool drifting;
    bool driftingInitialLerpFinished;
    public float driftingInitalLerpTime = 0.5f;
    float driftAngle;
    public float driftAngleCoef = 1f;
    public float driftAngleSteeringCoef = 0.5f;
    public float driftAngleVelocityCoef = 0.5f;
    public GameObject chasis;
    public float chasisDriftAngleCoef = 2f;
    public Text driftAngleText;

    public float wrapAroundX = 43f;
    public float wrapAroundY = 25f;

    public bool sloMo;

    void Start()
    {
        if (sloMo) { Time.timeScale = 0.2f; }

        v = 0;
        v0 = 0;
        accel = 0;
        x = 0;
        drag = 0;

        vSliderScript = vSlider.GetComponent<GaugeSliderScript>();
        aSliderScript = aSlider.GetComponent<GaugeSliderScript>();
        dSliderScript = dSlider.GetComponent<GaugeSliderScript>();

        steeringManagerScript = steeringManager.GetComponent<SteeringManagerScript>();
        UndersteeringSliderScript = UndersteeringSlider.GetComponent<GaugeSliderScript>();
        UndersteeringMagnitudeSliderScript = UndersteeringMagnitudeSlider.GetComponent<GaugeSliderScript>();

        drifting = false;
    }

    void Update()
    {
        Accel();
        AccelUI();
        Steering();
        SteeringUI();
        WrapAround();
    }

    void Accel()
    {
        accel = Input.GetAxis("Gas");
        brake = Input.GetAxis("Brake");

        v0 = v;
        accel = accel * accelCoef * Time.deltaTime;
        brake = brake * brakeCoef * Time.deltaTime;
        drag = ((v0 * v0) / 2) * dragCoef * Time.deltaTime;
        brakePlusDrag = brake + drag;
        brakePlusDrag = Mathf.Min(brakePlusDrag, v0);
        v = v0 + accel - brakePlusDrag;
        x = v * Time.deltaTime;
        transform.Translate(Vector3.up * x);
    }

    void AccelUI()
    {
        vt.text = "v: " + Mathf.Round(v);
        at.text = "a: " + Mathf.Round(accel);
        dt.text = "d: " + Mathf.Round(drag);
        vSliderScript.SetPosition((v / maxV) * 400);
        aSliderScript.SetPosition((accel / maxA) * 400);
        dSliderScript.SetPosition((drag / maxD) * 400);
    }

    void Steering()
    {
        steering = steeringManagerScript.GetSteering();
        steering = (steering + 1) / 2;
        steering = Mathf.Lerp(maxSteeringAngle, -maxSteeringAngle, steering);

        maxUndersteerDiffV = maxV - minUndersteerV;
        understeerDiffV = Mathf.Max(0, (v - minUndersteerV));
        understeerFrac = 1f - (understeerDiffV / maxUndersteerDiffV);
        understeering = Mathf.Max(Mathf.Min(minUndersteerAngle, Mathf.Abs(steering)), Mathf.Abs(steering * understeerFrac * understeerCoef));
        understeering *= ((steering >= 0) ? 1 : -1);
        
        if (drifting)
        {
            //handle drifting controls and stop drifting
            if (driftingInitialLerpFinished)
            {

            }
        }
        else if (Input.GetButtonDown("Drift"))
        {
            Debug.Log("FUCK");
            drifting = true;
            driftAngle = ((driftAngleSteeringCoef * steering) + (driftAngleVelocityCoef * v)) * driftAngleCoef;
            driftAngleText.text = "drift angle: " + Mathf.Round(driftAngle);
            StartCoroutine("DriftingInitialLerp");
        }
        
        //carRotation = steering * x * steeringCoef * Time.deltaTime;
        carRotation = understeering * x * steeringCoef * Time.deltaTime;
        transform.RotateAround(rearPivot.position, Vector3.forward, carRotation);
    }

    IEnumerator DriftingInitialLerp()
    {
        driftingInitialLerpFinished = false;
        float startTime = Time.deltaTime;
        float t = 0;
        float a;

        while (t <= driftingInitalLerpTime)
        {
            a = Mathf.SmoothStep(0f, driftAngle, t);
            eulers = new Vector3(0f, 0f, (a * chasisDriftAngleCoef));
            chasis.transform.localEulerAngles = eulers;
            t = Time.deltaTime - startTime;
            yield return null;
        }
        eulers = new Vector3(0f, 0f, (driftAngle * chasisDriftAngleCoef));
        driftingInitialLerpFinished = true;
    }

    void SteeringUI()
    {
        st.text = "s: " + Mathf.Round(steering);
        ust.text = "u: " + Mathf.Round(understeering);
        UndersteeringSliderScript.SetPosition(-(understeering * (200 / maxSteeringAngle)));
        UndersteeringMagnitudeSliderScript.SetPosition((1 - understeerFrac) * 400);
        eulers = new Vector3(0f, 0f, steering);
        steeringArrow.transform.localEulerAngles = eulers;
        eulers = new Vector3(0f, 0f, understeering);
        understeeringArrow.transform.localEulerAngles = eulers;
    }

    void WrapAround()
    {
        if (transform.position.x >= wrapAroundX) { transform.position = new Vector3(-(wrapAroundX - 1), transform.position.y, transform.position.z); }
        if (transform.position.x <= -wrapAroundX) { transform.position = new Vector3((wrapAroundX - 1), transform.position.y, transform.position.z); }
        if (transform.position.y >= wrapAroundY) { transform.position = new Vector3(transform.position.x, -(wrapAroundY - 1), transform.position.z); }
        if (transform.position.y <= -wrapAroundY) { transform.position = new Vector3(transform.position.x, (wrapAroundY - 1), transform.position.z); }
    }
}
