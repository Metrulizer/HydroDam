using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Updater : MonoBehaviour
{
    // Parent or Stranger(?) objects
    public Rigidbody character;
    public BoxCollider waterLevel;

    // Child objects
    public UnityEngine.UI.Text textUpperLeft;
    public UnityEngine.UI.Text textUpperRight;
    public UnityEngine.UI.Text textLowerLeft;
    public UnityEngine.UI.Text textLowerRight;

    public UnityEngine.UI.Text textBarrel;
    public UnityEngine.UI.Text textTris;
    private UnityEngine.UI.Text[] textTrisChildren; // <- assigned at runtime

    public Canvas canvasBarrel;
    public Canvas canvasTris;

    private bool showFPS = false;

    // Timer settings
    public float ticksPerSecond = 4.0f;
    private float start_time;

    // Rangefinder/Focus Settings
    private float range;
    private float rangeSmooth;

    // Speed Reference Settings
    //public float speedLowerBound = 2f;
    public float speedScaleUpperBound = 6f;
    public float speedDivisor = 1f; // formerly 400f

    // Lens smoothing settings
    private float smoothTime = 1.0f;
    private float zVelocity = 0.0f;
    private bool useAutoFocus = true;

    void Start()
    {
        float tickPeriod = 1 / ticksPerSecond;
        start_time = Time.time;
        textTrisChildren = textTris.GetComponentsInChildren<UnityEngine.UI.Text>();
        // Start an text updater loop on a slow tickrate
        StartCoroutine(Ticker(tickPeriod));
        StartCoroutine(AutoFocus());
    }

    private void Update()
    {
        // Align barrel direction_ by overwriting rotation each frame
        textBarrel.rectTransform.rotation = character.transform.rotation;
        //float range = Mathf.Clamp(GetRangeFloat() / 50f - .02f, 0f, .2f);

        // Scale close proximity rangefinder_ by overwriting z. localScale is a property so the whole vector is overwritten 
        range = GetRangeFloat();
        rangeSmooth = Mathf.SmoothStep(0f, .2f, range / 20f);    // Smoothstep looks marginally better than lerp, I can barely tell though
        textTris.rectTransform.localScale = new Vector3(0.4f, 0.4f, rangeSmooth);

        // Set speed reference_ by modifying linespacing in text for each child. It's not a speedometer.
        float speed = //Mathf.Clamp(character.velocity.magnitude * .5f, 2f, 10f);
                      //Mathf.Clamp(
                Mathf.SmoothStep(2f, speedScaleUpperBound, character.velocity.sqrMagnitude / speedDivisor);
                //, 2f, 10f);  // sqrMagnitude is said to be less resource intensive and the curve is nice
        foreach (UnityEngine.UI.Text child in textTrisChildren)
        {
            child.lineSpacing = speed;
        }

        // Disable auto focus
        if (Input.GetKeyDown(KeyCode.F3))
        {
            useAutoFocus ^= true;
            canvasBarrel.planeDistance = 20f;
            canvasTris.planeDistance = 20f;

            if (useAutoFocus)   StartCoroutine(AutoFocus());
            else                StopCoroutine(AutoFocus());

            Debug.Log("F3: Auto Focus = " + useAutoFocus);
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            showFPS ^= true;
        }
    }

    // Autofocus with smoothing effect to decrease eye strain
    IEnumerator AutoFocus()
    {
        while (true)
        {
            // https://docs.unity3d.com/ScriptReference/Mathf.SmoothDamp.html
            float npd = canvasBarrel.planeDistance / 20f;
            smoothTime = Mathf.Lerp(.5f, 0.01f, npd);   // 20f being a rate modifier that remaps 0-20m to 0-1, higher is slower
            //Debug.Log("SmoothTime = " + smoothTime + " from newpd = " + npd);
            float pd = Mathf.SmoothDamp(canvasBarrel.planeDistance, (rangeSmooth * 100f) + 0.5f, ref zVelocity, smoothTime);

            // plane distance scaling is painful when instantaneous. Human eye takes time to refocus. 
            canvasBarrel.planeDistance = pd;
            canvasTris.planeDistance = pd;

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    IEnumerator Ticker(float f)
    {
        while (true)
        {
            TickUpdate();
            yield return new WaitForSeconds(f);
        }
    }

    // Update data panels
    void TickUpdate()
    {
        textUpperLeft.text = (showFPS ? (int)(1 / Time.deltaTime) + " FPS\n" : "")
            + Decimate(Time.time - start_time) + "s TM'";
        textUpperRight.text = GetRangeString();
        textLowerRight.text =
            "NPS " + (int)(0f) + " ppm"   // could be NO3 or NH4 for actual component, or NPS for nonpoint source pollutuion
            + '\n' +
            "MUD " + Decimate(0f) + " kg"    // Mud seems silly but there aren't many abbreviations I can think of
            + '\n' +
            "GHG " + (int)0f + " pc"         // CH4 Methane as the big bad, but CO2 is also made. Could be GHG for greenhouse gas
            + '\n' +
            "EP' " + (int)0f + " pc"         // Engineering Plastics. There are too many types of plastic and no good umbrella abbreviation.
            ;
        textLowerLeft.text =
            Decimate(character.velocity.magnitude) + "m/s SPD"
            + '\n' +
            GetBearing() + '°' + " BRG"
            + '\n' +
            Decimate(waterLevel.bounds.max.y - transform.position.y) + "m DEP"
            + '\n' +    // IF WATER LEVEL IS UNASSIGNED THE TICKER WILL CRASH
            "!!!"
            ;
    }

    string Decimate(float f)
    {
        return ((f * 100.0f) / 100.0f).ToString("n2");
    }

    string GetBearing()
    {
        float ans = Mathf.Repeat(Mathf.Atan2(character.transform.forward.x, character.transform.forward.z) * Mathf.Rad2Deg, 360f);
        return Decimate(ans);
    }

    float GetRangeFloat()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 2;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.cyan);
            return hit.distance;
        }
        else
            return Mathf.Infinity;
    }

    // Tag reporting is a requirement, thus this near-duplicate that returns a string
    //https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
    string GetRangeString()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 2;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.cyan);
            //Debug.Log("Did Hit for " + hit.distance);
            string tag = (hit.collider.CompareTag("Untagged")) ? "" : hit.collider.tag;
            return tag + "\nRNG " + Decimate(hit.distance) + "m";
        }
        else
            return "RNG NaN";
    }
}