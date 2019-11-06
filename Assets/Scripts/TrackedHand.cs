using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedHand : MonoBehaviour
{
    public delegate void HandLiveEvent(Transform transform, Type type);
    public static HandLiveEvent OnHandAwake;

    public delegate void HandDeathEvent(Type type);
    public static HandDeathEvent OnHandLost;

    public enum Type { LEFT_HAND, RIGHT_HAND }

    [SerializeField]
    private Type _type;

    private void OnEnable()
    {
        OnHandAwake(transform, _type);
    }

    private void OnDisable()
    {
        OnHandLost(_type);
    }

}
