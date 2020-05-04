using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    int p;
    int t;

    void Start()
    {
        p = 1;
        t = 1;
    }

    public void SetP(int np)
    {
        p = np;
    }
    public void SetT(int nt)
    {
        t = nt;
    }

    public void StartLevel()
    {
        if (p == 1)
        {
            if (t == 1)
            {
                SceneManager.LoadScene("SingleTrack1");
            }
            else
            {
                SceneManager.LoadScene("SingleTrack2");
            }
        }
        else
        {
            if (t == 1)
            {
                SceneManager.LoadScene("MultiTrack1");
            }
            else
            {
                SceneManager.LoadScene("MultiTrack2");
            }
        }
    }
}
