using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenItem : MonoBehaviour
{
    public delegate void ItemEvent(Type type);
    public static event ItemEvent OnItemFound;

    public enum Type { BROWN_CHEESE, HEADLESS_FISH, BROKEN_BOTTLE, PINEAPPLE_PIZZA, MARGARINE }

    [SerializeField]
    private Type _type;

    private LerpAlpha _lerpAlpha;

    private ParticleSystem _particles;

    private bool _hasBeenDiscovered = false;

    private void Awake()
    {
        _lerpAlpha = GetComponent<LerpAlpha>();
        _particles = GetComponentInChildren<ParticleSystem>();
    }

    public void Discover()
    {
        if (_hasBeenDiscovered)
        {
            return;
        }
        // TODO: Sound effect
        _hasBeenDiscovered = true;
        OnItemFound(_type);
        StartCoroutine(FadeEffect());
    }

    private IEnumerator FadeEffect()
    {
        _lerpAlpha.FadeWithEmission();
        yield return null;
        Destroy(gameObject, 4f);
        _particles?.Play();
    }


}
