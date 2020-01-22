using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class NaturalMagnifier : MonoBehaviour, IMagnifier
{
    // Virtual image distance of mag. glass
    [SerializeField]
    private float _imageDistance = 1.4f;

    [SerializeField]
    private float _focalLength = 0.23f;

    private Transform _player;

    private Transform _magRect;

    private void Awake()
    {
        _player = Camera.main.transform;
        _magRect = GameObject.FindGameObjectWithTag("MagRect").transform;
    }


    public float GetMagnification(RaycastHit gazePoint, Vector3 planeNormal)
    {
        float eyeDistance = Vector3.Distance(_player.position, _magRect.transform.position);
        float realImageDistance = Mathf.Abs(eyeDistance - _imageDistance);

        float magnification = (0.25f / eyeDistance) * (1 + ((realImageDistance - eyeDistance) / _focalLength));

        return magnification;
    }
}
