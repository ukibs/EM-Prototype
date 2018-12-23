using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    public int direction = -1;
    public float speed = 1;
    public float alpha = 1;
    public Texture fadeTexture;
    //public Color fadeColor;

    //private Image

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        alpha += direction * speed * dt;
        alpha = Mathf.Clamp01(alpha);

        
    }

    private void OnGUI()
    {
        GUI.color = new Color(1, 1, 1, alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
        //GUI.DrawTexture();
    }
}
