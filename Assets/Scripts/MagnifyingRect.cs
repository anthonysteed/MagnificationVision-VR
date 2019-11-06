using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class MagnifyingRect : MonoBehaviour
{
    [SerializeField]
    private GameObject _rectObject;

    [SerializeField]
    private float _rectHeight = 0.2f;

    [SerializeField]
    private float _cameraDistance = 8f;

    [SerializeField]
    private float _zoomStep = 5f;

    [SerializeField]
    private Camera _magnifyingCamera;

    [SerializeField]
    private GameObject _debugCanvas;

    private Camera _playerCamera;

    private Transform _playerTransform;

    private Transform _leftHand;

    private Transform _rightHand;

    private Text _debugText;

    private bool _isActive = false;

    private Vector2 _lastTouchAxis = Vector2.zero;

    private float _zoomDistance = 0f;

    private void Awake()
    {
        TrackedHand.OnHandAwake += OnHandAwake;
        TrackedHand.OnHandLost += OnHandLost;

        _playerCamera = Camera.main;
        _playerTransform = _playerCamera.transform;
        _debugText = _debugCanvas.GetComponentInChildren<Text>();
        _zoomDistance = _cameraDistance;
        _rectObject.SetActive(false);
        _debugCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        TrackedHand.OnHandAwake -= OnHandAwake;
        TrackedHand.OnHandLost -= OnHandLost;
    }

    private bool AreHandsAlive()
    {
        return _leftHand && _rightHand;
    }

    private void OnHandLost(TrackedHand.Type type)
    {
        switch(type)
        {
            case TrackedHand.Type.LEFT_HAND:
                _leftHand = null;
                break;
            case TrackedHand.Type.RIGHT_HAND:
                _rightHand = null;
                break;
        }
    }

    private void OnHandAwake(Transform transform, TrackedHand.Type hand)
    {
        switch(hand)
        {
            case TrackedHand.Type.LEFT_HAND:
                _leftHand = transform;
                break;
            case TrackedHand.Type.RIGHT_HAND:
                _rightHand = transform;
                break;
        }
    }

    private void Update()
    {
        // If button pressed, enable the rect and move the camera
        if (ControllerManager.Instance.GetButtonPressDown(ControllerButton.Trigger) && AreHandsAlive())
        {
            if (!_isActive)
            {
                Debug.Log("Enabling magnification.");

                // TODO: remove below
                _rectObject.transform.rotation = Quaternion.LookRotation(_playerTransform.forward, _playerTransform.up);
                _rectObject.SetActive(true);
                _debugCanvas.gameObject.SetActive(true);
                _isActive = true;
            }
            else
            {
                Debug.Log("Disabling magnification.");
                _rectObject.SetActive(false);
                _debugCanvas.gameObject.SetActive(false);
                _isActive = false;
            }
        }

        if (_isActive)
        {
            UpdateRectDimensions();
            UpdateCameraTransform();
            UpdateDebugText();
        }
    }

    private void UpdateRectDimensions()
    {
        Bounds leftHandBounds = _leftHand.GetComponent<Renderer>().bounds;
        Bounds rightHandBounds = _rightHand.GetComponent<Renderer>().bounds;
        float width = Vector3.Distance(leftHandBounds.center, rightHandBounds.center);

        _rectObject.transform.position = (leftHandBounds.center + rightHandBounds.center) / 2f;
        _rectObject.transform.localScale = new Vector3(width, _rectHeight, 1f);

        Vector3 normal = Vector3.Cross(rightHandBounds.center - leftHandBounds.center, leftHandBounds.max - leftHandBounds.center);
        //_rectObject.transform.rotation = transform.rotation * Quaternion.FromToRotation(_rectObject.transform.forward, normal);

        _magnifyingCamera.aspect = width / _rectHeight;
    }

    private void UpdateCameraTransform()
    {
        Vector2 currentTouchAxis = ControllerManager.Instance.GetTouchpadAxis();
        if (_lastTouchAxis.y != 0f && currentTouchAxis.y != 0f)
        {
            _zoomDistance += (currentTouchAxis.y - _lastTouchAxis.y) * _zoomStep;
        }
        if (_lastTouchAxis.x != 0f && currentTouchAxis.x != 0f)
        {
            _magnifyingCamera.fieldOfView += (currentTouchAxis.x - _lastTouchAxis.x);
        }
        _lastTouchAxis = currentTouchAxis;

        // Base zoom direction on head movement? Below is a bit nauseating
        // _magnifyingCamera.transform.position = _rectObject.transform.position + _playerTransform.forward * (_cameraDistance + _zoomAmount);

        _magnifyingCamera.transform.position = _rectObject.transform.position + _rectObject.transform.forward * _zoomDistance;
        _magnifyingCamera.transform.rotation = Quaternion.LookRotation(_playerTransform.forward);
    }

    private void UpdateDebugText()
    {
        _debugCanvas.transform.position = _rectObject.transform.position;
        _debugCanvas.transform.rotation = Quaternion.LookRotation(_playerTransform.forward, _playerTransform.up);
        _debugText.text = "Zoom distance: " + _zoomDistance + "\nFOV: " + _magnifyingCamera.fieldOfView;
    }

}

