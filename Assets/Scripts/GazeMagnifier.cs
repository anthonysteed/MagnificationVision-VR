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

    private Vector3 _lastValidEyePos;

    // private Transform _gazeDot;

    private Text _debugText;

    private float _gazeRange = 100f;

    private float _sensitivity = 0.027f;

    private float _distMultiplier = 0.12f;

    private bool _isResetting = false;

    private float _lastGazeDistance = 0f;

    private float _averageGazeDistance;

    private Vector3 _lastGazeDir;

    private Vector3 _averageGazeDir;

    private int _framesPerSample;

    private int _framesPassed = 0;

    private Vector3[] _sampledPoints;

    private Vector3 _averageDirection;

    private float _lastStableDistance;

    private float _resetTime = 1f;

    private float _timePassed = 0f;

    private Vector3 _planeIntersection;

    private Vector3 _gazeSceenPos;

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
        _lastValidEyePos = player.position;

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
               _distMultiplier += rightHandTouch.delta.y * 0.1f;
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
            // Find gaze-plane intersection
            TobiiXR_GazeRay gazeRay = TobiiXR.EyeTrackingData.GazeRay;
            RaycastHit screenHit;
            if (gazeRay.IsValid && Physics.Raycast(gazeRay.Origin, gazeRay.Direction, out screenHit, _gazeRange) && screenHit.collider.transform == _magGlass)
            {
                _lastValidEyePos = gazeRay.Origin;

                _planeIntersection = screenHit.point;
                _gazeSceenPos = screenHit.textureCoord;

                RaycastHit hit;
                Ray magRay = _magCamera.ViewportPointToRay(_gazeSceenPos);

                Vector3 hitPos;
                if (Physics.Raycast(magRay, out hit, _gazeRange))
                {
                    hitPos = hit.point;
                }
                else
                {
                    hitPos = _planeIntersection + (magRay.direction * _gazeRange);
                }

                _sampledPoints[_framesPassed] = hitPos;
            }
            else
            {
                if (_framesPassed == 0)
                {
                    _sampledPoints[0] = Vector3.zero;
                }
                else
                {
                    _sampledPoints[_framesPassed] = _sampledPoints[_framesPassed - 1];
                }
                
            }

            _framesPassed++;
            if (_framesPassed >= _framesPerSample)
            {
                Vector3 averageDir = Vector3.zero;
                float averageDist = 0f;
                foreach (Vector3 point in _sampledPoints)
                {
                    Vector3 toPoint = point - _lastValidEyePos;
                    averageDir += toPoint;
                    averageDist += toPoint.magnitude;
                }
                averageDir /= _framesPerSample;
                averageDist /= _framesPerSample;

                _lastGazeDistance = _averageGazeDistance;
                _lastGazeDir = _averageDirection;

                _averageGazeDistance = averageDist;
                _averageGazeDir = averageDir;

                _framesPassed = 0;
            }

            // Reset zoom on left trigger click
            if (SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.LeftHand].stateDown)
            {
                _isResetting = true;
                _framesPassed = 0;
                _timePassed = 0f;
                _lastStableDistance = _averageGazeDistance;
            }
        }

        float t = ((float)_framesPassed) / ((float)_framesPerSample);
        float lerpedDist = Mathf.Lerp(_lastGazeDistance, _averageGazeDistance, t);
        Vector3 lerpedDir = Vector3.Lerp(_lastGazeDir, _averageGazeDir, t);
        _averageDot.position = lerpedDir * lerpedDist;

        float magnification = 1f + (lerpedDist * _distMultiplier);

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

            _debugText.text = "Sensitivity: " + _sensitivity + "\nConvergence Speed: " + _distMultiplier + "\nMagnification: " + magnification;
        }
        return magnification;
    }

}
