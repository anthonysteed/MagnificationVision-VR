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

    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public void Reveal()
    {
        // TODO
        _collider.enabled = false;
        OnItemFound(_type);

    }


}
