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


    [Header("Plane Visuals")]
    public DetectedPlaneGenerator _planeGen;

    [Header("Debug")]
    public Transform _debugAnchor;
   
    private Anchor _arAnchor;
    private List<GameObject> _anchorIndicators;
    private List<GameObject> _clearanceIndicators;

    private Anchor _arLeftTrack;
    private Anchor _arRightTrack;

    private bool _foundHit = false;

    private void Start()
    {
        _anchorIndicators = new List<GameObject>();
        _clearanceIndicators = new List<GameObject>();
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
                CreateTrackAnchor(ref _arLeftTrack);
            }
            else if (_arRightTrack == null)
            {
                CreateTrackAnchor(ref _arRightTrack);
            }
            else
            {
                ResetAnchorAndClearance();
            }
        }
         

        if(_arLeftTrack != null && _arRightTrack != null)
        {
            if(_clearanceIndicators.Count == 0)
            {
                ShowClearance();
            }
        }
       
    }

    private void ResetAnchorAndClearance()
    {
        foreach(var indicator in _clearanceIndicators)
        {
            Destroy(indicator);
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

        _clearanceIndicators.Add(ClearanceIndicator);

        // Add Transparency between Indicators

        for(int i = 1; i < 4; i++)
        {
            var ind1 = Instantiate(ClearanceObj, 
                ClearanceIndicator.transform.position + ClearanceIndicator.transform.right * i * 2,
                ClearanceIndicator.transform.rotation);
            
            var ind2 = Instantiate(ClearanceObj, 
                ClearanceIndicator.transform.position + ClearanceIndicator.transform.right * i * -2,
                ClearanceIndicator.transform.rotation);

            _clearanceIndicators.Add(ind1);
            _clearanceIndicators.Add(ind2);
        }

        //ClearanceRight = Instantiate(ClearanceObj, Center, ClearanceLeft.transform.rotation * Quaternion.Euler(0, 180, 0));

    }

    private void OnGUI()
    {
        if (_arLeftTrack == null)
        {
            GUI.skin.label.fontSize = 50;
            GUI.Label(new Rect(10, 30, 500, 1000), "LEFT NULL");
        }
        if (_arLeftTrack && _arRightTrack)
        {
            GUI.skin.label.fontSize = 50;
            GUI.Label(new Rect(10, 10, 500, 1000), $"Distance between Anchors = {Vector3.Distance(_arLeftTrack.transform.position, _arRightTrack.transform.position)}");
        }
    }

    public void CreateTrackAnchor(ref Anchor trackAnchor) //create Anchor on vertical plane in front of camera, from center position of smartphone screen
    {
        TrackableHit hit;


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

    public void SetTrackingStatus(bool status)
    {
        _planeGen._trackingLost = status;
    }

}
