// via https://www.youtube.com/watch?v=-T09oRMDuG8

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    private float _fadeTime = 0.5f;

    [SerializeField]
    private float _maxDistance = 10f;

    private Transform _player;

    private Transform _playSpace;

    private Transform _aimAnchor;

    private TeleportArc _arc;

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
        _arc.traceLayerMask = ~0;
        _aimAnchor = _arc.transform;
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
            return;
        }

        Vector3 arcVelocity = _aimAnchor.forward * _maxDistance;
        _arc.SetArcData(_aimAnchor.position, arcVelocity, true, false);

        bool didHit = _arc.DrawArc(out RaycastHit hit);
        if (didHit)
        {
            _arcTarget = hit.point;
            _isArcTargetValid = true;
            _arc.SetColor(Color.green);

            Debug.Log("hit gameobject " + hit.collider.gameObject + " with layer " + hit.collider.gameObject.layer);

        }
        
        if (!didHit || hit.collider.gameObject.layer != 12)
        {
            _arc.SetColor(Color.red);
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

        Vector3 playerPos = new Vector3(_player.position.x, _playSpace.position.y, _player.position.z);
        StartCoroutine(MovePlaySpace(targetPos - playerPos));
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
