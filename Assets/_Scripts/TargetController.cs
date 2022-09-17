using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    public float _movementSpeed = 0.5f;


    private void OnEnable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            child.gameObject.SetActive(true);
            
        }
    }

    void Update()
    {
        transform.Translate(Vector3.back * Time.deltaTime * _movementSpeed);

        if (transform.position.z < -3) gameObject.SetActive(false);//quick solution
    }







}
