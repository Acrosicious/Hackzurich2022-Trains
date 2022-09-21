using GoogleARCore;
using GoogleARCore.Examples.Common;
using RDG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class TargetSpawner : MonoBehaviour
{
    public static TargetSpawner Instance { get; private set; }


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

    public Camera _firstPersonCam;

    public GameObject ClearanceObj;
    public GameObject Radius1mIndicator;

    public CameraImageManager cameraImageManager;


    [Header("Plane Visuals")]
    public DetectedPlaneGenerator _planeGen;

    [Header("Debug")]
    public Transform _debugAnchor;

    private bool _manualMode = false;

    private Anchor _arAnchor;
    private List<GameObject> _anchorIndicators;
    private List<ClearanceSettings> _clearanceIndicators;

    private Anchor _arLeftTrack;
    private Anchor _arRightTrack;

    private List<Anchor> _additionalAnchors;

    private bool _foundHit = false;

    private void Start()
    {
        _anchorIndicators = new List<GameObject>();
        _clearanceIndicators = new List<ClearanceSettings>();
        Radius1mIndicator.SetActive(false);
        _additionalAnchors = new List<Anchor>();
    }

    public void Update()
    {
        
        // If in manual mode, check if user touched the screen in this frame to place Anchors manually
        if (_manualMode && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // First touch in manual mode places the left track anchor
            if (_arLeftTrack == null)
            {
                Quaternion planeRotation = Quaternion.identity;
                CreateTrackAnchor(ref _arLeftTrack, out planeRotation);
                if(_arLeftTrack != null)
                {
                    Radius1mIndicator.transform.position = _arLeftTrack.transform.position;
                    Radius1mIndicator.transform.rotation = planeRotation;
                    Radius1mIndicator.SetActive(true);
                }
            }
            else if (_arRightTrack == null)
            {
                CreateTrackAnchor(ref _arRightTrack, out _);
                if(_arRightTrack != null)
                {
                    Radius1mIndicator.SetActive(false);
                    _manualMode = false;
                }
            }
        }

        // If both anchors are placed, shot the clearance indication
        if(_arLeftTrack != null && _arRightTrack != null)
        {
            if(_clearanceIndicators.Count == 0)
            {
                ShowClearance(_arLeftTrack.transform.position, _arRightTrack.transform.position);
            }
        }
    }

    // Start listening for touches to place the anchors
    public void EnableManualMode()
    {
        _manualMode = true;
        MainUIController.Instance._snackbarPanel.SetActive(true);
    }

    /// <summary>
    /// Change the UI back to the selection of Auto + Manual mode.
    /// Destroy all anchors and indicators.
    /// </summary>
    public void ResetAnchorAndClearance()
    {
        MainUIController.Instance._resetButton.SetActive(false);
        MainUIController.Instance.ActivateUserButtons(false);

        foreach(var a in _additionalAnchors)
        {
            Destroy(a.gameObject);
        }
        _additionalAnchors.Clear();

        foreach (var indicator in _clearanceIndicators)
        {
            Destroy(indicator.gameObject);
        }

        _clearanceIndicators.Clear();

        _arLeftTrack = null;
        _arRightTrack = null;

        foreach(var go in _anchorIndicators)
        {
            Destroy(go);
        }

        _anchorIndicators.Clear();
    }

    /// <summary>
    /// Places the clearance object in between the two given vectors.
    /// They should represent an orthogonal line to the tracks.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public void ShowClearance(Vector3 left, Vector3 right)
    {
        var Center = (left + right) / 2f;
        var RightVector = (right - left).normalized;

        var rot = Quaternion.FromToRotation(Vector3.right, RightVector);

        var ClearanceIndicator = Instantiate(ClearanceObj, Center, Quaternion.identity);
        ClearanceIndicator.transform.LookAt(left);

        _clearanceIndicators.Add(ClearanceIndicator.GetComponent<ClearanceSettings>());

        // If you want to draw multiple clearances along a straight track.
        // If more input is available, this could be used to place clearence along a curve.

        //for(int i = 1; i < 4; i++)
        //{
        //    var ind1 = Instantiate(ClearanceObj, 
        //        ClearanceIndicator.transform.position + ClearanceIndicator.transform.right * i * 2,
        //        ClearanceIndicator.transform.rotation);
            
        //    var ind2 = Instantiate(ClearanceObj, 
        //        ClearanceIndicator.transform.position + ClearanceIndicator.transform.right * i * -2,
        //        ClearanceIndicator.transform.rotation);

        //    _clearanceIndicators.Add(ind1.GetComponent<ClearanceSettings>());
        //    _clearanceIndicators.Add(ind2.GetComponent<ClearanceSettings>());
        //}


        MainUIController.Instance._resetButton.SetActive(true);
        MainUIController.Instance.ActivateUserButtons(true);

        // Hide hint where user is asked to touch the screen.
        MainUIController.Instance._snackbarPanel.SetActive(false);

    }


    // Called via UI Input Action (Auto Button)
    private void SendImageToServer()
    {
        cameraImageManager.createScreenshot();
    }

    // Called via UI Input Action (Transparency Button)
    public void ToggleMaterial()
    {
        foreach(var c in _clearanceIndicators)
        {
            c.ToggleMaterial();
        }
    }

    // Called via UI Input Action (Tunnel/Area Button)
    public void ToggleTunnel()
    {
        foreach (var c in _clearanceIndicators)
        {
            c.ToggleTunnel();
        }
    }

    // Called via UI Input Action (Unused, previously used to display 3D Track object)
    public void ToggleModel()
    {
        foreach (var c in _clearanceIndicators)
        {
            c.ToggleRailroad();
        }
    }

    private void OnGUI()
    {
        // For degub to show the ARCore calculated distance between the anchors.
        if (_arLeftTrack && _arRightTrack)
        {
            GUI.skin.label.fontSize = 50;
            GUI.Label(new Rect(10, 10, 500, 1000), $"Distance between Anchors = {Vector3.Distance(_arLeftTrack.transform.position, _arRightTrack.transform.position)}");
        }
    }

    /// <summary>
    /// Create Anchor on vertical ARCore plane where the user touched the screen.
    /// Uses Raycast from Screenspace touch point.
    /// </summary>
    /// <param name="trackAnchor">Referenca to the anchor object to be used for the new anchor.</param>
    /// <param name="planeRotation">Rotation of the plane for the circular distance indicator.</param>
    public void CreateTrackAnchor(ref Anchor trackAnchor, out Quaternion planeRotation)
    {
        TrackableHit hit;

        planeRotation = Quaternion.identity;

        // If the player has not touched the screen, we are done with this update.
        // Probably redundant now.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
        TrackableHitFlags.FeaturePointWithSurfaceNormal;

        _foundHit = Frame.Raycast(
             touch.position.x, touch.position.y, raycastFilter, out hit);

        if (_foundHit && hit.Trackable is DetectedPlane)
        {
            // Use hit pose and camera pose to check if hit-test is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(_firstPersonCam.transform.position - hit.Pose.position,
                    hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                if (detectedPlane.PlaneType != DetectedPlaneType.Vertical)  //only horizontal surfaces
                {
                    trackAnchor = hit.Trackable.CreateAnchor(hit.Pose);
                    planeRotation = detectedPlane.CenterPose.rotation;
                }

            }
        }

        if (trackAnchor == null)
            return;

        var indicator = Instantiate(_debugAnchor, hit.Pose.position, hit.Pose.rotation);
        indicator.transform.parent = trackAnchor.transform;
        _anchorIndicators.Add(indicator.gameObject);
        Vibration.Vibrate(300, 80, false);
        _planeGen._anchorSet = true;

    }

    /// <summary>
    /// Like CreateTrackAnchor but uses the passed ray instead of touch input.
    /// </summary>
    /// <param name="ray">Ray which points towards the plane with the tracks.</param>
    /// <returns></returns>
    public Transform CreateTrackAnchorRaycast(Ray ray)
    {
        TrackableHit hit;
        Anchor newAnchor = null;

        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
        TrackableHitFlags.FeaturePointWithSurfaceNormal;

        _foundHit = Frame.Raycast(ray.origin, ray.direction, out hit, float.MaxValue, raycastFilter);

        if (_foundHit && hit.Trackable is DetectedPlane)
        {
            // Use hit pose and camera pose to check if hit-test is from the
            // back of the plane, if it is, no need to create the anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(_firstPersonCam.transform.position - hit.Pose.position,
                    hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                if (detectedPlane.PlaneType != DetectedPlaneType.Vertical)  //only horizontal surfaces
                {
                    newAnchor = hit.Trackable.CreateAnchor(hit.Pose);
                }

            }
        }

        if (newAnchor == null)
            return null;

        var indicator = Instantiate(_debugAnchor, hit.Pose.position, hit.Pose.rotation);
        indicator.transform.parent = newAnchor.transform;
        _anchorIndicators.Add(indicator.gameObject);
        _planeGen._anchorSet = true;
        return indicator;
    }

    /// <summary>
    /// Can be used to hide the UI for the screenshot.
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisibleMeshes(bool visible)
    {
        foreach(var a in _anchorIndicators)
        {
            a.SetActive(visible);
        }

        foreach(var m in _clearanceIndicators)
        {
            m.gameObject.SetActive(visible);
        }
    }

    public void SetTrackingStatus(bool status)
    {
        _planeGen._trackingLost = status;
    }

}
