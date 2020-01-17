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

    private float _gazeRange = 500f;

    private float _sensitivity = 0.027f;

    private float _distMultiplier = 0.12f;

    private float _idleResetTime = 1f;

    private float _timeAtLastSample;

    private bool _isResetting = false;

    private float _lastGazeDistance = 0f;

    private float _averageGazeDistance;

    private Vector3 _lastDotPos;

    private Vector3 _targetDotPos;

    private const int _numFramesToSample = 30;

    private int _numInertialFramesToSample = _numFramesToSample * 3;

    private int _frameIndex = 0;

    private int _inertialFrameIndex = 0;

    private Vector3[] _sampledPoints;

    private float[] _sampledDistances;

    private Vector3 _averageDirection;

    private float _lastStableDistance;

    private float _resetTime = 1f;

    private float _timePassed = 0f;

    private float _lastMag = 0f;

    private Vector3 _gazeSceenPos;

    private Vector3? _teleportCandidate;

    private Vector3 _lastPos;

    private float _eyesClosedTime = 0f;

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
        _timeAtLastSample = Time.time;

        _averageDirection = Vector3.zero;

        _screnDot = GameObject.FindGameObjectWithTag("GazeDotScreen")?.transform;
        _worldDot = GameObject.FindGameObjectWithTag("GazeDotWorld")?.transform;
        _averageDot = GameObject.FindGameObjectWithTag("AverageDot")?.transform;

        _magCamera = magGlass.GetComponentInChildren<Camera>();

        _sampledPoints = new Vector3[_numFramesToSample];
        _sampledDistances = new float[_numInertialFramesToSample];

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

    public float GetMagnification(RaycastHit gazePoint, Vector3 planeNormal, bool debugMode)
    {
        if (Time.time - _timeAtLastSample > _idleResetTime)
        {
            // Reset
            _frameIndex = 0;
        }

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

        if (TobiiXR.EyeTrackingData.IsLeftEyeBlinking && TobiiXR.EyeTrackingData.IsRightEyeBlinking)
        {
            if (!_teleportCandidate.HasValue)
            {
                _teleportCandidate = _averageDot.position;
                Debug.Log("Closing eyes");
            }
            _eyesClosedTime += Time.deltaTime;
            if (_eyesClosedTime >= 2f)
            {
                _player.position = _teleportCandidate.Value;
                _teleportCandidate = null;
                _eyesClosedTime = 0f;
            }
            return _lastMag;
        }
        else if (_teleportCandidate.HasValue)
        {
            if (!TobiiXR.EyeTrackingData.ConvergenceDistanceIsValid || !TobiiXR.EyeTrackingData.GazeRay.IsValid)
            {
                return _lastMag;
            }

            Debug.Log("Opened eyes after " + _eyesClosedTime + " seconds");
            _teleportCandidate = null;
            _eyesClosedTime = 0f;
        }

        float eyeVelocity = 0f;
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
            _gazeSceenPos = gazePoint.textureCoord;
            Ray magRay = _magCamera.ViewportPointToRay(_gazeSceenPos);

            Vector3 hitPos;
            if (Physics.Raycast(magRay, out RaycastHit hit, _gazeRange))
            {
                hitPos = hit.point;
            }
            else
            {
                Debug.Log("Looking outside range");
                hitPos = gazePoint.point + (magRay.direction * _gazeRange);
            }

            _sampledPoints[_frameIndex] = hitPos;

            eyeVelocity = Vector3.Distance(_lastDotPos, hitPos) / Time.deltaTime;
            int k = (int) Mathf.Min(_numFramesToSample * (eyeVelocity * _sensitivity), _numFramesToSample);

            Vector3 dotPos = Vector3.zero;
            _frameIndex = (_frameIndex + 1) % _numFramesToSample;

            int i = _frameIndex - k;

            if (i < 0)
            {
                i += _numFramesToSample;
            }
            Debug.Assert(i >= 0 && i < _numFramesToSample, "Position average error: i is " + i);

            int samplesUsed = 0;
            do
            {
                dotPos += _sampledPoints[i];
                i = (i + 1) % _numFramesToSample;
                samplesUsed++;
            }
            while (samplesUsed < k);
            dotPos /= k;
            _averageDot.position = dotPos;

            // Reset zoom on left trigger click
            if (SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.LeftHand].stateDown)
            {
                _isResetting = true;
                _frameIndex = 0;
                _timePassed = 0f;
                _lastStableDistance = _averageGazeDistance;
            }
        }

        Vector3 eyeBallPos = TobiiXR.EyeTrackingData.GazeRay.Origin;
        float distToDot = Vector3.Distance(eyeBallPos, _averageDot.position);

        float magnification = 1f + (GetWeightedAverageDist(distToDot, eyeVelocity) * _distMultiplier);

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
        _lastMag = magnification;
        _timeAtLastSample = Time.time;
        return magnification;
    }

    // Take weighted average of weighted average distances
    private float GetWeightedAverageDist(float currentDist, float eyeVelocity)
    {
        Debug.Assert(_inertialFrameIndex >= 0 && _inertialFrameIndex < _numInertialFramesToSample);

        _sampledDistances[_inertialFrameIndex] = currentDist;

        int k = (int)Mathf.Min(_numInertialFramesToSample * (eyeVelocity * _sensitivity), _numInertialFramesToSample);
        float averageDist = 0f;
        _inertialFrameIndex = (_inertialFrameIndex + 1) % _numInertialFramesToSample;

        int i = _inertialFrameIndex - k;
        if (i < 0)
        {
            i += _numInertialFramesToSample;
        }
        Debug.Assert(i >= 0 && i < _numInertialFramesToSample, "Distance average error: i is " + i);

        int samplesUsed = 0;
        do
        {
            averageDist += _sampledDistances[i];
            i = (i + 1) % _numInertialFramesToSample;
            samplesUsed++;
        }
        while (samplesUsed < k);
        averageDist /= k;
        return averageDist;
    }

}
