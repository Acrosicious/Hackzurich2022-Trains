using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundSelf : MonoBehaviour
{
    public float _rotationSpeed = 50f;


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * _rotationSpeed * Time.deltaTime, Space.World);
    }
}
