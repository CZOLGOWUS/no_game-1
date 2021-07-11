using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainCamera : MonoBehaviour
{

    public Transform Player;
    [Range(0.0f,1f)]
    public float cameraSpeed;
    public Vector3 cameraOffset;
    private float currentSpeed;
    private Vector3 currentVelocity;


    // Update is called once per frame
    void Update()
    {

        Camera.main.transform.position = Vector3.SmoothDamp( 
            Camera.main.transform.position ,
            Player.position , 
            ref currentVelocity,
            cameraSpeed ) + cameraOffset;
    }
}
