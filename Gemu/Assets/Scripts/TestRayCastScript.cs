using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRayCastScript : MonoBehaviour
{

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.forward, out hit))
        {
            if (hit.transform.gameObject.tag == "Track") { Debug.Log("I Am On The Track"); }
        }
    }

}
