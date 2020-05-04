using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarControllerScript : MonoBehaviour
{
    float accel;
    float brake;
    float v;
    float v0;
    float x;
    float drag;
    public float dragFromGrass = 50f;
    float brakePlusDrag;
    public float accelCoef = 60f;
    public float brakeCoef = 40f;
    public float dragCoef = 0.005f;
    bool drivingOnGrass;
    int numTiresInGrass;
    public GameObject[] tiresForRaycasts;

    public Text vt;
    public Text at;
    public Text dt;
    public GameObject vSlider;
    public GameObject aSlider;
    public GameObject dSlider;
    GaugeSliderScript vSliderScript;
    GaugeSliderScript aSliderScript;
    GaugeSliderScript dSliderScript;
    public float maxV = 77f;
    public float maxA = 1f;
    public float maxD = 1f;

    float steering;
    float understeering;
    public float maxSteeringAngle = 35f;
    public float steeringCoef = 6f;
    public float minUndersteerV = 45f;
    public float minUndersteerAngle = 10f;
    public float understeerCoef = 1f;
    float maxUndersteerDiffV;
    float understeerDiffV;
    float understeerFrac;
    float carRotation;
    public Transform rearPivot;
    public Transform frontPivot;

    public GameObject forwardArrow;
    public GameObject steeringArrow;
    public GameObject understeeringArrow;
    public GameObject steeringManager;
    public Text st;
    public Text ust;
    SteeringManagerScript steeringManagerScript;
    public GameObject understeeringSlider;
    public GameObject understeeringMagnitudeSlider;
    GaugeSliderScript understeeringSliderScript;
    GaugeSliderScript understeeringMagnitudeSliderScript;
    Vector3 eulers;

    public GameObject chasis;

    public float DriftWindUpRate;
    public float DriftAngle;
    public float DriftAngleSteeringCoef;
    public float DriftMaxAngle = 135;
    public float DriftCounterSteerCoef;
    public bool DriftWindingUp = false;
    public bool DriftWindingDown = false;
    public bool DriftLeft = false;
    public bool DriftRight = false;
    bool justEnded = false;
    ParticleSystem particles;

    public float wrapAroundX = 43f;
    public float wrapAroundY = 25f;

    public bool sloMo;

    public AudioSource[] audioSources;

    public int player;

    void Start()
    {
        if (sloMo) { Time.timeScale = 0.2f; }

        v = 0;
        v0 = 0;
        accel = 0;
        x = 0;
        drag = 0;

        vSliderScript = vSlider.GetComponent<GaugeSliderScript>();
        aSliderScript = aSlider.GetComponent<GaugeSliderScript>();
        dSliderScript = dSlider.GetComponent<GaugeSliderScript>();

        steeringManagerScript = steeringManager.GetComponent<SteeringManagerScript>();
        understeeringSliderScript = understeeringSlider.GetComponent<GaugeSliderScript>();
        understeeringMagnitudeSliderScript = understeeringMagnitudeSlider.GetComponent<GaugeSliderScript>();

        audioSources = gameObject.GetComponents<AudioSource>();

        particles = gameObject.GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale == 1f) { Time.timeScale = 0.2f; }
            else { Time.timeScale = 1f; }
        }

        Accel();
        AccelUI();
        if (player == 1) { Steering1(); }
        else { Steering2(); }
        SteeringUI();
        WrapAround();
    }

    void Accel()
    {
        if (player == 1) { accel = Input.GetAxis("Gas1"); }
        else { accel = Input.GetAxis("Gas2"); }
        if (Time.timeSinceLevelLoad <= 3) { accel = 0; }
        if (player == 1) { brake = Input.GetAxis("Brake1"); }
        else { brake = Input.GetAxis("Brake1"); }

        drivingOnGrass = false;
        numTiresInGrass = 4;
        RaycastHit hit;
        foreach (GameObject tire in tiresForRaycasts)
        {
            if (Physics.Raycast(tire.transform.position, Vector3.forward, out hit))
            {
                if (hit.transform.gameObject.tag == "Track") { numTiresInGrass--; }
            }
        }
        if (numTiresInGrass >= 2) { drivingOnGrass = true; }

        v0 = v;
        accel = accel * accelCoef * Time.deltaTime;
        brake = brake * brakeCoef * Time.deltaTime;
        drag = ((v0 * v0) / 2) * dragCoef * Time.deltaTime;
        if (drivingOnGrass) 
        {
            drag *= dragFromGrass;
        }
        //if (drifting && (Mathf.Abs(driftAngle) >= maxDriftAngle))
        //{
        //    drag += (Mathf.Abs(driftAngle) - maxDriftAngle) * driftDragCoef;
        //}
        //Debug.Log(drag);
        brakePlusDrag = brake + drag;
        brakePlusDrag = Mathf.Min(brakePlusDrag, v0);
        v = v0 + accel - brakePlusDrag;
        x = v * Time.deltaTime;
        
        transform.Translate(Vector3.up * x);

        //if (accel > 0) { if (!audioSources[0].isPlaying) { audioSources[0].Play(); } } else { audioSources[0].Stop(); }
        //if (brake > 0 && x > 0) { if (!audioSources[1].isPlaying) { audioSources[1].Play(); } } else { audioSources[1].Stop(); }
        if (x > 0) { if (!audioSources[2].isPlaying) { audioSources[2].Play(); } } else { audioSources[2].Stop(); }
        audioSources[2].volume = v / maxV;
    }

    void AccelUI()
    {
        vt.text = "v: " + Mathf.Round(v);
        at.text = "a: " + Mathf.Round(accel);
        dt.text = "d: " + Mathf.Round(drag);
        vSliderScript.SetPosition((v / maxV) * 400);
        aSliderScript.SetPosition((accel / maxA) * 400);
        dSliderScript.SetPosition((drag / maxD) * 400);
    }

    void Steering1()
    {
        steering = steeringManagerScript.GetSteering();
        steering = (steering + 1) / 2;
        steering = Mathf.Lerp(maxSteeringAngle, -maxSteeringAngle, steering);

        Debug.Log("steering: " + steering);

        maxUndersteerDiffV = maxV - minUndersteerV;
        understeerDiffV = Mathf.Max(0, (v - minUndersteerV));
        understeerFrac = 1f - (understeerDiffV / maxUndersteerDiffV);
        understeering = Mathf.Max(Mathf.Min(minUndersteerAngle, Mathf.Abs(steering)), Mathf.Abs(steering * understeerFrac * understeerCoef));
        understeering *= ((steering >= 0) ? 1 : -1);

        if (((Input.GetButtonDown("DriftLeft1") && DriftLeft) || (Input.GetButtonDown("DriftRight1") && DriftRight)) && DriftWindingDown)
        {
            DriftWindingDown = false;
            StartCoroutine("DriftEndLerp");
            particles.Stop();
            justEnded = true;
            audioSources[3].Stop();
        }
        if (((Input.GetButtonDown("DriftLeft1") && DriftRight) || (Input.GetButtonDown("DriftRight1") && DriftLeft)) && DriftWindingDown)
        {
            DriftLeft = !DriftLeft;
            DriftRight = !DriftRight;
            StartCoroutine("InertiaDriftLerp");
        }
        if (((Input.GetButtonDown("DriftLeft1") && DriftLeft) || (Input.GetButtonDown("DriftRight1") && DriftRight)) && DriftWindingUp)
        {
            DriftWindingUp = false;
            DriftWindingDown = true;
            particles.Play();
            audioSources[3].Play();
        }
        if ((Input.GetButtonDown("DriftLeft1") || Input.GetButtonDown("DriftRight1")) && !DriftWindingDown && !DriftWindingUp && !justEnded)
        {
            DriftWindingUp = true;
            DriftAngle = 0f;
            if (Input.GetButtonDown("DriftLeft1")) { DriftLeft = true; DriftRight = false; }
            if (Input.GetButtonDown("DriftRight1")) { DriftLeft = false; DriftRight = true; }
        }
        if (DriftWindingUp)
        {
            if (DriftLeft) { DriftAngle += DriftWindUpRate * Time.deltaTime; }
            if (DriftRight) { DriftAngle -= DriftWindUpRate * Time.deltaTime; }
            eulers = new Vector3(0f, 0f, DriftAngle);
            chasis.transform.localEulerAngles = eulers;

            if (Mathf.Abs(DriftAngle) >= DriftMaxAngle)
            {
                DriftWindingUp = false;
                DriftWindingDown = true;
                particles.Play();
                audioSources[3].Play();
            }
        }
        
        if (!DriftWindingUp && !DriftWindingDown)
        {
            carRotation = understeering * x * steeringCoef * Time.deltaTime;
        }
        if (DriftWindingUp)
        {
            carRotation = 0f;
        }
        if (DriftWindingDown)
        {
            //carRotation = DriftAngle * DriftAngleSteeringCoef * Time.deltaTime;
            carRotation = ((DriftAngle * DriftAngleSteeringCoef) + (steering * DriftCounterSteerCoef)) * Time.deltaTime;
        }
        if (justEnded) { justEnded = false; }
        
        transform.RotateAround(rearPivot.position, Vector3.forward, carRotation);
    }

    void Steering2()
    {
        steering = steeringManagerScript.GetSteering();
        steering = (steering + 1) / 2;
        steering = Mathf.Lerp(maxSteeringAngle, -maxSteeringAngle, steering);

        maxUndersteerDiffV = maxV - minUndersteerV;
        understeerDiffV = Mathf.Max(0, (v - minUndersteerV));
        understeerFrac = 1f - (understeerDiffV / maxUndersteerDiffV);
        understeering = Mathf.Max(Mathf.Min(minUndersteerAngle, Mathf.Abs(steering)), Mathf.Abs(steering * understeerFrac * understeerCoef));
        understeering *= ((steering >= 0) ? 1 : -1);

        if (((Input.GetButtonDown("DriftLeft2") && DriftLeft) || (Input.GetButtonDown("DriftRight2") && DriftRight)) && DriftWindingDown)
        {
            DriftWindingDown = false;
            StartCoroutine("DriftEndLerp");
            particles.Stop();
            justEnded = true;
            audioSources[3].Stop();
        }
        if (((Input.GetButtonDown("DriftLeft2") && DriftRight) || (Input.GetButtonDown("DriftRight2") && DriftLeft)) && DriftWindingDown)
        {
            DriftLeft = !DriftLeft;
            DriftRight = !DriftRight;
            StartCoroutine("InertiaDriftLerp");
        }
        if (((Input.GetButtonDown("DriftLeft2") && DriftLeft) || (Input.GetButtonDown("DriftRight2") && DriftRight)) && DriftWindingUp)
        {
            DriftWindingUp = false;
            DriftWindingDown = true;
            particles.Play();
            audioSources[3].Play();
        }
        if ((Input.GetButtonDown("DriftLeft2") || Input.GetButtonDown("DriftRight2")) && !DriftWindingDown && !DriftWindingUp && !justEnded)
        {
            DriftWindingUp = true;
            DriftAngle = 0f;
            if (Input.GetButtonDown("DriftLeft2")) { DriftLeft = true; DriftRight = false; }
            if (Input.GetButtonDown("DriftRight2")) { DriftLeft = false; DriftRight = true; }
        }
        if (DriftWindingUp)
        {
            if (DriftLeft) { DriftAngle += DriftWindUpRate * Time.deltaTime; }
            if (DriftRight) { DriftAngle -= DriftWindUpRate * Time.deltaTime; }
            eulers = new Vector3(0f, 0f, DriftAngle);
            chasis.transform.localEulerAngles = eulers;

            if (Mathf.Abs(DriftAngle) >= DriftMaxAngle)
            {
                DriftWindingUp = false;
                DriftWindingDown = true;
                particles.Play();
                audioSources[3].Play();
            }
        }

        if (!DriftWindingUp && !DriftWindingDown)
        {
            carRotation = understeering * x * steeringCoef * Time.deltaTime;
        }
        if (DriftWindingUp)
        {
            carRotation = 0f;
        }
        if (DriftWindingDown)
        {
            //carRotation = DriftAngle * DriftAngleSteeringCoef * Time.deltaTime;
            carRotation = ((DriftAngle * DriftAngleSteeringCoef) + (steering * DriftCounterSteerCoef)) * Time.deltaTime;
        }
        if (justEnded) { justEnded = false; }

        transform.RotateAround(rearPivot.position, Vector3.forward, carRotation);
    }

    IEnumerator DriftEndLerp()
    {
        float startTime = Time.time;
        float totalTime = 0.15f;
        float myAngle;
        while (Time.time - startTime < totalTime)
        {
            myAngle = Mathf.Lerp(DriftAngle, 0, (Time.time - startTime) / totalTime);
            eulers = new Vector3(0f, 0f, myAngle);
            chasis.transform.localEulerAngles = eulers;
            if (DriftWindingUp) { yield break; }
            yield return null;
        }
        eulers = new Vector3(0f, 0f, 0f);
        chasis.transform.localEulerAngles = eulers;
    }

    IEnumerator InertiaDriftLerp()
    {
        float startTime = Time.time;
        float totalTime = 0.2f;
        float initialAngle = DriftAngle;
        while (Time.time - startTime < totalTime)
        {
            DriftAngle = Mathf.Lerp(initialAngle, -initialAngle, (Time.time - startTime) / totalTime);
            eulers = new Vector3(0f, 0f, DriftAngle);
            chasis.transform.localEulerAngles = eulers;
            if (!DriftWindingDown) { yield break; }
            yield return null;
        }
        DriftAngle = -initialAngle;
    }

    void SteeringUI()
    {
        st.text = "s: " + Mathf.Round(steering);
        ust.text = "u: " + Mathf.Round(understeering);
        understeeringSliderScript.SetPosition(-(understeering * (200 / maxSteeringAngle)));
        understeeringMagnitudeSliderScript.SetPosition((1 - understeerFrac) * 400);
        eulers = new Vector3(0f, 0f, steering);
        steeringArrow.transform.localEulerAngles = eulers;
        eulers = new Vector3(0f, 0f, understeering);
        understeeringArrow.transform.localEulerAngles = eulers;
    }

    void WrapAround()
    {
        if (transform.position.x >= wrapAroundX) { transform.position = new Vector3(-(wrapAroundX - 1), transform.position.y, transform.position.z); }
        if (transform.position.x <= -wrapAroundX) { transform.position = new Vector3((wrapAroundX - 1), transform.position.y, transform.position.z); }
        if (transform.position.y >= wrapAroundY) { transform.position = new Vector3(transform.position.x, -(wrapAroundY - 1), transform.position.z); }
        if (transform.position.y <= -wrapAroundY) { transform.position = new Vector3(transform.position.x, (wrapAroundY - 1), transform.position.z); }
    }
}
