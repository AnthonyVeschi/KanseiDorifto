using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaugeSliderScript : MonoBehaviour
{
    public void SetPosition(float s)
    {
        transform.localPosition = new Vector3(s, 0f, 1f);
    }
}
