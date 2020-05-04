using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinText : MonoBehaviour
{
    Text t;
    bool set;

    void Start()
    {
        t = gameObject.GetComponent<Text>();
        set = false;
    }

    public void SetText(int player)
    {
        if (!set)
        {
            t.text = "PLAYER " + player + "WINS!!!";
            set = true;
        }
    }
}
