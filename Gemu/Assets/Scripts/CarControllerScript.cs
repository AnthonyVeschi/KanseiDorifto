using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarControllerScript : MonoBehaviour
{
    float accel;
    float brake;
    public float accelCoef = 100f;
    public float brakeCoef = 100f;

    public Text t;
    float v;

    Rigidbody2D rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.AddForce(transform.up * accelCoef * accel);
        rb.AddForce(-transform.up * brakeCoef * brake);

        v = rb.velocity.magnitude;
        t.text = "Velocity: " + v;
    }

    public void SetAcceleration(float a)
    {
        accel = a;
    }
    public void SetBraking(float b)
    {
        brake = b;
    }
}
