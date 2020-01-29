using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportMarkerCollider : MonoBehaviour
{
    // Ignore floor, teleport marker and mag. rect
    private int _layerMask = ~((1 << 12) | (1 << 13) | (1 << 9));

    private CapsuleCollider _myCollider;

    private void Awake()
    {
        _myCollider = GetComponent<CapsuleCollider>();
    }

    public bool HasCollided()
    {
        Vector3 capsuleTop = transform.position + (transform.up * _myCollider.height);
        return Physics.CheckCapsule(transform.position, capsuleTop, _myCollider.radius, _layerMask);
    }

    public Vector3 GetAdjustedPosition()
    {
        Vector3 capsuleTop = transform.position + (transform.up * _myCollider.height);

        Collider[] touchingColliders = Physics.OverlapCapsule(transform.position, capsuleTop, _myCollider.radius, _layerMask);

        float largestDist = 0f;
        Vector3 shiftDir = Vector3.zero;
        foreach (Collider col in touchingColliders)
        {
            if (!Physics.ComputePenetration(_myCollider, transform.position, transform.rotation,
                col, col.transform.position, col.transform.rotation, out Vector3 awayDir, out float distance))
            {
                continue;
            }
            // Don't want to shift downwards
            if (distance > largestDist && Vector3.Angle(-transform.up, awayDir) > 45f)
            {
                largestDist = distance;
                shiftDir = awayDir;
            }

        }

        largestDist += _myCollider.radius;
        return transform.position + (largestDist * shiftDir);
    }

}
