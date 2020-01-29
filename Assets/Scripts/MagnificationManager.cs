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
    public bool IsMagnifying { get { return _isActive; } }

    public Vector3 LastWorldGazePos { get; private set; }

    public enum MagnificationMode { NATURAL, GAZE, COMBINED }

    [SerializeField]
    private MagnificationMode _mode;

    [SerializeField]
    private float _rectHeight = 0.2f;

    [SerializeField]
    private float _offsetFromHands = 0.1f;

    [SerializeField]
    private int _numFramesToBuffer = 30;

    private Transform _magRect;

    private Camera _magCamera;

    private IMagnifier _magnifier;

    private Transform _player;

    private HandTeleporter _handTeleporter;

    private GazeTeleport _gazeTeleport;

    private Checklist _checklist;

    private Transform _leftHand;

    private Transform _rightHand;

    private LerpAlpha[] _rectFadeEffects;

    private RaycastHit? _gazeRectIntersection;

    private bool _isActive = false;

    private float _standardFov;

    private float _handDistance;

    private Vector3 _planeNormal = Vector3.zero;

    private int _gazeBufferIndex = 0;

    private Vector3[] _bufferedGazePositions;

    private void Awake()
    {
        _magRect = GameObject.FindGameObjectWithTag("MagRect").transform;
        _player = Camera.main.transform;
        _magCamera = GetComponentInChildren<Camera>();
        _standardFov = _magCamera.fieldOfView;
        _rectFadeEffects = _magRect.GetComponentsInChildren<LerpAlpha>();
        _gazeTeleport = FindObjectOfType<GazeTeleport>();
        _handTeleporter = FindObjectOfType<HandTeleporter>();
        _checklist = FindObjectOfType<Checklist>();

        _bufferedGazePositions = new Vector3[_numFramesToBuffer];
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

    private void FindGazeIntersections()
    {
        TobiiXR_GazeRay gazeRay = TobiiXR.EyeTrackingData.GazeRay;
        if (gazeRay.IsValid && Physics.Raycast(gazeRay.Origin, gazeRay.Direction, out RaycastHit hit, 10f))
        {
            if (hit.collider.transform == _magRect)
            {
                _gazeRectIntersection = hit;
            }
            else
            {
                _bufferedGazePositions[_gazeBufferIndex] = hit.point;
                int i = _gazeBufferIndex;
                Vector3 averageGazePos = Vector3.zero;
                for (int s = 0; s < _numFramesToBuffer; s++)
                {
                    averageGazePos += _bufferedGazePositions[i];
                    i = (i + 1) % _numFramesToBuffer;
                }
                averageGazePos /= _numFramesToBuffer;

                _gazeBufferIndex = (_gazeBufferIndex + 1) % _numFramesToBuffer;
                LastWorldGazePos = averageGazePos;
            }
        }
        else
        {
            _gazeRectIntersection = null;
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
            FindGazeIntersections();
            if (_gazeRectIntersection.HasValue && !_isActive && !_handTeleporter.IsArcActive && !_checklist.IsVisible && _handDistance <= 1f)
            {
                ToggleMagnification(true);
            }
            else if (_isActive && (!_gazeRectIntersection.HasValue || _handTeleporter.IsArcActive || _checklist.IsVisible || _handDistance > 1f))
            {
                ToggleMagnification(false);
            }
        }

        UpdateCameraTransform();
        if (_isActive && !_gazeTeleport.IsTeleportPending)
        {
            _magCamera.fieldOfView = _standardFov / _magnifier.GetMagnification(_gazeRectIntersection.Value, _planeNormal);
        }
    }

    private void UpdateRectDimensions()
    {
        _handDistance = Vector3.Distance(_leftHand.position, _rightHand.position);
        float width = _handDistance - _offsetFromHands;

        _magRect.localScale = new Vector3(width, _rectHeight, 1f);
        _magCamera.aspect = width / _rectHeight;
        _magRect.position = (_leftHand.position + _rightHand.position) / 2f;

        Vector3 upDir = _leftHand.forward + _rightHand.forward;
        Vector3 rightDir = _rightHand.position - _leftHand.position;

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

