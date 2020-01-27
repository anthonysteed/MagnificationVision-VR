using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class Checklist : MonoBehaviour
{
    public bool IsVisible { get; private set; }

    private Canvas _canvas;

    private Renderer _renderer;

    private Text[] _texts;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _canvas = GetComponentInChildren<Canvas>();
        _texts = GetComponentsInChildren<Text>();
        SetVisible(false);
    }

    private void SetVisible(bool state)
    {
        IsVisible = state;
        _renderer.enabled = state;
        _canvas.enabled = state;

        if (state)
        {
            foreach (Text text in _texts)
            {
                text.fontSize = 20;
            }
        }

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
