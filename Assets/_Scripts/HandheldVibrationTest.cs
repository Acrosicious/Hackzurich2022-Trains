using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using RDG;

public class HandheldVibrationTest : MonoBehaviour
{

    public enum VibrationType {VibrateClick, VibrateDoubleClick, VibrateHeavyClick, VibrateTick, VibrateOverTime }
    public VibrationType _vibrationType;
    public int _vibrationTime = 200;
    private float _countDownTime = 5f;
    private long[] _timings = new long[] { 1000, 1000, 1000, 1000, 1000 };
    private int[] _amplitudes = new int[] { 20, 40, 70, 100, 150 };

    public void Vibrate()
    {
        switch (_vibrationType)
        {
            case VibrationType.VibrateClick:
                //VibrationHelper.VibrateClick(_vibrationTime); 
                Vibration.Vibrate(500, 150, false);
                break;
            case VibrationType.VibrateDoubleClick:
                //VibrationHelper.VibrateDoubleClick(_vibrationTime);
                Vibration.Vibrate(50, 5, false);
                break;
            case VibrationType.VibrateHeavyClick:
                //VibrationHelper.VibrateHeavyClick(_vibrationTime);
                Vibration.Vibrate(200, 150, false);
                Vibration.VibratePredefined(5, false);
                break;
            case VibrationType.VibrateTick:
                //VibrationHelper.VibrateWithAmplitude(200, 80);
                //Vibration.Vibrate(200, 5, false);
                Vibration.VibratePredefined(2, false);
                break;
            case VibrationType.VibrateOverTime:
                //AndroidVibration.CreateWaveform(_timings, _amplitudes, 3);
                Vibration.Vibrate(_timings, _amplitudes, -1, false);
                break;

        }

    }



    public void UnityVibrate()
    {
        Handheld.Vibrate();
    }

}
