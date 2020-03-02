using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireRotateScript : MonoBehaviour
{
    public float maxSteeringAngle = 65f;
    Quaternion r;
    Quaternion maxVector;
    Quaternion negMaxVector;

    void Start()
    {
        r = Quaternion.Euler(0, 0, 0);
        maxVector = Quaternion.Euler(0, 0, maxSteeringAngle);
        negMaxVector = Quaternion.Euler(0, 0, -maxSteeringAngle);
    }

    public void Rotate(float s)
    {
        s = (s + 1) / 2;

        foreach (Transform tire in transform)
        {
            r = Quaternion.Slerp(maxVector, negMaxVector, s);
            tire.transform.localRotation = r;
        }
    }
}
