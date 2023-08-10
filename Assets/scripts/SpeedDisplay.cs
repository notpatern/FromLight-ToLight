using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class SpeedDisplay : MonoBehaviour
{
    public PlayerMovement pm;
    float speedValue;
    public TMP_Text speedText;
    Vector3 horizontalSpeed;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        horizontalSpeed = new Vector3(pm.rb.velocity.x, pm.rb.velocity.y, pm.rb.velocity.z);
        speedValue = horizontalSpeed.magnitude;
        speedText.text = "Speed: " + Math.Floor(speedValue);
    }
}
