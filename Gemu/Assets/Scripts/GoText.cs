﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoText : MonoBehaviour
{
    int t;
    public Text myText;
    bool go = false;

    void Start()
    {
        t = 3;
        myText = gameObject.GetComponent<Text>();
        myText.text = "" + t;
    }

    void Update()
    {
        if (Time.time >= 1 && t == 3) { t = 2; myText.text = "" + t; }
        if (Time.time >= 2 && t == 2) { t = 1; myText.text = "" + t; }
        if (Time.time >= 3 && t == 1) { t = 0; myText.text = "GO"; }
        if (Time.time >= 4 && t == 0) { t = 2; myText.text = ""; }
    }
}