using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

public class MagnifyingRect : MonoBehaviour
{
    [SerializeField]
    private GameObject _rectObject;

    [SerializeField]
    private float _rectHeight = 0.2f;

    [SerializeField]
    private float _cameraDistance = 8f;

    [SerializeField]
    private float _zoomStep = 5f;

    [SerializeField]
    private float _fovStep = 5f;

    [SerializeField]
    private Camera _magnifyingCamera;

    [SerializeField]
    private GameObject _debugCanvas;

    private Transform _playerTransform;

    private Transform _leftHand;

    private Transform _rightHand;

    private Text _debugText;

    private bool _isActive = false;

    private float _zoomDistance = 0f;

    private void Awake()
    {
        _playerTransform = Camera.main.transform;
        _debugText = _debugCanvas.GetComponentInChildren<Text>();
        _zoomDistance = _cameraDistance;
        ToggleMagnification(false);
    }

    private bool AreHandsAlive()
    {
        return _leftHand && _rightHand;
    }

    private void ToggleMagnification(bool isEnabled)
    {
        _rectObject.SetActive(isEnabled);
        _debugCanvas.gameObject.SetActive(isEnabled);
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
            } else if (changedSource == SteamVR_Input_Sources.RightHand)
            {
                _rightHand = pose.transform;
            }
        } else
        {
            if (changedSource == SteamVR_Input_Sources.LeftHand)
            {
                _leftHand = null;
            } else if (changedSource == SteamVR_Input_Sources.RightHand)
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
        if (SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.Any].stateDown && AreHandsAlive())
        {
            ToggleMagnification(!_isActive);
        }

        if (_isActive)
        {
            UpdateRectDimensions();
            UpdateCameraTransform();
            UpdateDebugText();
        }
    }

    private void UpdateRectDimensions()
    {
        if (SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.Any].stateDown)
        {
            _rectObject.transform.rotation = Quaternion.LookRotation(_playerTransform.forward, _playerTransform.up);
        }

        Transform leftTrans = _leftHand.transform;
        Transform rightTrans = _rightHand.transform;
        float width = Vector3.Distance(leftTrans.position, rightTrans.position);

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

        Vector3 normalDir = Vector3.Cross(rightDir, upDir);

        _rectObject.transform.rotation = Quaternion.LookRotation(normalDir, upDir);

    }

    private void UpdateCameraTransform()
    {
        ISteamVR_Action_Vector2 rightHandTouch = SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.RightHand];
        if (rightHandTouch.axis.y != 0f && rightHandTouch.lastAxis.y != 0f)
        {
            _zoomDistance += rightHandTouch.delta.y * _zoomStep;

        }
        ISteamVR_Action_Vector2 leftHandTouch = SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.LeftHand];
        if (leftHandTouch.axis.y != 0f && leftHandTouch.lastAxis.y != 0f)
        {
            _magnifyingCamera.fieldOfView -= leftHandTouch.delta.y * _fovStep;
        }

        // Base zoom direction on head movement? Below is a bit nauseating
        // _magnifyingCamera.transform.position = _rectObject.transform.position + _playerTransform.forward * (_cameraDistance + _zoomAmount);

        _magnifyingCamera.transform.position = _rectObject.transform.position + _rectObject.transform.forward * _zoomDistance;
        _magnifyingCamera.transform.rotation = Quaternion.LookRotation(_playerTransform.forward);
    }

    private void UpdateDebugText()
    {
        _debugCanvas.transform.position = _rectObject.transform.position;
        _debugCanvas.transform.rotation = Quaternion.LookRotation(_playerTransform.forward, _playerTransform.up);
        _debugText.text = "Zoom distance: " + _zoomDistance + "\nFOV: " + _magnifyingCamera.fieldOfView;
    }

}

