using System;
using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

public class MagnificationManager : MonoBehaviour
{
    public enum MagnificationMode { NATURAL, GAZE, COMBINED }

    [SerializeField]
    private MagnificationMode _mode;


    [SerializeField]
    private float _rectHeight = 0.2f;

    [SerializeField]
    private float _offsetFromHands = 0.1f;

    private Transform _magRect;

    private Camera _magCamera;

    private IMagnifier _magnifier;

    private Transform _player;

    private GazeTeleport _gazeTeleport;

    private Transform _leftHand;

    private Transform _rightHand;

    private LerpAlpha[] _rectFadeEffects;

    private RaycastHit? _gazeGlassIntersection;

    private bool _isActive = false;

    private float _standardFov;

    private Vector3 _planeNormal = Vector3.zero;

    private void Awake()
    {
        _magRect = GameObject.FindGameObjectWithTag("MagRect").transform;
        _player = Camera.main.transform;
        _magCamera = GetComponentInChildren<Camera>();
        _standardFov = _magCamera.fieldOfView;
        _rectFadeEffects = _magRect.GetComponentsInChildren<LerpAlpha>();
        _gazeTeleport = GetComponent<GazeTeleport>();

        AssignMagMode();
    }

    private void Start()
    {
        ToggleMagnification(false);
    }

    private void AssignMagMode()
    {
        switch (_mode)
        {
            case MagnificationMode.NATURAL:
                _magnifier = GetComponent<NaturalMagnifier>();
                break;
            case MagnificationMode.GAZE:
                _magnifier = GetComponent<GazeMagnifier>();
                break;
            case MagnificationMode.COMBINED:
                _magnifier = GetComponent<CombinedMagnifier>();
                break;
        }
    }

    private bool AreHandsAlive()
    {
        return _leftHand && _rightHand;
    }

    private void FindGazeRectIntersection()
    {
        TobiiXR_GazeRay gazeRay = TobiiXR.EyeTrackingData.GazeRay;
        RaycastHit screenHit;
        if (gazeRay.IsValid && Physics.Raycast(gazeRay.Origin, gazeRay.Direction, out screenHit, 10f) && screenHit.collider.transform == _magRect)
        {
            _gazeGlassIntersection = screenHit;
        }
        else
        {
            _gazeGlassIntersection = null;
        }
    }

    private void ToggleMagnification(bool isEnabled)
    {
        foreach (LerpAlpha la in _rectFadeEffects)
        {
            la.Fade(isEnabled);
        }
        _isActive = isEnabled;
    }

    public void OnHandConnectionChange(SteamVR_Behaviour_Pose pose, SteamVR_Input_Sources changedSource, bool isConnected)
    {
        if (isConnected)
        {
            pose.GetComponentInChildren<Renderer>().enabled = true;
            if (changedSource == SteamVR_Input_Sources.LeftHand)
            {
                _leftHand = pose.transform;
            }
            else if (changedSource == SteamVR_Input_Sources.RightHand)
            {
                _rightHand = pose.transform;
            }
        }
        else
        {
            if (changedSource == SteamVR_Input_Sources.LeftHand)
            {
                _leftHand = null;
            }
            else if (changedSource == SteamVR_Input_Sources.RightHand)
            {
                _rightHand = null;
            }
            pose.GetComponentInChildren<Renderer>().enabled = false;
            ToggleMagnification(false);
        }
    } 

    private void Update()
    {
        if (AreHandsAlive())
        {
            UpdateRectDimensions();
            FindGazeRectIntersection();
            if (_gazeGlassIntersection.HasValue && !_isActive)
            {
                ToggleMagnification(true);
            }
            else if (!_gazeGlassIntersection.HasValue && _isActive)
            {
                ToggleMagnification(false);
            }
        }

        UpdateCameraTransform();
        if (_isActive && !_gazeTeleport.IsTeleportPending)
        {
            _magCamera.fieldOfView = _standardFov / _magnifier.GetMagnification(_gazeGlassIntersection.Value, _planeNormal);
        }
    }

    private void UpdateRectDimensions()
    {
        Transform leftTrans = _leftHand.transform;
        Transform rightTrans = _rightHand.transform;
        float width = Vector3.Distance(leftTrans.position, rightTrans.position) - _offsetFromHands;

        _magRect.localScale = new Vector3(width, _rectHeight, 1f);
        _magCamera.aspect = width / _rectHeight;
        _magRect.position = (leftTrans.position + rightTrans.position) / 2f;

        Vector3 upDir = leftTrans.forward + rightTrans.forward;
        Vector3 rightDir = rightTrans.position - leftTrans.position;

        _planeNormal = Vector3.Cross(rightDir, upDir);
        _magRect.rotation = Quaternion.LookRotation(_planeNormal, upDir);
    }

    private void UpdateCameraTransform()
    {
        _magCamera.transform.position = _magRect.position;
        _magCamera.transform.rotation = Quaternion.LookRotation(_player.forward);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _player != null)
        {
            AssignMagMode();
        }
    }

}

