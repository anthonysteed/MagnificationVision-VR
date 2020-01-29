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
        Vector3 gazePos;
        if (_magManager.IsMagnifying)
        {
            // Check gaze ray through MagRect
            gazePos = _gazeMagnifier.LastGazePos;
        }
        else
        {
            // Check normal gaze ray
            gazePos = _magManager.LastWorldGazePos;
        }
        
        Collider[] collidersSeen = Physics.OverlapSphere(gazePos, 0.2f);
        foreach (Collider col in collidersSeen)
        {
            HiddenItem item = col.GetComponent<HiddenItem>();
            if (item != null)
            {
                return item;
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
            if (item == null)
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
