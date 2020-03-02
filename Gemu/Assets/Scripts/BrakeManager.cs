using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakeManager : MonoBehaviour
{
    float b;
    public GameObject brakeGauge;
    GaugeSliderScript brakeGaugeScript;

    public GameObject car;
    CarControllerScript carScript;

    void Start()
    {
        b = 0;
        brakeGaugeScript = brakeGauge.GetComponent<GaugeSliderScript>();
        carScript = car.GetComponent<CarControllerScript>();
    }

    void Update()
    {
        b = Input.GetAxis("Brake");
        b *= 400;
        brakeGaugeScript.SetPosition(b);

        b /= 400;
        carScript.SetBraking(b);
    }
}
