using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBounds : MonoBehaviour
{
    [SerializeField]
    private bool _renderer;

    [SerializeField]
    private bool _mesh;

    [SerializeField]
    private bool _collider;


    private void OnDrawGizmos()
    {
        if (_renderer)
        {
            Gizmos.color = Color.cyan;
            Bounds bounds = GetComponent<Renderer>().bounds;

            Gizmos.DrawWireCube(bounds.center, bounds.size); 
        }
        if (_mesh)
        {
            Gizmos.color = Color.magenta;
            Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        if (_collider)
        {
            Gizmos.color = Color.yellow;
            Bounds bounds = GetComponent<Collider>().bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

}
