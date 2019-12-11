using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class NaturalMagnifier : IMagnifier
{
    private float _imageDistance = 1.4f;

    private float _focalLength = 0.23f;

    private Transform _player;

    private Transform _magGlass;

    private Text _debugText;

    public NaturalMagnifier(Transform player, Transform magGlass, Text debugText)
    {
        _player = player;
        _magGlass = magGlass;
        _debugText = debugText;
        _debugText.alignment = TextAnchor.UpperLeft;
    }

    public float GetMagnification(Vector3 planeNormal, bool debugMode)
    {
        if (debugMode)
        {
            // Adjust parameters with touchpad
            ISteamVR_Action_Vector2 rightHandTouch = SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.RightHand];
            if (rightHandTouch.axis.y != 0f && rightHandTouch.lastAxis.y != 0f)
            {
                _imageDistance += rightHandTouch.delta.y;
            }
            ISteamVR_Action_Vector2 leftHandTouch = SteamVR_Actions.default_TouchPad[SteamVR_Input_Sources.LeftHand];
            if (leftHandTouch.axis.y != 0f && leftHandTouch.lastAxis.y != 0f)
            {
                _focalLength += leftHandTouch.delta.y;
            }
        }

        float eyeDistance = Vector3.Distance(_player.position, _magGlass.transform.position);
        float realImageDistance = Mathf.Abs(eyeDistance - _imageDistance);

        float magnification = (0.25f / eyeDistance) * (1 + ((realImageDistance - eyeDistance) / _focalLength));

        if (debugMode)
        {
            if (magnification < 1f)
            {
                _debugText.color = Color.red;
            }
            else
            {
                _debugText.color = Color.green;
            }
            _debugText.text = "Image distance: " + _imageDistance + "\nFocal length: " + _focalLength + "\nEye distance: " + eyeDistance + "\nMagnification: " + magnification;
        }

        return magnification;
    }
}
