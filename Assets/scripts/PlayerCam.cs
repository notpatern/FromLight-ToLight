using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{

    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform Camera;

    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * 0.001f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * 0.001f;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void ChangeFOV(float maxFov, float transitionTime)
    {
        GetComponent<Camera>().DOFieldOfView(maxFov, transitionTime);
    }

    public void ChangeCamTilt(float maxTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, maxTilt), 0.25f);
    }
}
