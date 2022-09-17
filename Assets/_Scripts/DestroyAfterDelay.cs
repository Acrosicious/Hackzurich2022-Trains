using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    public float _delayTime = 5f;

    void Start()
    {
       Destroy(this.gameObject, _delayTime);
    }


}
