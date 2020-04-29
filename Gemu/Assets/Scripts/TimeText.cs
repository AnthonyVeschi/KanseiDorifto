using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeText : MonoBehaviour
{
    int t;
    public Text myText;
    bool done = false;

    void Start()
    {
        t = 0;
        myText = gameObject.GetComponent<Text>();
    }

    void Update()
    {
        if (!done) { t = (int)Mathf.Round(Time.time); }
        myText.text = "Time: " + t;
    }

    public void Finish()
    {
        done = true;
    }
}
