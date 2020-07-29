using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCamera : MonoBehaviour
{
    private float mouseX;
    private float mouseY;
    public float MouseSensitivity = 180f;

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;
        transform.Rotate(-mouseY, mouseX, 0);
    }
}
