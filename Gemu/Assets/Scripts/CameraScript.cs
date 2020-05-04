using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    void Update()
    {
        transform.eulerAngles = new Vector3(0f, 0f, 0f);
    }

    void LateUpdate()
    {
        transform.eulerAngles = new Vector3(0f, 0f, 0f);
    }
}
