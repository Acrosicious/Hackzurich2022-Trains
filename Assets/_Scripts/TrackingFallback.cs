using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingFallback : MonoBehaviour
{

    public Transform cameraTransform;

    Quaternion? offset = null;
    Quaternion targetRotation = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Session.Status == SessionStatus.LostTracking &&
                Session.LostTrackingReason != LostTrackingReason.None)
        {
            var s = (Input.gyro.attitude);
            if (!offset.HasValue)
            {

                offset = Quaternion.Inverse(s);
                targetRotation = s * offset.Value;
                StartCoroutine(LerpRotation());

            }

            var rot = s * offset.Value;
            // Coroutine next rotation if change is large
            if (Quaternion.Angle(transform.rotation, rot) > 0.01f)
            {
                targetRotation = rot;
            }
        }
        else
        {
            offset = null;
            transform.rotation = Quaternion.identity;
        }

    }

    IEnumerator LerpRotation()
    {
        while (offset.HasValue)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
            yield return null;
        }

    }


    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
