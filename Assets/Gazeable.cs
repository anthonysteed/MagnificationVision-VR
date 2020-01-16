using System.Collections;
using System.Collections.Generic;
using Tobii.G2OM;
using UnityEngine;

public class Gazeable : MonoBehaviour, IGazeFocusable
{
    public bool HasFocus { get; private set; } = true;

    public void GazeFocusChanged(bool hasFocus)
    {
        Debug.Log("Gaze focus changed: " + hasFocus);
        HasFocus = hasFocus;
    }
}
