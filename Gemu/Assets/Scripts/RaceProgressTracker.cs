using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceProgressTracker : MonoBehaviour
{
    public int tracker = 0;
    public int laps = 0;

    public GameObject textObj;
    public TimeText textScript;

    void Start()
    {
        textScript = textObj.GetComponent<TimeText>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "one" && tracker == 0) { tracker++; }
        if (col.gameObject.tag == "two" && tracker == 1) { tracker++; }
        if (col.gameObject.tag == "three" && tracker == 2) { tracker++; }
        if (col.gameObject.tag == "four" && tracker == 3) { tracker++; }
        if (col.gameObject.tag == "finish" && tracker == 4) { tracker = 0; laps++; }
        if (col.gameObject.tag == "finish" && laps == 3) { textScript.Finish(); }

    }
}
