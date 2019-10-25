using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCursor : MonoBehaviour
{
    // Start is called before the first frame update

    private Camera cam;
    [SerializeField] float zDistance = -2f;

    void Start()
    {
        cam = Camera.main;
    }

    void OnGUI()
    {
        Event currentEvent = Event.current;
        Vector2 mousePos = new Vector2(currentEvent.mousePosition.x, cam.pixelHeight - currentEvent.mousePosition.y);
        Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane - zDistance));
        transform.LookAt(point);
    }
}
