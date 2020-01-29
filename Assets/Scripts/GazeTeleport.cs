﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class GazeTeleport : MonoBehaviour
{
    public bool IsTeleportPending { get { return _teleportCandidate.HasValue; } }

    [SerializeField]
    private float _activationTime = 1f;

    private MagnificationManager _magManager;

    private GazeMagnifier _gazeMag;

    private TeleportPoint _teleportMarker;

    private TeleportMarkerCollider _markerCollider;

    private Teleporter _teleporter;

    private Vector3? _teleportCandidate;

    private Transform _player;

    private Transform _playspace;

    private Image _gazeDotImage;

    private float _imageAlpha;

    private float _holdDownTime = 0f;

    private void Awake()
    {
        _teleporter = FindObjectOfType<Teleporter>();
        _gazeMag = FindObjectOfType<GazeMagnifier>();
        _magManager = FindObjectOfType<MagnificationManager>();
        _teleportMarker = FindObjectOfType<TeleportPoint>();
        _markerCollider = FindObjectOfType<TeleportMarkerCollider>();
        _gazeDotImage = GetComponentInChildren<Image>();
        _imageAlpha = _gazeDotImage.color.a;

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
        SetDotColour(isTargetValid);

        SteamVR_Action_Boolean_Source triggerDown = SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.RightHand];
        if (triggerDown.state && !_teleporter.IsTeleporting)
        {
            if (!_teleportCandidate.HasValue)
            {
                if (isTargetValid)
                {
                    SetTeleportTarget();
                    _teleportMarker.SetAlpha(1f, 1f);
                }
                else
                {
                    //_teleportMarker.SetAlpha(0f, 0f);
                    return;
                }
            }
            _holdDownTime += Time.deltaTime;
            _gazeDotImage.fillAmount = _holdDownTime / _activationTime;
            if (_holdDownTime >= _activationTime)
            {
                // Start teleport
                _teleporter.Teleport(_teleportCandidate.Value);
                _teleportCandidate = null;
                _holdDownTime = 0f;
            }
        }
        else if (_teleportCandidate.HasValue)
        {
            _teleportCandidate = null;
            _holdDownTime = 0f;
        }
        else
        {
            //_teleportMarker.SetAlpha(0f, 0f);
            _gazeDotImage.fillAmount = 1f;
        }
    }

    private void SetDotColour(bool isValid)
    {
        Color color = isValid ? Color.green : Color.red;
        color.a = _imageAlpha;
        _gazeDotImage.color = color;
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
        target.y = 0f;
        _teleportMarker.transform.position = target;

        if (_markerCollider.HasCollided())
        {
            target = _markerCollider.GetAdjustedPosition();
        }
        _teleportMarker.transform.position = target;

        _teleportCandidate = target;
    }

}
