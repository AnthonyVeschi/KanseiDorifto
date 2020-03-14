using System.Collections;
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
    float driftTargetAngle;
    float driftAngle;
    public float driftTargetAngleCoef = 1f;
    public float driftTargetAngleSteeringCoef = 0.5f;
    public float driftTargetAngleVelocityCoef = 0.5f;
    public GameObject chasis;
    public float chasisDriftAngleCoef = 1.5f;
    public float counterSteerCoef = 0.2f;
    public float driftOpenLerpTime = 0.5f;
    float driftPushOpen;
    public float driftPushOpenCoef = 0.2f;
    float driftPushClose;
    float driftPushCloseRate;
    public float driftPushCloseRateCoef = 1f;
    bool driftIsPositive;
    public float minDriftAngle = 5f;
    bool driftEnded;
    bool recovering;
    public float driftRecoverTime = 0.2f;

    public Text driftAngleText;
    public GameObject driftSlider;
    GaugeSliderScript driftSliderScript;
    public float maxDriftAngle = 45f;
    string state;

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
        
        if (Input.GetButtonDown("Drift") && !drifting)
        {
            drifting = true;
            recovering = false;
            driftAngle = 0;
            driftTargetAngle = ((driftTargetAngleSteeringCoef * Mathf.Abs(steering)) + (driftTargetAngleVelocityCoef * v)) * driftTargetAngleCoef;
            driftTargetAngle *= ((steering >= 0) ? 1 : -1);
            driftIsPositive = (driftTargetAngle >= 0);

            driftPushCloseRate = Mathf.Abs((Mathf.Max((maxDriftAngle - driftTargetAngle), 1) / maxDriftAngle) * driftPushCloseRateCoef);
            driftPushOpen = 0;
            driftPushClose = 0;
            StartCoroutine("DriftOpenLerp");
        }
        if (drifting)
        {
            if (!recovering)
            {
                driftAngle += (driftPushOpen - driftPushClose + (steering * counterSteerCoef)) * Time.deltaTime;
                eulers = new Vector3(0f, 0f, driftAngle);
                chasis.transform.localEulerAngles = eulers;

                driftAngleText.text = "DriftAngle: " + Mathf.Round(driftAngle);
                driftSliderScript.SetPosition(-driftAngle * (200 / maxDriftAngle));
            }
            Debug.Log("Push Open: " + (Mathf.Round(driftPushOpen * 1000) / 1000) + "    Push Close:" + (Mathf.Round(driftPushClose * 1000) / 1000) + "    Push: " + (Mathf.Round((driftPushOpen - driftPushClose) * 1000) / 1000));
        }
        else
        {
            //carRotation = understeering * x * steeringCoef * Time.deltaTime;
            transform.RotateAround(rearPivot.position, Vector3.forward, carRotation);
        }
    }

    IEnumerator DriftOpenLerp()
    {
        state = "OPEN";
        float startTime = Time.time;
        float t = Time.time;
        while (((t - startTime) <= driftOpenLerpTime))
        {
            driftPushOpen = Mathf.SmoothStep(0f, (driftTargetAngle * driftPushOpenCoef), ((t - startTime) / driftOpenLerpTime));
            t = Time.time;
            yield return null;
        }
        driftPushOpen = (driftTargetAngle * driftPushOpenCoef);
        driftEnded = false;
        StartCoroutine("DriftClose");
    }
    IEnumerator DriftClose()
    {
        state = "CLOSE";
        driftPushClose = 0f;
        while (!((Mathf.Abs(driftAngle) <= minDriftAngle) || (driftIsPositive != (driftAngle >= 0f))))
        {
            driftPushClose += driftPushCloseRate * Time.deltaTime;
            yield return null;
        }
        StartCoroutine("DriftRecoverLerp");
    }
    IEnumerator DriftRecoverLerp()
    {
        state = "RECOVER";
        recovering = true;
        float startTime = Time.time;
        float t = Time.time;
        while ((t - startTime) <= driftRecoverTime)
        {
            driftAngle = Mathf.Lerp(driftAngle, 0f, ((t - startTime) / driftRecoverTime));
            eulers = new Vector3(0f, 0f, driftAngle);
            chasis.transform.localEulerAngles = eulers;
            t = Time.time;
            yield return null;
        }
        state = "END";
        driftAngle = 0f;
        eulers = new Vector3(0f, 0f, driftAngle);
        chasis.transform.localEulerAngles = eulers;
        drifting = false;
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
