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
    public float maxV = 155f;
    public float maxA = 1f;
    public float maxD = 1f;

    float steering;
    public float maxSteeringAngle = 65f;
    public Transform rearPivot;
    public Transform frontPivot;

    public GameObject forwardArrow;
    public GameObject steeringArrow;
    public GameObject steeringManager;
    SteeringManagerScript steeringManagerScript;

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
    }

    void Update()
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

        vt.text = "v: " + Mathf.Round(v);
        at.text = "a: " + Mathf.Round(accel);
        dt.text = "d: " + Mathf.Round(drag);
        vSliderScript.SetPosition((v / maxV) * 400);
        aSliderScript.SetPosition((accel / maxA) * 400);
        dSliderScript.SetPosition((drag / maxD) * 400);


        steering = steeringManagerScript.GetSteering();
        steering = Mathf.Lerp(-maxSteeringAngle, maxSteeringAngle, steering);

        //steeringArrow.transform.Rotate(Vector3.forward, )


        ///*
        if (transform.position.x >= 69) { transform.position = new Vector3(-68f, transform.position.y, transform.position.z); }
        if (transform.position.x <= -69) { transform.position = new Vector3(68f, transform.position.y, transform.position.z); }
        if (transform.position.y >= 39) { transform.position = new Vector3(transform.position.x, -38f, transform.position.z); }
        if (transform.position.y <= -39) { transform.position = new Vector3(transform.position.x, 38f, transform.position.z); }
        //*/
    }
}
