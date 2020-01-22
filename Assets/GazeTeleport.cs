using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GazeTeleport : MonoBehaviour
{
    public bool IsTeleportPending { get { return _teleportCandidate.HasValue; } }

    private GazeMagnifier _gazeMag;

    private TeleportPerson _playerToTeleport;

    private Vector3? _teleportCandidate;

    private float _holdDownTime = 0f;

    private void Awake()
    {
        _playerToTeleport = FindObjectOfType<TeleportPerson>();
        _gazeMag = FindObjectOfType<GazeMagnifier>();
    }

    private void Update()
    {
        SteamVR_Action_Boolean_Source triggerDown = SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.RightHand];
        if (triggerDown.state)
        {
            if (!_teleportCandidate.HasValue)
            {
                _teleportCandidate = _gazeMag.LastGazePos;
                Debug.Log("Pending teleport...");
            }
            _holdDownTime += Time.deltaTime;
            if (_holdDownTime >= 2f)
            {
                // Start teleport
                Debug.Log("Teleporting to " + _teleportCandidate.Value);
                _playerToTeleport.Teleport(_teleportCandidate.Value);
                _teleportCandidate = null;
                _holdDownTime = 0f;
            }
        }
        else if (_teleportCandidate.HasValue)
        {
            Debug.Log("Released teleport trigger after " + _holdDownTime + " seconds");
            _teleportCandidate = null;
            _holdDownTime = 0f;
        }
    }

}
