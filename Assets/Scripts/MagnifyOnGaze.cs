using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.G2OM;

public class MagnifyOnGaze : MonoBehaviour, IGazeFocusable
{
    public delegate void GazeEvent(bool hasFocus);
    public static event GazeEvent OnGaze;

    public void GazeFocusChanged(bool hasFocus)
    {
        OnGaze(hasFocus);
    }
}
