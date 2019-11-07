using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetControl : MonoBehaviour
{
    public Transform[] levels;
    private InputManager inputManager;
    private int lvlIndex;
    private Transform cameraTransform;
    // Start is called before the first frame update
    void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
        cameraTransform = Camera.main.transform;
        AlignToLvl();
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        PerLevelMovement(dt);
        //ManualMovement(dt);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "Current planet: " + lvlIndex);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(levels[lvlIndex].position, 1);
    }
    void AlignToLvl()
    {
        Transform currentLvl = levels[lvlIndex];
        /*
        Vector3 levelDir = currentLvl.position - transform.position;
        Vector3 levelDirRev = transform.position - currentLvl.position;
        //Vector3 cameraDir = transform.position - cameraTransform.position;

        transform.LookAt(transform.position + levelDirRev);

        Quaternion levelRotation = Quaternion.LookRotation(levelDir);*/

        Vector3 lvlPosition = currentLvl.position;

        transform.LookAt(lvlPosition);
        //Quaternion cameraRotation = Quaternion.LookRotation(cameraDir);

        //float angle = Vector3.Angle(levelDir, cameraDir);

        //transform.rotation = Quaternion.RotateTowards(levelRotation, cameraRotation, angle);
    }

    void PerLevelMovement(float dt)
    {
        Vector2 leftAxis = inputManager.StickAxis;
        if (Mathf.Abs(leftAxis.y) > 0.01)
        {
            
            lvlIndex += (int)(leftAxis.y * 1);
            Debug.Log("dasdsa: " + leftAxis.y + ", level index: " + lvlIndex);

            lvlIndex = lvlIndex % (levels.Length - 1);
            lvlIndex = Mathf.Abs(lvlIndex);

            AlignToLvl();
        }
    }

    void ManualMovement(float dt)
    {
        Vector2 leftAxis = inputManager.StickAxis;
        transform.Rotate((transform.up * leftAxis.x) + (transform.right * leftAxis.y));

    }
}
