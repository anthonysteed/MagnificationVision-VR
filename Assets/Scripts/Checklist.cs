using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Checklist : MonoBehaviour
{
    public bool IsVisible { get; private set; }

    private Canvas _canvas;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _canvas = GetComponentInChildren<Canvas>();
        SetVisible(false);
    }

    private void SetVisible(bool state)
    {
        IsVisible = state;
        _renderer.enabled = state;
        _canvas.enabled = state;
    }

    private void Update()
    {
        SteamVR_Action_Boolean_Source triggerState = SteamVR_Actions.default_GrabPinch[SteamVR_Input_Sources.LeftHand];
        if (triggerState.stateDown)
        {
            SetVisible(true);
        }
        else if (triggerState.stateUp)
        {
            SetVisible(false);
        }

    }
}
