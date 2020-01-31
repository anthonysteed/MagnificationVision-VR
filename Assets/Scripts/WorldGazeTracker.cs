using System.Collections;
using System.Collections.Generic;
using Tobii.XR;
using UnityEngine;

public class WorldGazeTracker : MonoBehaviour
{
    public Ray LastGazeRay { get; private set; }

    public Vector3 GazePos { get; private set; }

    [SerializeField]
    private int _numFramesToBuffer = 30;

    private int _gazeBufferIndex = 0;

    private Vector3[] _bufferedGazePositions;

    private void Awake()
    {
        _bufferedGazePositions = new Vector3[_numFramesToBuffer];
    }

    private void Update()
    {
        TobiiXR_GazeRay gazeRay = TobiiXR.EyeTrackingData.GazeRay;

        if (gazeRay.IsValid && Physics.Raycast(gazeRay.Origin, gazeRay.Direction, out RaycastHit hit, 10f))
        {
            LastGazeRay = new Ray(gazeRay.Origin, gazeRay.Direction);

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
            GazePos = averageGazePos;
        }
    }

}
