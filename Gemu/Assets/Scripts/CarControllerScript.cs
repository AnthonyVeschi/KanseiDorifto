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
    public GameObject understeeringSlider;
    public GameObject understeeringMagnitudeSlider;
    GaugeSliderScript understeeringSliderScript;
    GaugeSliderScript understeeringMagnitudeSliderScript;
    Vector3 eulers;

    bool drifting;
    bool driftingInitialLerpFinished;
    public float driftingInitalLerpTime = 0.37f;
    float driftAngle;
    public float driftAngleCoef = 1f;
    public float driftAngleSteeringCoef = 0.5f;
    public float driftAngleVelocityCoef = 0.5f;
    public GameObject chasis;
    public float chasisDriftAngleCoef = 1.5f;
    float driftDecayRate;
    float driftGainRate;
    public float driftDecayVelocityCoef = 5f;
    public float driftGainVelocityCoef = 5f;
    public float driftDecaySteeringCoef = 0.4f;
    public float driftGainSteeringCoef = 0.4f;
    public float driftDecayCoef = 1f;
    public float driftGainCoef = 1f;
    public float minDriftAngle = 5f;
    bool driftIsPositive;

    public Text driftAngleText;
    public GameObject driftSlider;
    GaugeSliderScript driftSliderScript;
    public float maxDriftAngle = 45f;

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
        understeeringSliderScript = understeeringSlider.GetComponent<GaugeSliderScript>();
        understeeringMagnitudeSliderScript = understeeringMagnitudeSlider.GetComponent<GaugeSliderScript>();

        driftSliderScript = driftSlider.GetComponent<GaugeSliderScript>();

        drifting = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale == 1f) { Time.timeScale = 0.2f; }
            else { Time.timeScale = 1f; }
        }

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
                driftDecayRate = ((((maxV - v) / maxV) * driftDecayVelocityCoef * Mathf.Abs(steering)) + (Mathf.Abs(steering) * driftDecaySteeringCoef)) * driftDecayCoef;
                //driftGainRate = (((v / maxV) * driftGainVelocityCoef * Mathf.Abs(steering)) + (Mathf.Abs(steering) * driftGainSteeringCoef)) * driftGainCoef;
                Debug.Log("driftDecayVelocity: " + Mathf.Round(((((maxV - v) / maxV) * driftDecayVelocityCoef * Mathf.Abs(steering)) * 1000) / 1000) + "    driftDecaySteering: " + Mathf.Round(((Mathf.Abs(steering) * driftDecaySteeringCoef) * 1000) / 1000));
                if (driftIsPositive)
                {
                    if (steering >= 0) { driftAngle += driftGainRate; }
                    else { driftAngle -= driftDecayRate; }
                }
                else
                {
                    if (steering <= 0) { driftAngle -= driftGainRate; }
                    else { driftAngle += driftDecayRate; }
                }
                eulers = new Vector3(0f, 0f, (driftAngle * chasisDriftAngleCoef));
                chasis.transform.localEulerAngles = eulers;

                driftSliderScript.SetPosition(-driftAngle * (200 / maxDriftAngle));
                driftAngleText.text = "drift angle: " + Mathf.Round(driftAngle);
            }
        }
        else if (Input.GetButtonDown("Drift"))
        {
            drifting = true;
            driftAngle = ((driftAngleSteeringCoef * Mathf.Abs(steering)) + (driftAngleVelocityCoef * v)) * driftAngleCoef;
            driftAngle *= ((steering >= 0) ? 1 : -1);
            driftIsPositive = (driftAngle >= 0);
            driftAngleText.text = "drift angle: " + Mathf.Round(driftAngle);
            StartCoroutine("DriftingInitialLerp");
        }
        
        //carRotation = steering * x * steeringCoef * Time.deltaTime;
        //carRotation = understeering * x * steeringCoef * Time.deltaTime;
        transform.RotateAround(rearPivot.position, Vector3.forward, carRotation);
    }

    IEnumerator DriftingInitialLerp()
    {
        driftingInitialLerpFinished = false;
        float startTime = Time.time;
        float t = 0;
        float a;

        while (t <= driftingInitalLerpTime)
        {
            a = Mathf.SmoothStep(0f, (driftAngle * chasisDriftAngleCoef), (t / driftingInitalLerpTime));
            eulers = new Vector3(0f, 0f, a);
            chasis.transform.localEulerAngles = eulers;
            driftSliderScript.SetPosition(-(a / chasisDriftAngleCoef) * (200 / maxDriftAngle));
            t = Time.time - startTime;
            yield return null;
        }
        eulers = new Vector3(0f, 0f, (driftAngle * chasisDriftAngleCoef));
        chasis.transform.localEulerAngles = eulers;
        driftSliderScript.SetPosition(-driftAngle * (200 / maxDriftAngle));
        driftingInitialLerpFinished = true;
    }

    IEnumerator DriftingRecover()
    {
        yield return null;
    }

    void SteeringUI()
    {
        st.text = "s: " + Mathf.Round(steering);
        ust.text = "u: " + Mathf.Round(understeering);
        understeeringSliderScript.SetPosition(-(understeering * (200 / maxSteeringAngle)));
        understeeringMagnitudeSliderScript.SetPosition((1 - understeerFrac) * 400);
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
