using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerLogger : MonoBehaviour
{
    private BufferedLogger _log = new BufferedLogger("Controller");

    private Transform _leftHand;

    private Transform _rightHand;

    private void Update()
    {
        if (_leftHand != null)
        {
            _log.Append("LAlive", true);
            _log.Append("LPos", _leftHand.position);
            _log.Append("LRot", _leftHand.rotation.eulerAngles);
            LogButtonPresses('L', SteamVR_Input_Sources.LeftHand);
        }
        if (_rightHand != null)
        {
            _log.Append("RAlive", true);
            _log.Append("RPos", _rightHand.position);
            _log.Append("RRot", _rightHand.rotation.eulerAngles);
            LogButtonPresses('R', SteamVR_Input_Sources.RightHand);
        }

        _log.CommitLine();
    }

    private void LogButtonPresses(char prefix, SteamVR_Input_Sources hand)
    {
        _log.Append(prefix + "Trigger", SteamVR_Actions.default_GrabPinch[hand].state);
        _log.Append(prefix + "TouchPad", SteamVR_Actions.default_TouchPad[hand].axis);
        _log.Append(prefix + "Teleport", SteamVR_Actions.default_Teleport.state);
    }

    public void OnHandConnectionChange(SteamVR_Behaviour_Pose pose, SteamVR_Input_Sources changedSource, bool isConnected)
    {
        if (changedSource == SteamVR_Input_Sources.LeftHand)
        {
            _leftHand = isConnected ? pose.transform : null;
        }
        else if (changedSource == SteamVR_Input_Sources.RightHand)
        {
            _rightHand = isConnected ? pose.transform : null;
        }
    }
}
