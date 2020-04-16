﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeItemDetector : MonoBehaviour
{
    [SerializeField]
    private float _detectionTime = 2f;

    private MagnificationManager _magManager;

    private WorldGazeTracker _worldGaze;

    private GazeMagnifier _gazeMagnifier;

    private HiddenItem[] _allItems;

    private HiddenItem _gazedAtItem;

    private Camera _playerCam;

    private Camera _magRectCam;

    private bool _isDiscoveryPending = false;

    private float _timeGazed = 0f;

    private BufferedLogger _log = new BufferedLogger("Items");

    private void Awake()
    {
        _magManager = FindObjectOfType<MagnificationManager>();
        _gazeMagnifier = FindObjectOfType<GazeMagnifier>();
        _worldGaze = FindObjectOfType<WorldGazeTracker>();
        _allItems = FindObjectsOfType<HiddenItem>();

        _playerCam = Camera.main;
        _magRectCam = _magManager.GetComponentInChildren<Camera>();
    }

    // Or null
    private HiddenItem GetItemAtPoint(Vector3 pos)
    {
        // Check all objects in 0.2m radius of gaze point
        Collider[] collidersSeen = Physics.OverlapSphere(pos, 0.2f);
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

    // Or null
    private HiddenItem GetCurrentGazedAtItem()
    {
        // Check regular gaze ray
        HiddenItem item =  GetItemAtPoint(_worldGaze.GazePos);
        if (item == null && _magManager.IsMagnifying)
        {
            // Check gaze ray through MagRect
            item = GetItemAtPoint(_gazeMagnifier.LastGazePos);
        }
        return item;       
    }

    private bool InViewOf(Vector3 point, Camera cam)
    {
        const int layerMask = ~((1 << 9) | (1 << 10) | (1 << 11) | (1 << 13));

        // Check if inside camera's viewport
        Vector3 vp = cam.WorldToViewportPoint(point);
        if (vp.x < 1 && vp.x > 0 && vp.y < 1 && vp.y > 0 && vp.y < 1 && vp.z > 0)
        {
            // Check that nothing is in the way
            if (!Physics.Linecast(_playerCam.transform.position, point, layerMask))
            {
                return true;
            }
        }
        return false;
    }

    private void CheckFov()
    {
        foreach (HiddenItem item in _allItems)
        {
            if (item.Found)
            {
                continue;
            }
            
            if (InViewOf(item.transform.position, _playerCam))
            {
                Debug.Log(item.ItemType + " visible to player");
                _log.Append(item.ItemType + "_inPlayerFov", true);
            }
            if (_magManager.IsMagnifying && InViewOf(item.transform.position, _magRectCam))
            {
                Debug.Log(item.ItemType + " visible through rect");
                _log.Append(item.ItemType + "_inRectFov", true);
            }
        }
    }


    private void Update()
    {
        CheckFov();
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

                    _log.Append(item.ItemType + "_discovered", true);
                }
            }
        }
        if (item != null && !_isDiscoveryPending)
        {
            _gazedAtItem = item;
            _timeGazed = 0f;
            _isDiscoveryPending = true;

            _log.Append(item.ItemType + "_firstSpotted", true);
        }

        _log.CommitLine();
    }


}
