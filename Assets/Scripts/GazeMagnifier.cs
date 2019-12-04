using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class GazeMagnifier : IMagnifier
{
    private Transform _player;

    private Transform _magGlass;

    private Text _debugText;

    private float _gazeRange = 20f;

    private float _sensitivity = 0.027f;

    private float _convergenceSpeed = 0.12f;

    private bool _isResetting = false;

    private float _averageGazeDistance;

    private float _lastStableDistance;

    private float _resetTime = 1f;

    private float _timePassed = 0f;

    private Vector3 _lastGazeDir;

    private float _thresholdAngle = 1f;


    public GazeMagnifier(Transform player, Transform magGlass, Text debugText)
    {
        _player = player;
        _magGlass = magGlass;
        _debugText = debugText;
    }

    public float GetMagnification(bool debugMode)
    {
        if (debugMode)
        {
            // Adjust parameters with touchpad
            ISteamVR_Action_Vector2 rightHandTouch = SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.RightHand];
            if (rightHandTouch.axis.y != 0f && rightHandTouch.lastAxis.y != 0f)
            {
               _convergenceSpeed += rightHandTouch.delta.y * 0.1f;
            }
            ISteamVR_Action_Vector2 leftHandTouch = SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.LeftHand];
            if (leftHandTouch.axis.y != 0f && leftHandTouch.lastAxis.y != 0f)
            {
                _sensitivity += leftHandTouch.delta.y * 0.1f;
            }
        }

        // Reset zoom on left trigger click
        if (SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.LeftHand].stateDown)
        {
            _isResetting = true;
            _timePassed = 0f;
            _lastStableDistance = _averageGazeDistance;
        }

        if (_isResetting)
        {
            _timePassed += Time.deltaTime;
            _averageGazeDistance = Mathf.Lerp(_lastStableDistance, 0f, _timePassed / _resetTime);
            if (_timePassed > _resetTime)
            {
                _isResetting = false;
            }
        }
        else
        {
            TobiiXR_GazeRay gazeRay = TobiiXR.EyeTrackingData.GazeRay;
            if (gazeRay.IsValid)
            {
                //if (Vector3.Angle(_lastGazeDir, gazeRay.Direction) > _thresholdAngle)
                //{
                    RaycastHit hit;
                    float newDistance;
                    if (Physics.Raycast(gazeRay.Origin, gazeRay.Direction, out hit, _gazeRange))
                    {
                        newDistance = Vector3.Distance(gazeRay.Origin, hit.point);
                    }
                    else
                    {
                        newDistance = _gazeRange;
                    }

                    if (newDistance > _averageGazeDistance)
                    {
                        _averageGazeDistance += _sensitivity * newDistance;
                    }
                    else if (newDistance < _averageGazeDistance)
                    {
                        _averageGazeDistance -= _sensitivity * newDistance;
                    }
                //}
                _lastGazeDir = gazeRay.Direction;
            }
        }

        float magnification = 1f + (_averageGazeDistance * _convergenceSpeed);

        if (debugMode)
        {
            if (_isResetting)
            {
                _debugText.color = Color.yellow;
            }
            else if (magnification < 1f)
            {
                _debugText.color = Color.red;
            }
            else
            {
                _debugText.color = Color.green;
            }

            _debugText.text = "Sensitivity: " + _sensitivity + "\nConvergence Speed: " + _convergenceSpeed + "\nMagnification: " + magnification;
        }
        return magnification;
    }

}
