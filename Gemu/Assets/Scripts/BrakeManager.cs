using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakeManager : MonoBehaviour
{
    float b;
    public GameObject brakeGauge;
    GaugeSliderScript brakeGaugeScript;

    void Start()
    {
        b = 0;
        brakeGaugeScript = brakeGauge.GetComponent<GaugeSliderScript>();
    }

    void Update()
    {
        b = Input.GetAxis("Brake1");
        b *= 400;
        brakeGaugeScript.SetPosition(b);
    }
}
