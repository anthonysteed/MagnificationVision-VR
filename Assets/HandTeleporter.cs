using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class HandTeleporter : MonoBehaviour
{
    public bool IsArcActive { get; private set; }

    [SerializeField]
    private float _maxDistance = 10f;

    private Teleporter _teleporter;

    private Transform _aimAnchor;

    private TeleportArc _arc;

    private TeleportPoint _teleportMarker;

    private TeleportMarkerCollider _markerCollider;

    private bool _isArcTargetValid = false;

    private Vector3 _arcTarget;

    private void Awake()
    {
        _teleporter = FindObjectOfType<Teleporter>();
        _arc = GetComponentInChildren<TeleportArc>();
        _teleportMarker = FindObjectOfType<TeleportPoint>();
        _markerCollider = _teleportMarker.GetComponentInChildren<TeleportMarkerCollider>();
        _arc.traceLayerMask = ~((1 << 13) | (1 << 9)); // ignore teleport marker mag. rect
        _aimAnchor = _arc.transform;
    }

    private void Start()
    {
        _teleportMarker.SetAlpha(0f, 0f);
    }

    private void Update()
    {
        UpdateArc();
        if (SteamVR_Actions.default_Teleport[SteamVR_Input_Sources.RightHand].stateDown && _isArcTargetValid)
        {
            _teleporter.Teleport(_arcTarget);
        }
    }

    private void UpdateArc()
    {
        if (SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.RightHand].axis == Vector2.zero)
        {
            if (IsArcActive)
            {
                _arc.Hide();
                _teleportMarker.SetAlpha(0f, 0f);
            }
            IsArcActive = false;
            return;
        }

        IsArcActive = true;
        Vector3 arcVelocity = _aimAnchor.forward * _maxDistance;
        _arc.SetArcData(_aimAnchor.position, arcVelocity, true, false);

        bool didHit = _arc.DrawArc(out RaycastHit hit);
        if (didHit)
        {
            _arcTarget = hit.point;
            _isArcTargetValid = true;
            _arc.SetColor(Color.green);

            _teleportMarker.transform.position = hit.point;
            Vector3 markerPos;
            if (_markerCollider.HasCollided())
            {
                markerPos = _markerCollider.GetAdjustedPosition();
                markerPos.y = hit.point.y;
                _teleportMarker.transform.position = markerPos;
            }

            _teleportMarker.SetAlpha(1f, 1f);
        }

        if (!didHit || hit.collider.gameObject.layer != 12)
        {
            _arc.SetColor(Color.red);
            _teleportMarker.SetAlpha(0f, 0f);
            _isArcTargetValid = false;
        }
        _arc.Show();
    }

}
