using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearanceSettings : MonoBehaviour
{

    [Header("3D Indicators")]
    public GameObject Tunnel;
    public GameObject Area;
    public GameObject Border;
    public GameObject Railroad;

    [Header("Materials")]
    public Material TransparentLight;
    public Material TransparentDark;
    public Material OpaqueMat;

    enum AreaType
    {
        BorderArea0,
        BorderArea25,
        BorderArea75,
        BorderArea100,
    }

    private int currentAreaType = 0;

    // Tunnel deactivates Area
    private bool TunnelActive = false;
    private bool RailroadActive = false;


    public void ToggleTunnel()
    {
        TunnelActive = !TunnelActive;
        Tunnel.SetActive(TunnelActive);
        Area.SetActive(!TunnelActive);
    }

    public void ToggleRailroad()
    {
        RailroadActive = !RailroadActive;
        Railroad.SetActive(RailroadActive);
    }

    public void ToggleMaterial()
    {
        currentAreaType = (currentAreaType + 1) % 4;

        Tunnel.SetActive(TunnelActive);
        Area.SetActive(!TunnelActive);

        switch (currentAreaType)
        {
            case 0:
                Area.SetActive(false);
                Tunnel.SetActive(false);
                break;
            case 1:
                SetMaterial(TransparentLight);
                break;
            case 2:
                SetMaterial(TransparentDark);
                break;
            case 3:
                SetMaterial(OpaqueMat);
                break;
        }
    }

    private void SetMaterial(Material mat)
    {
        var mrs = Area.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in mrs)
        {
            mr.material = mat;
        }
        mrs = Tunnel.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in mrs)
        {
            mr.material = mat;
        }
    }
}
