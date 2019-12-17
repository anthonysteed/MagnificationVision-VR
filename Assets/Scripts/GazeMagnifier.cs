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

    // private Transform _gazeDot;

    private Text _debugText;

    private float _gazeRange = 100f;

    private float _sensitivity = 0.027f;

    private float _convergenceSpeed = 0.12f;

    private bool _isResetting = false;

    private float _averageGazeDistance;

    private int _framesPerSample;

    private int _framesPassed = 0;

    private Vector3[] _sampledPoints;

    private Vector3 _newDirection;

    private Vector3 _averageDirection;

    private float _realAverageDistance = 0f;

    private float _lastStableDistance;

    private float _resetTime = 1f;

    private float _timePassed = 0f;

    private Vector3 _lastGazeDir;

    private Vector3 _planeIntersection;

    private Vector3 _gazeSceenPos;

    private float _thresholdAngle = 1f;

    private Transform _screnDot;

    private Transform _worldDot;

    private Transform _averageDot;

    private LineRenderer[] _dotRenderers;

    private Camera _magCamera;


    public GazeMagnifier(Transform player, Transform magGlass, Text debugText)
    {
        _player = player;
        _magGlass = magGlass;
        _debugText = debugText;

        _averageDirection = Vector3.zero;

        _screnDot = GameObject.FindGameObjectWithTag("GazeDotScreen")?.transform;
        _worldDot = GameObject.FindGameObjectWithTag("GazeDotWorld")?.transform;
        _averageDot = GameObject.FindGameObjectWithTag("AverageDot")?.transform;

        _magCamera = magGlass.GetComponentInChildren<Camera>();

        _dotRenderers = new LineRenderer[] { _screnDot.GetComponent<LineRenderer>(), _worldDot.GetComponent<LineRenderer>() };
        foreach (LineRenderer renderer in _dotRenderers)
        {
            renderer.enabled = false;
        }
        //if (_gazeDot == null)
        //{
        //    Debug.LogError("Couldn't find gaze dot");
        //}
    }

    public float GetMagnification(Vector3 planeNormal, bool debugMode)
    {
        foreach (LineRenderer renderer in _dotRenderers)
        {
            renderer.enabled = true;
        }

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
            float newDistance;
            // Find gaze-plane intersection
            TobiiXR_GazeRay gazeRay = TobiiXR.EyeTrackingData.GazeRay;
            RaycastHit screenHit;
            if (gazeRay.IsValid && Physics.Raycast(gazeRay.Origin, gazeRay.Direction, out screenHit, _gazeRange) && screenHit.collider.transform == _magGlass)
            {
                _planeIntersection = screenHit.point;
                _gazeSceenPos = screenHit.textureCoord;

                _dotRenderers[0].SetPosition(0, gazeRay.Origin);
                _dotRenderers[0].SetPosition(1, _planeIntersection);
                // _screnDot.rotation = Quaternion.LookRotation(gazeRay.Direction, _player.up);

                RaycastHit hit;
                Ray magRay = _magCamera.ViewportPointToRay(_gazeSceenPos);

                _dotRenderers[1].SetPosition(0, _planeIntersection);
                Vector3 hitPos;
                if (Physics.Raycast(magRay, out hit, _gazeRange))
                {
                    newDistance = Vector3.Distance(_planeIntersection, hit.point);
                    hitPos = hit.point;
                }
                else
                {
                    newDistance = _gazeRange;
                    hitPos = _planeIntersection + (magRay.direction * _gazeRange);
                }
                _dotRenderers[1].SetPosition(1, hitPos);

                _newDirection = hitPos - gazeRay.Origin;

                // _worldDot.rotation = Quaternion.LookRotation(gazeRay.Direction, _player.up);
            }
            else
            {
                newDistance = 0f;
                foreach (Renderer renderer in _dotRenderers)
                {
                    renderer.enabled = false;
                }
            }

            if (newDistance > _averageGazeDistance)
            {
                _averageGazeDistance += _sensitivity * (newDistance - _averageGazeDistance);
            }
            else if (newDistance < _averageGazeDistance)
            {
                _averageGazeDistance -= _sensitivity * (_averageGazeDistance - newDistance);
            }
            _averageDirection = (_averageDirection +  _newDirection).normalized;
            _averageDot.position = _averageDirection * _averageGazeDistance;
            _lastGazeDir = gazeRay.Direction;

            // Reset zoom on left trigger click
            if (SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.LeftHand].stateDown)
            {
                _isResetting = true;
                _timePassed = 0f;
                _lastStableDistance = _averageGazeDistance;
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
