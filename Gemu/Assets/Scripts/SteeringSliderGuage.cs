using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringSliderGuage : MonoBehaviour
{
    float x;
    float s;

    public float maxSpeed = 1400f;
    float diff;
    float realSpeed;

    public float easeStrength = 0.15f;
    float newS;

    public bool steeringIsFlat;

    void Start()
    {
        Time.timeScale = 0.2f;
        s = 0f;
    }

    void Update()
    {
        x = Input.GetAxis("Horizontal");
        x *= 200;

        diff = Mathf.Abs(x - s);

        if (steeringIsFlat) { Flat(); } else { Smooth(); }
        transform.localPosition = new Vector3(s, 0f, 1f);
    }

    void Flat()
    {
        realSpeed = Mathf.Min(diff, maxSpeed * Time.deltaTime);
        realSpeed = realSpeed * ((x >= s) ? (1) : (-1));
        s += realSpeed;
    }

    void Smooth()
    {
        newS = Mathf.Lerp(s, x, easeStrength);
        newS -= s;
        if (Mathf.Abs(newS) >= 0.1f && Mathf.Abs(newS) <= 7f) { newS = ((newS > 0) ? (7f) : (-7f)); }
        newS *= Time.deltaTime * 60;
        if (Mathf.Abs(newS) >= Mathf.Abs(diff))
        {
            s = x;
        }
        else
        {
            s += newS;
        }
    }
}
