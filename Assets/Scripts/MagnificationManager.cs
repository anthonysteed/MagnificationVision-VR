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

    public enum MagnificationMode { NATURAL, GAZE, COMBINED, NONE }

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

    private WorldGazeTracker _gazeTracker;

    private HandTeleporter _handTeleporter;

    private GazeTeleport _gazeTeleport;

    private Checklist _checklist;

    private Transform _leftHand;

    private Transform _rightHand;

    private LerpAlpha[] _rectFadeEffects;

    private bool _isActive = false;

    private float _standardFov;

    private float _handDistance;

    private Vector3 _planeNormal = Vector3.zero;

    private RaycastHit? _gazeRectIntersection;

    private float _totalTimeActive = 0f;

    private float _curTimeActive = 0f;

    private BufferedLogger _log = new BufferedLogger("MagRect");

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
        _gazeTracker = FindObjectOfType<WorldGazeTracker>();

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
        if (Physics.Raycast(_gazeTracker.LastGazeRay, out RaycastHit hit, 10f))
        {
            if (hit.collider.transform == _magRect)
            {
                _gazeRectIntersection = hit;
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
        if (isEnabled)
        {
            _curTimeActive += Time.deltaTime;
            _totalTimeActive += Time.deltaTime;
        }
        else
        {
            _curTimeActive = 0f;
        }
    }

    public void OnHandConnectionChange(SteamVR_Behaviour_Pose pose, SteamVR_Input_Sources changedSource, bool isConnected)
    {
        if (changedSource == SteamVR_Input_Sources.LeftHand)
        {
            _leftHand = isConnected ? pose.transform : null;
        }
        else if (changedSource == SteamVR_Input_Sources.RightHand)
        {
            _rightHand = isConnected ? pose.transform : null;
        }

        pose.GetComponentInChildren<Renderer>().enabled = isConnected;
        if (!isConnected)
        {
            ToggleMagnification(false);
        }
    } 

    private void Update()
    {
        if (AreHandsAlive())
        {
            UpdateRectDimensions();
            FindGazeRectIntersection();
            if (_gazeRectIntersection.HasValue && !_isActive && !_handTeleporter.IsArcActive && !_checklist.IsVisible && _handDistance <= 0.5f)
            {
                ToggleMagnification(true);
            }
            else if (_isActive && (!_gazeRectIntersection.HasValue || _handTeleporter.IsArcActive || _checklist.IsVisible || _handDistance > 0.5f))
            {
                ToggleMagnification(false);
            }
        }

        UpdateCameraTransform();
        if (_isActive && !_gazeTeleport.IsTeleportPending)
        {
            float magnification;
            if (_mode == MagnificationMode.NONE)
            {
                magnification = 1f;
            }
            else
            {
                magnification = _magnifier.GetMagnification(_gazeRectIntersection.Value, _planeNormal);
            }

            _log.Append("magFactor", magnification);
            _magCamera.fieldOfView = _standardFov / magnification;
        }

        if (_isActive)
        {
            _log.Append("curActiveTime", _curTimeActive);
            _log.Append("activeTimeTotal", _totalTimeActive);
        }

        _log.Append("active", _isActive);
        _log.CommitLine();
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

        _log.Append("width", width);
        _log.Append("pos", _magRect.position);
        _log.Append("rot", _magRect.rotation.eulerAngles);
        _log.Append("headDist", Vector3.Distance(_player.position, _magRect.position));
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

