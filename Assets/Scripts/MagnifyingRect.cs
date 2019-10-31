using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnifyingRect : MonoBehaviour
{
    [SerializeField]
    private GameObject _rectObject;

    [SerializeField]
    private float _rectHeight = 0.2f;

    [SerializeField]
    private float _cameraDistance = 8f;

    [SerializeField]
    private float _zoomStep = 1f;

    [SerializeField]
    private Camera _magnifyingCamera;

    private Transform _playerTransform;

    private Transform[] _hands = new Transform[2];

    private bool _areHandsActive = false;

    private bool _isActive = false;

    private float _lastTouchpadY = 0f;

    private float _zoomAmount = 0f;

    private void Awake()
    {
        _playerTransform = Camera.main.transform;
        _rectObject.SetActive(false);
    }

    private void Start()
    {
        FindHands();
    }

    private void FindHands()
    {
        int i = 0;
        foreach (Transform child in this.transform)
        {
            if (child.CompareTag("Hand"))
            {
                _hands[i++] = child;
            }
        }
        _areHandsActive = i == 2;
        Debug.Log("Found " + i + " hands.");
    }

    private void Update()
    {
        Debug.Log("Touchpad axis: " + ControllerManager.Instance.GetTouchpadAxis());

        if (ControllerManager.Instance.GetButtonPressDown(ControllerButton.Menu))
        {
            FindHands();
        }
        // If button pressed, enable the rect and move the camera
        else if (ControllerManager.Instance.GetButtonPressDown(ControllerButton.Trigger) && _areHandsActive)
        {
            if (_isActive)
            {
                Debug.Log("Disabling magnification.");
                _rectObject.SetActive(false);
                _isActive = false;
            }
            else
            {
                if (!_areHandsActive)
                {
                    FindHands();
                    return;
                }
                Debug.Log("Enabling magnification.");

                // TODO: remove below
                _rectObject.transform.rotation = Quaternion.LookRotation(_playerTransform.forward, _playerTransform.up);
                _rectObject.SetActive(true);
                _isActive = true;
            }
        }

        if (_isActive)
        {
            UpdateRectDimensions();
            UpdateCameraTransform();
        }
    }

    private void UpdateRectDimensions()
    {
        Bounds leftHandBounds = _hands[0].GetComponent<Renderer>().bounds;
        Bounds rightHandBounds = _hands[1].GetComponent<Renderer>().bounds;
        float width = Vector3.Distance(leftHandBounds.center, rightHandBounds.center);

        _rectObject.transform.position = (leftHandBounds.center + rightHandBounds.center) / 2f;
        _rectObject.transform.localScale = new Vector3(width, _rectHeight, 1f);

        Vector3 normal = Vector3.Cross(rightHandBounds.center - leftHandBounds.center, leftHandBounds.max - leftHandBounds.center);
        //_rectObject.transform.rotation = transform.rotation * Quaternion.FromToRotation(_rectObject.transform.forward, normal);

        _magnifyingCamera.aspect = width / _rectHeight;

        // SetRenderTextureAspect(Mathf.RoundToInt(width), Mathf.RoundToInt(_rectHeight));
    }

    private void SetRenderTextureAspect(int width, int height)
    {
        RenderTexture renderTexture = new RenderTexture(width, height, 0);
        _magnifyingCamera.targetTexture = renderTexture;
    }

    private void UpdateCameraTransform()
    {
        // Each 0.1 unit difference in touchpad y is one zoom step
        float currentTouchPadY = ControllerManager.Instance.GetTouchpadAxis().y;
        if (_lastTouchpadY != 0f && currentTouchPadY != 0f)
        {
            _zoomAmount += (currentTouchPadY - _lastTouchpadY) * 10f * _zoomStep;
        }
        _lastTouchpadY = currentTouchPadY;

        _magnifyingCamera.transform.position = _rectObject.transform.position + _rectObject.transform.forward * (_cameraDistance + _zoomAmount);
        _magnifyingCamera.transform.rotation = Quaternion.LookRotation(_playerTransform.forward);
    }

}

