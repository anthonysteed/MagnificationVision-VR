using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeItemDetector : MonoBehaviour
{
    [SerializeField]
    private float _detectionTime = 2f;

    private MagnificationManager _magManager;

    private GazeMagnifier _gazeMagnifier;

    private HiddenItem _gazedAtItem;

    private Queue<Collider> _gazeCandidates;

    private bool _isDiscoveryPending = false;

    private float _timeGazed = 0f;

    private void Awake()
    {
        _magManager = FindObjectOfType<MagnificationManager>();
        _gazeMagnifier = FindObjectOfType<GazeMagnifier>();
    }

    // Or null
    private HiddenItem GetCurrentGazedAtItem()
    {
        if (_magManager.IsMagnifying)
        {
            // Check gaze ray going through MagRect
            Collider col = _gazeMagnifier.LastGazeTarget;
            if (col != null)
            {
                return col.GetComponent<HiddenItem>();
            }
        }
        else
        {
            // Check normal gaze ray
            Collider col = _magManager.LastGazeTarget;
            if (col != null)
            {
                return col.GetComponent<HiddenItem>();
            }
        }
        return null;
    }




    private void Update()
    {
        HiddenItem item = GetCurrentGazedAtItem();   

        if (_isDiscoveryPending)
        {
            // Player looked away -- cancel discovery
            if (item == null || item != _gazedAtItem)
            {
                _isDiscoveryPending = false;
            }
            else
            {
                _timeGazed += Time.deltaTime;
                if (_timeGazed >= _detectionTime)
                {
                    _gazedAtItem.Discover();
                    _isDiscoveryPending = false;
                }
            }
        }
        if (item != null && !_isDiscoveryPending)
        {
            _gazedAtItem = item;
            _timeGazed = 0f;
            _isDiscoveryPending = true;
        }

    }


}
