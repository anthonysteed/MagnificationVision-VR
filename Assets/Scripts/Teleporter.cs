// via https://www.youtube.com/watch?v=-T09oRMDuG8

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Teleporter : MonoBehaviour
{
    public bool IsArcActive { get; private set; }

    [SerializeField]
    private float _fadeTime = 0.5f;

    [SerializeField]
    private float _maxDistance = 10f;

    private Transform _player;

    private Transform _playSpace;

    private Transform _aimAnchor;

    private TeleportArc _arc;

    private TeleportPoint _teleportMarker;

    private TeleportMarkerCollider _markerCollider;

    private SteamVR_Behaviour_Pose _handPose;

    private bool _isTeleporting = false;

    private bool _isArcTargetValid = false;

    private Vector3 _arcTarget;

    private void Awake()
    {
        _handPose = GetComponent<SteamVR_Behaviour_Pose>();
        _player = Camera.main.transform;
        _playSpace = _player.parent;
        _arc = GetComponentInChildren<TeleportArc>();
        _teleportMarker = FindObjectOfType<TeleportPoint>();
        _markerCollider = _teleportMarker.GetComponentInChildren<TeleportMarkerCollider>();
        _arc.traceLayerMask = ~((1 << 13) | (1 << 9)); // ignore teleport marker
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
            Teleport(_arcTarget);
        }
    }

    private void UpdateArc()
    {
        if (SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.RightHand].axis == Vector2.zero)
        {
            _arc.Hide();
            _teleportMarker.SetAlpha(0f, 0f);
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

    public void Teleport(Vector3 targetPos)
    {
        if (_isTeleporting)
        {
            return;
        }
        Vector3 target;
        if (_markerCollider.HasCollided())
        {
            target = _markerCollider.GetAdjustedPosition();
            target.y = targetPos.y;
        }
        else
        {
            target = targetPos;
        }

        Vector3 playerPos = new Vector3(_player.position.x, _playSpace.position.y, _player.position.z);
        StartCoroutine(MovePlaySpace(target - playerPos));
    }

    private IEnumerator MovePlaySpace(Vector3 translation)
    {
        Debug.Log("Teleport-moving by " + translation);
        _isTeleporting = true;
        SteamVR_Fade.Start(Color.white, _fadeTime, true);

        yield return new WaitForSeconds(_fadeTime);
        _playSpace.position += translation;

        SteamVR_Fade.Start(Color.clear, _fadeTime, true);

        _isTeleporting = false;
    }

}
