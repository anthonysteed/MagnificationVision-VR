using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnifyingRect : MonoBehaviour
{
    [SerializeField]
    private GameObject _rectObject;

    [SerializeField]
    private float _cameraDistance = 12f;

    [SerializeField]
    private Camera _magnifyingCamera;

    private Transform _playerTransform;

    private Transform[] _hands = new Transform[2];

    private bool _areHandsActive = false;


    private bool _isActive = false;

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
                Debug.Log("Enabling magnification.");
                if (!_areHandsActive)
                {
                    FindHands();
                    return;
                }

                _rectObject.transform.position = (_hands[0].position + _hands[1].position) / 2f;
                _magnifyingCamera.transform.position = _rectObject.transform.position + _rectObject.transform.forward * _cameraDistance;
                _rectObject.SetActive(true);
                _isActive = true;
            }
        }

        if (_isActive)
        {
            UpdateRotation();
        }

    }

    private void UpdateRotation()
    {
        _magnifyingCamera.transform.rotation = Quaternion.LookRotation(_playerTransform.forward);
    }

}

