using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportMarkerCollider : MonoBehaviour
{
    [SerializeField]
    private float _colliderRadius = 0.5f;

    [SerializeField]
    private float _colliderHeight = 1.8f;

    // Ignore floor, mag. rect and teleport marker
    private int _layerMask = ~((1 << 12) | (1 << 13) | (1 << 9));

    public bool HasCollided()
    {
        Vector3 capsuleTop = transform.position + (transform.up * _colliderHeight);
        return Physics.CheckCapsule(transform.position, capsuleTop, _colliderRadius, _layerMask);
    }

    public Vector3 GetAdjustedPosition()
    {
        //RaycastHit hit;
        //if (Physics.SphereCast(transform.position, _colliderRadius, Vector3.zero, out hit, 1f, _layerMask))
        //{
        //    Debug.Log("detected sphere intersection with " + hit.collider.gameObject);
        //    Vector3 toIntersection = transform.position - hit.point;
        //    return transform.position + toIntersection;
        //}
        //return transform.position;


        Vector3 capsuleTop = transform.position + (transform.up * _colliderHeight);
        Collider[] touchingColliders = Physics.OverlapCapsule(transform.position, capsuleTop, _colliderRadius, _layerMask);
        if (touchingColliders.Length <= 0)
        {
            return transform.position;
        }
        float closestDist = float.MaxValue;
        GameObject closestObj = null;
        Vector3 intersection = Vector3.zero;
        foreach (Collider col in touchingColliders)
        {
            Vector3 closestPt = col.ClosestPoint(transform.position);
            Vector3 toPt = (closestPt - transform.position).normalized;
            float distance = Vector3.Distance(transform.position + (toPt * _colliderRadius), closestPt);
            if (distance < closestDist)
            {
                closestObj = col.gameObject;
                closestDist = distance;
                intersection = closestPt;
            }
        }
        Debug.Log("detected sphere intersection with " + closestObj);
        Debug.Log("shifted teleport position by " + closestDist);
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
