using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationManager : MonoBehaviour
{
    float x;
    public GameObject gasGauge;
    GaugeSliderScript gasGaugeScript;

    public GameObject car;
    CarControllerScript carScript;

    void Start()
    {
        x = 0;
        gasGaugeScript = gasGauge.GetComponent<GaugeSliderScript>();
        carScript = car.GetComponent<CarControllerScript>();
    }

    void Update()
    {
        x = Input.GetAxis("Gas");
        x *= 400;
        gasGaugeScript.SetPosition(x);

        x /= 400;
        carScript.SetAcceleration(x);
    }
}
