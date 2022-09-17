using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using RDG;

public class VibrationText : MonoBehaviour
{
    public TMP_Text _canVibrate;
    public TMP_Text _hasAmplitudeControl;


    public void Start()
    {
        _canVibrate.text = Vibration.GetApiLevel().ToString();
         _hasAmplitudeControl.text = Vibration.HasAmplitudeControl().ToString();

    }
}
