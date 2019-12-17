using System.Collections;
using System.Collections.Generic;
using Tobii.G2OM;
using UnityEngine;

public class Gazeable : MonoBehaviour, IGazeFocusable
{
    public bool HasFocus { get; private set; }

    public void GazeFocusChanged(bool hasFocus)
    {
        HasFocus = hasFocus;
    }
}
