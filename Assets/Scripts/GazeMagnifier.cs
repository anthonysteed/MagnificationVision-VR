using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class GazeMagnifier : MonoBehaviour, IMagnifier
{
    public Vector3 LastGazePos { get; private set; }

    public Collider LastGazeTarget { get; private set; }

    // How much to weigh the most recently sampled gaze distance
    [SerializeField]
    private float _sampleAlpha = 0.05f;

    // Max. distance of gaze ray
    [SerializeField]
    private float _gazeRange = 500f;

    [SerializeField]
    private float _distMultiplier = 0.8f;
    
    // How long after looking away to wait before reset magnification
    [SerializeField]
    private float _idleResetTime = 1f;

    // How many frames (n) position of gaze dot is determined from
    [SerializeField]
    private int _numFramesToSample = 30;

    private Transform _player;

    private Transform _magRect;

    private float _timeAtLastSample;

    // World space pos. of gaze dot last frame
    private Vector3 _lastDotPos;

    // How many frames to weigh average gaze distance from; defined in awake as 3n
    private int _numInertialFramesToSample;

    // Sampled world space gaze pos. for last n frames (stored as ring buffer)
    private Vector3[] _sampledPoints;

    // Index in sampled points of most recent sample (always mod n)
    private int _frameIndex = 0;

    // Sampled distances between player pos. and gaze pos. (stored as ring buffer)
    private float[] _sampledDistances;

    // Index in sampled distances of most recent sample (mod 3n)
    private int _inertialFrameIndex = 0;

    private bool _isMagActive = false;

    private Transform _averageDot;

    private Camera _magCamera;

    // Avg. gaze distance calculated last frame
    private float _oldAverageDist;

    private void Awake()
    {
        _magRect = GameObject.FindGameObjectWithTag("MagRect").transform;
        _player = Camera.main.transform;

        _timeAtLastSample = Time.time;

        _averageDot = GameObject.FindGameObjectWithTag("GazeDot")?.transform;

        _magCamera = _magRect.GetComponentInChildren<Camera>();

        _numInertialFramesToSample = _numFramesToSample * 3;

        _sampledPoints = new Vector3[_numFramesToSample];
        _sampledDistances = new float[_numInertialFramesToSample];
    }

    private void Update()
    {
        if (Time.time - _timeAtLastSample > _idleResetTime && _isMagActive)
        {
            // Reset
            _isMagActive = false;
            _frameIndex = 0;
            _inertialFrameIndex = 0;
            _lastDotPos = _player.position;
            _oldAverageDist = 0f;

            for (int i = 0; i < _numFramesToSample; i++)
            {
                _sampledPoints[i] = Vector3.zero;
            }
            for (int i = 0; i < _numInertialFramesToSample; i++)
            {
                _sampledDistances[i] = 0f;
            }
        }
    }

    // Called every frame when magnification active
    public float GetMagnification(RaycastHit gazePoint, Vector3 planeNormal)
    {
        _isMagActive = true;

        Vector3 gazeSceenPos = gazePoint.textureCoord;
        Ray magRay = _magCamera.ViewportPointToRay(gazeSceenPos);

        Vector3 hitPos;
        if (Physics.Raycast(magRay, out RaycastHit hit, _gazeRange))
        {
            hitPos = hit.point;
            LastGazeTarget = hit.collider;
        }
        else
        {
            Debug.Log("Looking outside range");
            hitPos = gazePoint.point + (magRay.direction * _gazeRange);
        }

        _sampledPoints[_frameIndex] = hitPos;

        float eyeVelocity = Vector3.Distance(_lastDotPos, hitPos) / Time.deltaTime;

        Vector3 dotPos = Vector3.zero;
        _frameIndex = (_frameIndex + 1) % _numFramesToSample;

        int i = _frameIndex;
        Debug.Assert(i >= 0 && i < _numFramesToSample, "Position average error: i is " + i);
        int samplesUsed = 0;
        do
        {
            dotPos += _sampledPoints[i];
            i = (i + 1) % _numFramesToSample;
            samplesUsed++;
        }
        while (samplesUsed < _numFramesToSample);
        dotPos /= _numFramesToSample;
        _averageDot.position = dotPos - (0.1f * magRay.direction);
        _averageDot.rotation = Quaternion.LookRotation(_player.forward, _player.up);

        LastGazePos = _averageDot.position;

        Vector3 eyeBallPos = TobiiXR.EyeTrackingData.GazeRay.Origin;
        float distToDot = Vector3.Distance(eyeBallPos, _averageDot.position);

        float magnification = 1f + (GetWeightedAverageDist(distToDot, eyeVelocity) * _distMultiplier);

        _timeAtLastSample = Time.time;
        return magnification;
    }

    // Take weighted average of weighted average distances
    private float GetWeightedAverageDist(float currentDist, float eyeVelocity)
    {
        Debug.Assert(_inertialFrameIndex >= 0 && _inertialFrameIndex < _numInertialFramesToSample);

        _sampledDistances[_inertialFrameIndex] = currentDist;

        float averageDist = 0f;
        _inertialFrameIndex = (_inertialFrameIndex + 1) % _numInertialFramesToSample;

        int i = _inertialFrameIndex;
        Debug.Assert(i >= 0 && i < _numInertialFramesToSample, "Distance average error: i is " + i);

        int samplesUsed = 0;
        do
        {
            averageDist += _sampledDistances[i];
            i = (i + 1) % _numInertialFramesToSample;
            samplesUsed++;
        }
        while (samplesUsed < _numInertialFramesToSample);

        averageDist /= _numInertialFramesToSample;

        // exponential moving average
        averageDist = (averageDist * _sampleAlpha) + ((1 - _sampleAlpha) * _oldAverageDist);
        _oldAverageDist = averageDist;

        return averageDist;
    }

}
