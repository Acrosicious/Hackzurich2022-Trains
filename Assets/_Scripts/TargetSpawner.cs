using GoogleARCore;
using GoogleARCore.Examples.Common;
using RDG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void OnEnable()
    {

    }

    public void OnDisable()
    {
        Debug.Log("###DEBUG: Spawn Disabled!");
    }

    public void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
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
                }
            }
            //else
            //{
            //    ResetAnchorAndClearance();
            //}
        }

        if(_arLeftTrack != null && _arRightTrack != null)
        {
            if(_clearanceIndicators.Count == 0)
            {
                ShowClearance();
            }
        }
    }

    public void ResetAnchorAndClearance()
    {
        MainUIController.Instance._resetButton.SetActive(false);
        MainUIController.Instance.ActivateUserButtons(false);
        MainUIController.Instance._snackbarPanel.SetActive(true);

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

    private void ShowClearance()
    {
        var Center = (_arLeftTrack.transform.position + _arRightTrack.transform.position) / 2f;
        var RightVector = (_arRightTrack.transform.position - _arLeftTrack.transform.position).normalized;

        var rot = Quaternion.FromToRotation(Vector3.right, RightVector);

        var ClearanceIndicator = Instantiate(ClearanceObj, Center, Quaternion.identity);
        ClearanceIndicator.transform.LookAt(_arLeftTrack.transform.position);

        _clearanceIndicators.Add(ClearanceIndicator.GetComponent<ClearanceSettings>());

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
        MainUIController.Instance._snackbarPanel.SetActive(false);

        SendImageToServer();

        //ClearanceRight = Instantiate(ClearanceObj, Center, ClearanceLeft.transform.rotation * Quaternion.Euler(0, 180, 0));

    }

    private void SendImageToServer()
    {
        cameraImageManager.createScreenshot();
    }

    private void test()
    {
        
    }

    public void UserInputConfirm()
    {

    }

    public void ToggleMaterial()
    {
        foreach(var c in _clearanceIndicators)
        {
            c.ToggleMaterial();
        }
    }

    public void ToggleTunnel()
    {
        foreach (var c in _clearanceIndicators)
        {
            c.ToggleTunnel();
        }
    }

    public void ToggleModel()
    {
        foreach (var c in _clearanceIndicators)
        {
            c.ToggleRailroad();
        }
    }

    private void OnGUI()
    {
        //if (_arLeftTrack == null)
        //{
        //    GUI.skin.label.fontSize = 50;
        //    GUI.Label(new Rect(10, 30, 500, 1000), "LEFT NULL");
        //}
        if (_arLeftTrack && _arRightTrack)
        {
            GUI.skin.label.fontSize = 50;
            GUI.Label(new Rect(10, 10, 500, 1000), $"Distance between Anchors = {Vector3.Distance(_arLeftTrack.transform.position, _arRightTrack.transform.position)}");
        }
    }

    public void CreateTrackAnchor(ref Anchor trackAnchor, out Quaternion planeRotation) //create Anchor on vertical plane in front of camera, from center position of smartphone screen
    {
        TrackableHit hit;

        planeRotation = Quaternion.identity;

        // If the player has not touched the screen, we are done with this update.
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
                    //_anchorCanvas.SetActive(false);
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
