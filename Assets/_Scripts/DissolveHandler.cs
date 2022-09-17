using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveHandler : MonoBehaviour
{
    private Material _mat;
    public float _dissolveDuration = 1f;

    void Start()
    {
        _mat = GetComponent<MeshRenderer>().material;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            _mat.EnableKeyword("_EMISSION");
            StartCoroutine(Dissolve());
        }

    }

    IEnumerator Dissolve()
    {
        float elapsedTime = 0;
 
        while (elapsedTime < _dissolveDuration)
        {           
            float amount = Mathf.SmoothStep(0, 1, (elapsedTime / _dissolveDuration));
            _mat.SetFloat("_Cutoff", amount);
             elapsedTime += Time.deltaTime;

            yield return null;
        }

        //ResetPiece();
    }

    public void ResetPiece()
    {
        _mat.DisableKeyword("_EMISSION");
        _mat.SetFloat("_Cutoff", 0);
    }


}
