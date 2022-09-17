using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeController : MonoBehaviour
{
    public float _expandTime = 0.15f;
    public float _shrinkTime = 0.25f;
    public float _defaultScale = 1.5f;
    public float _minScale = 0;
    public float _maxScale = 1.7f;

    public void HitAnimation()
    {
        StartCoroutine(ChangeSize());       
    }

    public void ResetScale()
    {
        //transform.localScale = Vector3.one;
        transform.localScale = new Vector3(_defaultScale, _defaultScale, _defaultScale);
    }

    IEnumerator ChangeSize()
    {
   
        float elapsedTime = 0;

        while (elapsedTime < _expandTime)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(_maxScale, _maxScale, _maxScale), (elapsedTime / _expandTime));
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0;

        while (elapsedTime < _shrinkTime)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(_minScale, _minScale, _minScale), (elapsedTime / _shrinkTime));
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        gameObject.SetActive(false);
    }

}
