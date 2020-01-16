using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class CombinedMagnifier : IMagnifier
{
    private GazeMagnifier _gazeMag;

    private NaturalMagnifier _naturalMag;

    private Text _debugText;

    private float _gazeWeight = 0.5f;

    public CombinedMagnifier(Transform player, Transform magGlass, Text debugText)
    {
        _gazeMag = new GazeMagnifier(player, magGlass, debugText);
        _naturalMag = new NaturalMagnifier(player, magGlass, debugText);

        _debugText = debugText;
    }

    public float GetMagnification(RaycastHit gazePoint, Vector3 planeNormal, bool debugMode)
    {
        float naturalMag = _naturalMag.GetMagnification(gazePoint, planeNormal, false);
        float gazeMag = _gazeMag.GetMagnification(gazePoint, planeNormal, false);

        if (debugMode)
        {
            // Adjust parameters with touchpad
            ISteamVR_Action_Vector2 rightHandTouch = SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.RightHand];
            if (rightHandTouch.axis.y != 0f && rightHandTouch.lastAxis.y != 0f)
            {
                _gazeWeight += rightHandTouch.delta.y * 0.1f;
            }

            _debugText.text = "Gaze weight: " + _gazeWeight + "\nNatural weight: " + (1 - _gazeWeight);
        }

        return (gazeMag * _gazeWeight) + ((1 - _gazeWeight) * naturalMag);

    }
}
