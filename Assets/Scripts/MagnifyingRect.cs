using System;
using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;
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
    private GameObject _debugCanvas;

    private IMagnifier _magnifier;

    private Transform _playerTransform;

    private Transform _leftHand;

    private Transform _rightHand;

    private Text _debugText;

    private bool _isActive = false;

    private float _standardFov;

    private Vector3 _planeNormal = Vector3.zero;

    private void Awake()
    {
        _playerTransform = Camera.main.transform;
        _standardFov = _magnifyingCamera.fieldOfView;

        _debugText = _debugCanvas.GetComponentInChildren<Text>();
        ToggleMagnification(false);

        _magnifier = AssignMagMode();
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

    private void ToggleMagnification(bool isEnabled)
    {
        _rectObject.SetActive(isEnabled);
        _debugCanvas.gameObject.SetActive(isEnabled && _debugMode);
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
        // If button pressed, enable the rect and move the camera
        if (SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.RightHand].stateDown && AreHandsAlive())
        {
            ToggleMagnification(!_isActive);
        }

        if (_isActive)
        {
            UpdateRectDimensions();
            UpdateCameraTransform();
            _magnifyingCamera.fieldOfView = _standardFov / _magnifier.GetMagnification(_planeNormal, _debugMode);
            if (_debugMode)
            {
                UpdateDebugCanvas();
            }
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
        if (Vector3.Angle(upDir, rightDir) < 1f)
        {
            ToggleMagnification(false);
            return;
        }

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
        if (Application.isPlaying)
        {
            _magnifier = AssignMagMode();
        }
    }

}

