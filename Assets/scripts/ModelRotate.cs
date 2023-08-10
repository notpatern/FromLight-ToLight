using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelRotate : MonoBehaviour
{
    public Transform orientation;
    float yRotation;
    public float sensX;

    // Start is called before the first frame update
    void Start()
    {
        
    } 

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * 0.001f;
        yRotation += mouseX;
        orientation.rotation = Quaternion.Euler(0, yRotation, 0); ;
    }
}
