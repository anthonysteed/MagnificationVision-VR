using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpAlpha : MonoBehaviour
{
    [SerializeField]
    private float _fadeTime = 1f;

    [SerializeField]
    private bool _startVisible = false;

    private Renderer _renderer;

    private int _emissionId;

    private Color _startEmission;

    private float _startAlpha;

    private bool _isFading = false;

    private bool _fadingIn;

    private float _timePassed = 0f;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        Color color = _renderer.material.color;
        _startAlpha = color.a;
        if (!_startVisible)
        {
            color.a = 0f;
            _renderer.material.color = color;
        }

        _emissionId = Shader.PropertyToID("_EmissionColor");
        _startEmission = _renderer.material.GetColor(_emissionId);
    }

    private void Update()
    {
        if (!_isFading)
        {
            return;
        }

        float t;
        if (_fadingIn)
        {
            t = _timePassed / _fadeTime;
        }
        else
        {
            t = 1 - (_timePassed / _fadeTime);
        }

        Color c = _renderer.material.color;
        c.a = Mathf.Lerp(0f, _startAlpha, t);
        _renderer.material.color = c;

        _timePassed += Time.deltaTime;
        if (_timePassed > _fadeTime)
        {
            _timePassed = 0f;
            _isFading = false;
        }
    }

    public void FadeWithEmission()
    {
        StartCoroutine(EmissionEffect());
    }

    private IEnumerator EmissionEffect()
    {
        float emissionTime = 0f;
        while (emissionTime <= 1f)
        {
            emissionTime += Time.deltaTime;
            _renderer.material.SetColor(_emissionId, Color.Lerp(_startEmission, Color.white, emissionTime));
            _renderer.material.EnableKeyword("_EMISSION");
            yield return null;
        }
        Fade(false);
    }

    public void Fade(bool fadeIn)
    {
        if (_isFading)
        {
            _timePassed = _fadeTime - _timePassed;
        }
        _fadingIn = fadeIn;
        _isFading = true;
    }


}
