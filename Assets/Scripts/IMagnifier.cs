using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMagnifier
{
    float GetMagnification(Vector3 planeNormal, bool debugMode);
}
