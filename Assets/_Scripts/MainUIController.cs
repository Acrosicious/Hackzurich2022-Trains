using GoogleARCore.Examples.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;


public class MainUIController : MonoBehaviour
{

    [Header("Warning Splash Screen")]
    public GameObject _warningSplashScreen;
    public float _splashScreenDuration = 2;
    public float _fadeDuration = 0.5f;

    [Header("Snackbar")]
    public GameObject _snackbarPanel;
    public TMP_Text _snackbarText;
    public string _panelAnchorText = "Move phone slowly around to scan for surfaces. Tap on the left and right train tracks for clearance indication.";
    
    [Header("Retice")]
    public GameObject _reticleObject;
    public GameObject _arrowObject;

    [Header("In Game Objects")]
    public GameObject _planeDiscovery;
    public GameObject _XTarget;

    public static MainUIController Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        StartCoroutine(FadeoutPanel());
        ResetUI();
    }


    public void ResetUI()
    {
        _arrowObject.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    IEnumerator FadeoutPanel()
    {
        float elapsedTime = 0;

        yield return new WaitForSeconds(_splashScreenDuration);

        var canvasGroup = _warningSplashScreen.GetComponent<CanvasGroup>();

        while (elapsedTime < _fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / _fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _warningSplashScreen.SetActive(false);
        _snackbarPanel.SetActive(true);
        _snackbarText.text = _panelAnchorText;
    }

}
