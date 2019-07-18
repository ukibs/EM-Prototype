using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissiveController : MonoBehaviour
{
    #region attributes
    // PUBLIC
    public float timeToChangeColor;
    public float intensity = 2.0f;

    public Color initColor;
    public Color finalColor;  

    public bool active;

    // PRIVATE
    private float process;
    private float timer;

    private bool towardsFinal;

    private Material material;
    private Renderer ren;
    private Color lerpColor;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //active = false;
        towardsFinal = true;

        timer = timeToChangeColor;

        ren = gameObject.GetComponent<Renderer>();
        material = gameObject.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        // In case that run is true, enter the main loop process
        if(active)
        {
            // if the countdown ends, reset the timer and reverse lerp
            if (timer <= 0)
            {
                timer = timeToChangeColor;
                towardsFinal = !towardsFinal;
                process = 0.0f;
            }

            // lerp to "finalColor". Make regresive count and use the unitary value of "process" to control the color change
            if (towardsFinal)
            {
                // Debug.Log("Color is going towards FINAL, process state: " + process);
                lerpColor = Color.Lerp(initColor, finalColor, process);
                timer = timer - Time.deltaTime;
                process += Time.deltaTime / timeToChangeColor;
            }
            // lerp to "initColor". Make additive count and use the unitary value of "process" to control the color change
            else
            {
                // Debug.Log("Color is going towards INIT, process state: "+ process);
                lerpColor = Color.Lerp(finalColor, initColor, process);
                timer = timer - Time.deltaTime;
                process += Time.deltaTime / timeToChangeColor;
            }

            // set the emisive color as the "lerpColor" and multiply to generate some intensity
            material.SetColor("_EmissionColor", lerpColor * intensity);
        }       
    }
}