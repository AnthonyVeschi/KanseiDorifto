using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSliderScript : MonoBehaviour
{
    float x;

    void Update()
    {
        x = Input.GetAxis("Horizontal");
        x *= 200;
        transform.localPosition = new Vector3(x, 0f, 1f);
    }
}
