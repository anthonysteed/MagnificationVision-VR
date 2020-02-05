﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class GazeTeleport : MonoBehaviour
{
    public delegate void GazeTeleportEvent(Vector3 destination);
    public static event GazeTeleportEvent OnGazeTeleport;

    public bool IsTeleportPending { get { return _teleportCandidate.HasValue; } }

    [SerializeField]
    private float _activationTime = 1f;

    private GazeDotIndicator _dotImage;

    private MagnificationManager _magManager;

    private GazeMagnifier _gazeMag;

    private TeleportPoint _teleportMarker;

    private TeleportMarkerCollider _markerCollider;

    private Teleporter _teleporter;

    private Vector3? _teleportCandidate;

    private Transform _player;

    private Transform _playspace;

    private float _holdDownTime = 0f;

    private bool _isHoldingTrigger = false;

    private BufferedLogger _log = new BufferedLogger("GazeTeleport");

    private void Awake()
    {
        _teleporter = FindObjectOfType<Teleporter>();
        _gazeMag = FindObjectOfType<GazeMagnifier>();
        _magManager = FindObjectOfType<MagnificationManager>();
        _teleportMarker = FindObjectOfType<TeleportPoint>();
        _markerCollider = FindObjectOfType<TeleportMarkerCollider>();
        _dotImage = GetComponent<GazeDotIndicator>();

        _player = Camera.main.transform;
        _playspace = _player.parent;
    }

    private void Update()
    {
        if (!_magManager.IsMagnifying)
        {
            return;
        }

        // Set + adjust position once per frame
        SetMarkerPosition();
        // If marker still collides after adjustment, target cannot be teleported to
        bool isTargetValid = IsTeleportTargetValid();
        _dotImage.SetValid(isTargetValid);

        _log.Append("targetValid", isTargetValid);

        SteamVR_Action_Boolean_Source triggerDown = SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.RightHand];
        if (triggerDown.stateDown && !_teleporter.IsTeleporting)
        {
            if (isTargetValid)
            {
                _isHoldingTrigger = true;
                _holdDownTime = 0f;

                SetTeleportTarget();
                _teleportMarker.SetAlpha(1f, 1f);
            }
            else
            {
                _teleportCandidate = null;
                _teleportMarker.SetAlpha(0f, 0f);
                _log.CommitLine();
                return;
            }
        }
        if (triggerDown.stateUp)
        {
            _isHoldingTrigger = false;
            _teleportCandidate = null;
            _teleportMarker.SetAlpha(0f, 0f);
        }

        if (_isHoldingTrigger)
        {
            _holdDownTime += Time.deltaTime;
            _dotImage.SetProgress(_holdDownTime / _activationTime);
            if (_holdDownTime >= _activationTime)
            {
                _isHoldingTrigger = false;
                _log.Append("isTeleporting", true);

                // Start teleport
                _teleporter.Teleport(_teleportCandidate.Value);
                OnGazeTeleport(_teleportCandidate.Value);
                _teleportCandidate = null;
            }
        }
        else
        {
            _teleportMarker.SetAlpha(0f, 0f);
            _dotImage.SetProgress(1f);
        }
        _log.CommitLine();
    }

    private void SetMarkerPosition()
    {
        Vector3 target = _gazeMag.LastGazePos;
        target.y = 0f;
        _teleportMarker.transform.position = target;

        if (_markerCollider.HasCollided())
        {
            _teleportMarker.transform.position = _markerCollider.GetAdjustedPosition();
        }
    }


    private bool IsTeleportTargetValid()
    {
        Vector3 target = _gazeMag.LastGazePos;
        if (target.y > _player.position.y * 2f || _markerCollider.HasCollided())
        {
            return false;
        }
        return true;
    }

    private void SetTeleportTarget()
    {
        Vector3 target = _gazeMag.LastGazePos;
        _log.Append("origTarget", target);

        target.y = 0f;
        _teleportMarker.transform.position = target;

        if (_markerCollider.HasCollided())
        {
            target = _markerCollider.GetAdjustedPosition();
        }
        _teleportMarker.transform.position = target;
        _log.Append("shiftedTarget", target);

        _teleportCandidate = target;
    }

}
