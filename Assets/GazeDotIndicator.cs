using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GazeDotIndicator : MonoBehaviour
{
    [SerializeField]
    private float _minVisibleDistance = 0.5f;

    [SerializeField]
    private float _maxFadeDistance = 3f;

    private MagnificationManager _magManager;

    private GazeMagnifier _gazeMag;

    private float _maxAlpha;

    private float _currentAlpha;

    private Image _dotImage;

    private void Awake()
    {
        _magManager = FindObjectOfType<MagnificationManager>();
        _gazeMag = FindObjectOfType<GazeMagnifier>();
        _dotImage = GetComponentInChildren<Image>();

        _maxAlpha = _dotImage.color.a;
        _currentAlpha = _maxAlpha;
    }


    private void Update()
    {
        if (!_magManager.IsMagnifying)
        {
            return;
        }

        // Set alpha based on distance from player
        float dist = Mathf.Clamp(_gazeMag.LastGazeDistance, _minVisibleDistance, _maxFadeDistance) - _minVisibleDistance;
        float max = _maxFadeDistance - _minVisibleDistance;

        _currentAlpha = Mathf.Lerp(0f, _maxAlpha, dist / max);

        Color c = _dotImage.color;
        c.a = _currentAlpha;
        _dotImage.color = c;
    }

    public void SetValid(bool isValid)
    {
        Color color = isValid ? Color.green : Color.red;
        color.a = _currentAlpha;
        _dotImage.color = color;
    }

    public void SetProgress(float t)
    {
        _dotImage.fillAmount = t;
    }



}
