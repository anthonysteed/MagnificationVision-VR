using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpAlpha : MonoBehaviour
{
    private Renderer _renderer;

    private float _startAlpha;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _startAlpha = _renderer.material.color.a;
    }

    public void SetT(float t)
    {
        Color c = _renderer.material.color;
        c.a = Mathf.Lerp(0f, _startAlpha, t);
        _renderer.material.color = c;
    }


}
