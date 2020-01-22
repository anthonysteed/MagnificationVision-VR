using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class CombinedMagnifier : MonoBehaviour, IMagnifier
{
    // How much weight [0, 1] to give gaze magnification (rest provided by natural-mag)
    [SerializeField]
    private float _gazeWeight = 0.5f;

    private GazeMagnifier _gazeMag;

    private NaturalMagnifier _naturalMag;

    private void Awake()
    {
        _gazeMag = GetComponent<GazeMagnifier>();
        _naturalMag = GetComponent<NaturalMagnifier>();
    }

    public float GetMagnification(RaycastHit gazePoint, Vector3 planeNormal)
    {
        float naturalMag = _naturalMag.GetMagnification(gazePoint, planeNormal);
        float gazeMag = _gazeMag.GetMagnification(gazePoint, planeNormal);

        return (gazeMag * _gazeWeight) + ((1 - _gazeWeight) * naturalMag);

    }
}
