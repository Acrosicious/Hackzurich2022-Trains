using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSelection : MonoBehaviour
{

    public ARCoreSession aRCoreSession;
    private List<CameraConfig> configs = null;

    public void Awake()
    {
        // Register a custom selection function for the callback.
        aRCoreSession.RegisterChooseCameraConfigurationCallback(
            _OnChooseCameraConfigurationDelegate);
    }

    // Define the new callback function.
    private int _OnChooseCameraConfigurationDelegate(
        List<CameraConfig> supportedConfigurations)
    {
        // Look through supported camera configurations, and return the
        // index that you want (using 0 as an example).
        configs = supportedConfigurations;
        return 0;
    }

    private void OnGUI()
    {
        if(configs != null)
        {
            GUI.skin.label.fontSize = 50;
            int y = 50;
            int i = 0;
            foreach(var c in configs)
            {
                GUI.Label(new Rect(10, y, 500, 1000), $"Kamera {i}:{c.ImageSize.x}, {c.ImageSize.y}");
                y += 40;
                i++;
            }
    
            
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
