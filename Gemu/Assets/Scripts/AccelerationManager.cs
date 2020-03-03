using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationManager : MonoBehaviour
{
    float x;
    public GameObject gasGauge;
    GaugeSliderScript gasGaugeScript;

    void Start()
    {
        x = 0;
        gasGaugeScript = gasGauge.GetComponent<GaugeSliderScript>();
    }

    void Update()
    {
        x = Input.GetAxis("Gas");
        x *= 400;
        gasGaugeScript.SetPosition(x);
    }
}
