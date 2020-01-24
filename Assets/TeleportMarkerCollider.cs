using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportMarkerCollider : MonoBehaviour
{
    public bool HasCollided { get; private set; }

    private SphereCollider _myCollider;

    private Collider _other;

    private void Awake()
    {
        _myCollider = GetComponentInChildren<SphereCollider>();
    }

    public Vector3 GetAdjustedPosition()
    {
        if (!HasCollided)
        {
            return transform.position;
        }
        Vector3 closest = _other.ClosestPointOnBounds(transform.position);
        Vector3 awayFromOther = (transform.position - closest).normalized;

        return transform.position + (awayFromOther * _myCollider.radius);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Marker collided");
        HasCollided = true;
        _other = other;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Marker stopped colliding");
        HasCollided = false;
        _other = null;
    }


}
