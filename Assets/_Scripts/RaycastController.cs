using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastController : MonoBehaviour
{
    public Transform _raycastSource;
    public float _raycastDistance = 10f;

    [Header("Debug Settings")]
    public bool _showDebugRay = true; //editor only

    void FixedUpdate()
    {
        // Bit shift the index of the layer (8) to get a bit mask. Cast rays only against colliders in layer 8.
        int layerMask = 1 << 8; //Targets

        RaycastHit hit;

        Vector3 forward = _raycastSource.TransformDirection(Vector3.forward);
        if(_showDebugRay)Debug.DrawRay(_raycastSource.position, forward * _raycastDistance, Color.green);

        if (Physics.Raycast(_raycastSource.position, forward, out hit, _raycastDistance, layerMask))
        {
            Debug.Log("###DEBUG: object hit " + hit.collider.gameObject.name);
            //Handheld.Vibrate(); //no support for vibration duration or pattern, need to use asset
            //hit.collider.gameObject.SetActive(false);
            hit.collider.gameObject.GetComponent<ResizeController>().HitAnimation();
        }

    }
}
