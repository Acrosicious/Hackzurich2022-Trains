using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePlane : MonoBehaviour
{

    public Material red, green;
    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void activateObstaclePlane(float height, Transform target)
    {
        this.target = target;
        transform.position = Vector3.zero;
        transform.Translate(Vector3.up * height);
        GetComponent<Renderer>().material = red;
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null)
        {

            if (TargetIsInZone())
            {
                var cs = GetComponentsInChildren<Renderer>();
                foreach(var c in cs)
                {
                    c.material = green;
                }
            }
            else
            {
                var cs = GetComponentsInChildren<Renderer>();
                foreach (var c in cs)
                {
                    c.material = red;
                }
            }
        }
    }

    public bool TargetIsInZone()
    {
        return target.position.y < transform.position.y;
    }
}
