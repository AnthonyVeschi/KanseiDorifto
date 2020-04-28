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
    public float dragFromGrass = 50f;
    float brakePlusDrag;
    public float accelCoef = 60f;
    public float brakeCoef = 40f;
    public float dragCoef = 0.005f;
    bool drivingOnGrass;
    int numTiresInGrass;
    public GameObject[] tiresForRaycasts;

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

    public GameObject chasis;

    public float DriftWindUpRate;
    public float DriftWindDownRate;
    public float DriftAngle;
    public float DriftAngleSteeringCoef;
    public bool DriftWindingUp;
    public bool DriftWindingDown = false;
    public bool DriftLeft;
    public bool DriftRight;

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

        //driftSliderScript = driftSlider.GetComponent<GaugeSliderScript>();

        //drifting = false;
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

        drivingOnGrass = false;
        numTiresInGrass = 4;
        RaycastHit hit;
        foreach (GameObject tire in tiresForRaycasts)
        {
            if (Physics.Raycast(tire.transform.position, Vector3.forward, out hit))
            {
                if (hit.transform.gameObject.tag == "Track") { numTiresInGrass--; }
            }
        }
        if (numTiresInGrass >= 2) { drivingOnGrass = true; }

        v0 = v;
        accel = accel * accelCoef * Time.deltaTime;
        brake = brake * brakeCoef * Time.deltaTime;
        drag = ((v0 * v0) / 2) * dragCoef * Time.deltaTime;
        if (drivingOnGrass) 
        {
            //drag *= dragFromGrass;
        }
        //if (drifting && (Mathf.Abs(driftAngle) >= maxDriftAngle))
        //{
        //    drag += (Mathf.Abs(driftAngle) - maxDriftAngle) * driftDragCoef;
        //}
        //Debug.Log(drag);
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

        if ((Input.GetButtonDown("DriftLeft") || Input.GetButtonDown("DriftRight")) && !DriftWindingDown)
        {
            DriftWindingUp = true;
            DriftAngle = 0f;
            if (Input.GetButtonDown("DriftLeft")) { DriftLeft = true; DriftRight = false; }
            if (Input.GetButtonDown("DriftRight")) { DriftLeft = false; DriftRight = true; }
        }
        if (((Input.GetButton("DriftLeft") && DriftLeft) || (Input.GetButton("DriftRight") && DriftRight)) && DriftWindingUp)
        {
            if (Input.GetButton("DriftLeft")) { DriftAngle += DriftWindUpRate * Time.deltaTime; }
            if (Input.GetButton("DriftRight")) { DriftAngle -= DriftWindUpRate * Time.deltaTime; }
        }
        if (((!Input.GetButton("DriftLeft") && DriftLeft) || (!Input.GetButton("DriftRight") && DriftRight)) && DriftWindingUp)
        {
            DriftWindingUp = false;
            DriftWindingDown = true;
        }

        if (DriftWindingUp)
        {
            eulers = new Vector3(0f, 0f, DriftAngle);
            chasis.transform.localEulerAngles = eulers;
        }
        if (DriftWindingDown)
        {
            if (DriftLeft) { DriftAngle -= DriftWindDownRate * Time.deltaTime; }
            if (DriftRight) { DriftAngle += DriftWindDownRate * Time.deltaTime; }

            eulers = new Vector3(0f, 0f, DriftAngle);
            chasis.transform.localEulerAngles = eulers;

            if (DriftLeft)
            {
                if (DriftAngle <= 0) 
                {
                    eulers = new Vector3(0f, 0f, 0f);
                    chasis.transform.localEulerAngles = eulers;
                    DriftWindingDown = false;
                }
            }
            if (DriftRight)
            {
                if (DriftAngle >= 0)
                {
                    eulers = new Vector3(0f, 0f, 0f);
                    chasis.transform.localEulerAngles = eulers;
                    DriftWindingDown = false;
                }
            }
        }
        if (!DriftWindingDown)
        {
            carRotation = understeering * x * steeringCoef * Time.deltaTime;
        }
        if (DriftWindingDown)
        {
            carRotation = (understeering + (DriftAngle * DriftAngleSteeringCoef)) * x * steeringCoef * Time.deltaTime;
        }
        
        transform.RotateAround(rearPivot.position, Vector3.forward, carRotation);
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
