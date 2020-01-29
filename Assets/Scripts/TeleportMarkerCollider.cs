using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportMarkerCollider : MonoBehaviour
{


    [SerializeField]
    private float _colliderRadius = 0.5f;

    [SerializeField]
    private float _colliderHeight = 1.8f;

    // Ignore floor, teleport marker and mag. rect
    private int _layerMask = ~((1 << 12) | (1 << 13) | (1 << 9));

    public bool IsObscured()
    {
        return Physics.Raycast(transform.position, transform.up, 1f, _layerMask);
    }

    public bool HasCollided()
    {
        Vector3 capsuleTop = transform.position + (transform.up * _colliderHeight);
        return Physics.CheckCapsule(transform.position, capsuleTop, _colliderRadius, _layerMask);
    }

    public Vector3 GetAdjustedPosition()
    {
        Vector3 capsuleTop = transform.position + (transform.up * _colliderHeight);
        Collider[] touchingColliders = Physics.OverlapCapsule(transform.position, capsuleTop, _colliderRadius, _layerMask);

        float closestDist = float.MaxValue;
        GameObject closestObj = null;
        Vector3 intersection = Vector3.zero;
        foreach (Collider col in touchingColliders)
        {
            Vector3 closestPt = col.ClosestPoint(transform.position);
            Vector3 toPt = (closestPt - transform.position).normalized;
            float distance = Vector3.Distance(transform.position + (toPt * _colliderRadius), closestPt);

            // Ensure closest collider is not above us
            if (distance < closestDist)
            {
                closestObj = col.gameObject;
                closestDist = distance;
                intersection = closestPt;
            }
        }
        
        // Move away from collider by same distance
        Vector3 awayFromCol = (transform.position - intersection).normalized;
        return transform.position + (awayFromCol * closestDist);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 centre = transform.position + (transform.up * _colliderHeight / 2f);
        Gizmos.DrawWireCube(centre, new Vector3(_colliderRadius * 2f, _colliderHeight, _colliderRadius * 2f));
    }

}
