﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringManagerScript : MonoBehaviour
{
    float s;
    float x;
    float sFlat;
    float sSmooth;

    public float maxSpeedFlat = 1400f;
    public float minSpeedSmooth = 7f;
    public float smoothCoef = 60f;
    public float easeStrength = 0.15f;
    float diffFlat;
    float diffSmooth;
    float realSpeed;
    float newS;

    public bool steeringIsRaw;
    public bool steeringIsFlat;

    public GameObject RawGauge;
    public GameObject FlatGauge;
    public GameObject SmoothGauge;
    GaugeSliderScript rawGaugeScript;
    GaugeSliderScript flatGaugeScript;
    GaugeSliderScript smoothGaugeScript;

    public GameObject Tires;
    TireRotateScript tireScript;

    void Start()
    {
        sFlat = 0f;
        sSmooth = 0f;

        rawGaugeScript = RawGauge.GetComponent<GaugeSliderScript>();
        flatGaugeScript = FlatGauge.GetComponent<GaugeSliderScript>();
        smoothGaugeScript = SmoothGauge.GetComponent<GaugeSliderScript>();

        tireScript = Tires.GetComponent<TireRotateScript>();
    }

    void Update()
    {
        x = Input.GetAxis("Horizontal");
        x *= 200;

        Raw();
        Flat();
        Smooth();

        if (steeringIsRaw) { s = x / 200; }
        else { if (steeringIsFlat) { s = sFlat / 200; } else { s = sSmooth / 200; } }

        tireScript.Rotate(s);
    }

    void Raw()
    {
        rawGaugeScript.SetPosition(x);
    }

    void Flat()
    {
        diffFlat = Mathf.Abs(x - sFlat);
        realSpeed = Mathf.Min(diffFlat, maxSpeedFlat * Time.deltaTime);
        realSpeed = realSpeed * ((x >= sFlat) ? (1) : (-1));
        sFlat += realSpeed;

        flatGaugeScript.SetPosition(sFlat);
    }

    void Smooth()
    {
        diffSmooth = Mathf.Abs(x - sSmooth);
        newS = Mathf.Lerp(sSmooth, x, easeStrength);
        newS -= sSmooth;
        if (Mathf.Abs(newS) >= 0.1f && Mathf.Abs(newS) <= minSpeedSmooth) { newS = ((newS > 0) ? (minSpeedSmooth) : (-minSpeedSmooth)); }
        newS *= Time.deltaTime * smoothCoef;
        if (Mathf.Abs(newS) >= Mathf.Abs(diffSmooth))
        {
            sSmooth = x;
        }
        else
        {
            sSmooth += newS;
        }

        smoothGaugeScript.SetPosition(sSmooth);
    }
}
