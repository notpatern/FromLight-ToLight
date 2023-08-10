using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parjourswitch1 : MonoBehaviour
{
    public Transform Parkour;
    public AudioSource playSound;

    void OnTriggerEnter(Collider other)
    {
        if (Parkour.transform.position == new Vector3(-1125.9f, -160.5312f, 2165.02f))
        {
            playSound.Play();
            Parkour.transform.position = new Vector3(-1125.9f, 57777.5312f, 2165.02f);
        }
        
    }
}
