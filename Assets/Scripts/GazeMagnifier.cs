using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class GazeMagnifier : MonoBehaviour, IMagnifier
{
    // World space pos. of gaze dot last frame
    public Vector3 LastGazePos { get; private set; }

    public float LastGazeDistance { get; private set; }

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

    private WorldGazeTracker _gazeTracker;

    private Transform _gazeDot;

    private Camera _magCamera;

    // Ignore teleport marker
    private int _layerMask = ~(1 << 13);

    // Avg. gaze distance calculated last frame
    private float _oldAverageDist;

    private bool _isTeleporting = false;

    private float _lastMagnification = 1f;

    private BufferedLogger _log = new BufferedLogger("GazeMag");

    private void OnEnable()
    {
        GazeTeleport.OnGazeTeleport += OnGazeTeleport;
        Teleporter.OnTeleportComplete += OnTeleportComplete;
    }

    private void OnDisable()
    {
        GazeTeleport.OnGazeTeleport -= OnGazeTeleport;
        Teleporter.OnTeleportComplete -= OnTeleportComplete;
    }

    private void Awake()
    {
        _magRect = GameObject.FindGameObjectWithTag("MagRect").transform;
        _gazeTracker = FindObjectOfType<WorldGazeTracker>();
        _player = Camera.main.transform;

        _timeAtLastSample = Time.time;

        _gazeDot = GameObject.FindGameObjectWithTag("GazeDot")?.transform;

        _magCamera = _magRect.GetComponentInChildren<Camera>();

        _numInertialFramesToSample = _numFramesToSample * 4;

        _sampledPoints = new Vector3[_numFramesToSample];
        _sampledDistances = new float[_numInertialFramesToSample];
    }

    private void Update()
    {
        if (Time.time - _timeAtLastSample > _idleResetTime && _isMagActive)
        {
            _isMagActive = false;
        }
    }

    private void ResetZoom(bool toWorldGaze)
    {
        _frameIndex = 0;
        _inertialFrameIndex = 0;
        LastGazePos = _player.position;
        _oldAverageDist = 0f;

        for (int i = 0; i < _numFramesToSample; i++)
        {
            _sampledPoints[i] = toWorldGaze ? _gazeTracker.GazePos : _gazeDot.position;
        }

        Vector3 eyeBallsPos = TobiiXR.EyeTrackingData.GazeRay.Origin;
        float gazeDistEstimate = Vector3.Distance(eyeBallsPos, _gazeTracker.GazePos) / 2f;
        for (int i = 0; i < _numInertialFramesToSample; i++)
        {
            _sampledDistances[i] = toWorldGaze ? gazeDistEstimate : 0f;
        }

        _oldAverageDist = toWorldGaze ? gazeDistEstimate : 0f;

        _log.Append("didReset", true);
    }

    private void OnGazeTeleport(Vector3 destination)
    {
        _isTeleporting = true;
        // Extra zoom effect
        //float dist = Vector3.Distance(_player.position, destination) * 6f;
        //for (int j = 0; j < _numInertialFramesToSample; j++)
        //{
        //    _sampledDistances[_inertialFrameIndex] = dist;
        //    _inertialFrameIndex = (_inertialFrameIndex + 1) % _numInertialFramesToSample;
        //}
    }

    private void OnTeleportComplete()
    {
        _isTeleporting = false;
        ResetZoom(false);
    }


    // Called every frame when magnification active
    public float GetMagnification(RaycastHit gazePoint, Vector3 planeNormal)
    {
        if (!_isMagActive)
        {
            _isMagActive = true;
            ResetZoom(true);
        }
        if (_isTeleporting)
        {
            _log.CommitLine();
            return _lastMagnification;
        }

        _log.Append("active", true);
        Vector3 gazeSceenPos = gazePoint.textureCoord;
        Ray magRay = _magCamera.ViewportPointToRay(gazeSceenPos);

        Vector3 hitPos;
        if (Physics.Raycast(magRay, out RaycastHit hit, _gazeRange, _layerMask))
        {
            hitPos = hit.point;
            _log.Append("curFrameGazePos", hitPos);
        }
        else
        {
            Debug.Log("Looking outside range");
            hitPos = gazePoint.point + (magRay.direction * _gazeRange);
            _log.Append("outsideRange", true);
        }

        _sampledPoints[_frameIndex] = hitPos;

        Vector3 dotPos = Vector3.zero;
        _frameIndex = (_frameIndex + 1) % _numFramesToSample;
        int i = _frameIndex;
        for (int s = 0; s < _numFramesToSample; s++)
        {
            dotPos += _sampledPoints[i];
            i = (i + 1) % _numFramesToSample;
        }
        dotPos /= _numFramesToSample;
        _log.Append("dotPos", dotPos);

        _gazeDot.position = dotPos - (0.1f * magRay.direction);
        _gazeDot.rotation = Quaternion.LookRotation(_player.forward, _player.up);

        LastGazePos = dotPos;

        Vector3 eyeBallPos = TobiiXR.EyeTrackingData.GazeRay.Origin;
        LastGazeDistance = Vector3.Distance(eyeBallPos, _gazeDot.position);

        float magnification = 1f + (GetWeightedAverageDist(LastGazeDistance) * _distMultiplier);

        _timeAtLastSample = Time.time;
        _lastMagnification = magnification;

        _log.CommitLine();
        return magnification;
    }

    // Take weighted average of weighted average distances
    private float GetWeightedAverageDist(float currentDist)
    {
        Debug.Assert(_inertialFrameIndex >= 0 && _inertialFrameIndex < _numInertialFramesToSample);
        _log.Append("curDotDist", currentDist);

        _sampledDistances[_inertialFrameIndex] = currentDist;
        _inertialFrameIndex = (_inertialFrameIndex + 1) % _numInertialFramesToSample;

        float averageDist = 0f;
        int i = _inertialFrameIndex;

        for (int s = 0; s < _numInertialFramesToSample; s++)
        {
            averageDist += _sampledDistances[i];
            i = (i + 1) % _numInertialFramesToSample;
        }
        averageDist /= _numInertialFramesToSample;

        // exponential moving average
        averageDist = (averageDist * _sampleAlpha) + ((1 - _sampleAlpha) * _oldAverageDist);
        _oldAverageDist = averageDist;

        _log.Append("avgDotDist", averageDist);
        return averageDist;
    }

}
