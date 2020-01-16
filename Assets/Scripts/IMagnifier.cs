using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMagnifier
{
    float GetMagnification(RaycastHit gazePoint, Vector3 planeNormal, bool debugMode);
}
