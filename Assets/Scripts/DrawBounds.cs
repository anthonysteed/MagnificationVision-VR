using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBounds : MonoBehaviour
{

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Bounds bounds = GetComponent<Renderer>().bounds;

        Gizmos.DrawWireCube(bounds.center, bounds.size); 
    }

}
