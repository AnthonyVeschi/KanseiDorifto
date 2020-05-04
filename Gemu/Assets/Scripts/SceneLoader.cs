using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader: MonoBehaviour
{
    public int numPlayers;
    public int trackNum;

    public GameObject playersText;
    public GameObject trackText;
    Text pt;
    Text tt;

    public GameObject startButtonObj;
    StartButton startButtonScript;

    void Start()
    {
        numPlayers = 1;
        trackNum = 1;

        if (playersText) { pt = playersText.GetComponent<Text>(); }
        if (trackText) { tt = trackText.GetComponent<Text>(); }

        if (startButtonObj) { startButtonScript = startButtonObj.GetComponent<StartButton>(); }
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void HowTo()
    {
        SceneManager.LoadScene("HowTo");
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void Players()
    {
        if (numPlayers == 1) { numPlayers = 2; }
        else { numPlayers = 1; }
        pt.text = "Players: " + numPlayers;

        startButtonScript.SetP(numPlayers);
    }

    public void Track()
    {
        if (trackNum == 1) { trackNum = 2; }
        else { trackNum = 1; }
        tt.text = "Track: " + trackNum;

        startButtonScript.SetT(trackNum);
    }
}
