using System;
using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

public class MagnifyingRect : MonoBehaviour
{
    public enum MagnificationMode { NATURAL, GAZE, COMBINED }

    [SerializeField]
    private MagnificationMode _mode;

    [SerializeField]
    private bool _debugMode = true;

    [SerializeField]
    private GameObject _rectObject;

    [SerializeField]
    private float _rectHeight = 0.2f;

    [SerializeField]
    private float _offsetFromHands = 0.1f;

    [SerializeField]
    private Camera _magnifyingCamera;

    [SerializeField]
    private float _thresholdAngle = 40f;

    [SerializeField]
    private GameObject _debugCanvas;

    private IMagnifier _magnifier;

    private Transform _playerTransform;

    private Transform _leftHand;

    private Transform _rightHand;

    private Gazeable _rectGazeable;

    private Coroutine _fadeCoroutine;

    private LerpAlpha[] _lerpAlphas;

    private RaycastHit? _gazeGlassIntersection;

    private Text _debugText;

    private bool _isActive = false;

    private float _standardFov;

    private Vector3 _planeNormal = Vector3.zero;

    private void Awake()
    {
        _playerTransform = Camera.main.transform;
        _standardFov = _magnifyingCamera.fieldOfView;
        _rectGazeable = _rectObject.GetComponentInChildren<Gazeable>();
        _lerpAlphas = _rectObject.GetComponentsInChildren<LerpAlpha>();

        _debugText = _debugCanvas.GetComponentInChildren<Text>();

        _magnifier = AssignMagMode();
    }

    private void Start()
    {
        ToggleMagnification(false);
    }

    private IMagnifier AssignMagMode()
    {
        switch (_mode)
        {
            case MagnificationMode.NATURAL:
                return new NaturalMagnifier(_playerTransform, _rectObject.transform, _debugText);
            case MagnificationMode.GAZE:
                return new GazeMagnifier(_playerTransform, _rectObject.transform, _debugText);
            case MagnificationMode.COMBINED:
                return new CombinedMagnifier(_playerTransform, _rectObject.transform, _debugText);
        }
        return null;
    }

    private bool AreHandsAlive()
    {
        return _leftHand && _rightHand;
    }

    private void FindGazeGlassIntersection()
    {
        TobiiXR_GazeRay gazeRay = TobiiXR.EyeTrackingData.GazeRay;
        RaycastHit screenHit;
        if (gazeRay.IsValid && Physics.Raycast(gazeRay.Origin, gazeRay.Direction, out screenHit, 10f) && screenHit.collider.transform == _rectObject.transform)
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
        foreach (LerpAlpha la in _lerpAlphas)
        {
            la.Fade(isEnabled);
        }

        //_debugCanvas.gameObject.SetActive(isEnabled && _debugMode);
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
        // 
        if (AreHandsAlive())
        {
            UpdateRectDimensions();
            FindGazeGlassIntersection();
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
        if (_isActive)
        {
            _magnifyingCamera.fieldOfView = _standardFov / _magnifier.GetMagnification(_gazeGlassIntersection.Value, _planeNormal, _debugMode);
        }
        if (_debugMode)
        {
            UpdateDebugCanvas();
        }
    }

    private void UpdateRectDimensions()
    {
        Transform leftTrans = _leftHand.transform;
        Transform rightTrans = _rightHand.transform;
        float width = Vector3.Distance(leftTrans.position, rightTrans.position) - _offsetFromHands;

        _rectObject.transform.localScale = new Vector3(width, _rectHeight, 1f);
        _magnifyingCamera.aspect = width / _rectHeight;
        _rectObject.transform.position = (leftTrans.position + rightTrans.position) / 2f;

        Vector3 upDir = leftTrans.forward + rightTrans.forward;
        Vector3 rightDir = rightTrans.position - leftTrans.position;
        //if (Vector3.Angle(leftTrans.forward, rightDir) < _thresholdAngle && _rectGazeable.HasFocus)

        _planeNormal = Vector3.Cross(rightDir, upDir);
        _rectObject.transform.rotation = Quaternion.LookRotation(_planeNormal, upDir);
    }

    private void UpdateCameraTransform()
    {
        _magnifyingCamera.transform.position = _rectObject.transform.position;
        _magnifyingCamera.transform.rotation = Quaternion.LookRotation(_playerTransform.forward);
    }

    private void UpdateDebugCanvas()
    {
        _debugCanvas.transform.position = _rectObject.transform.position;
        _debugCanvas.transform.rotation = Quaternion.LookRotation(_playerTransform.forward, _playerTransform.up);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _playerTransform != null)
        {
            _magnifier = AssignMagMode();
        }
    }

}

