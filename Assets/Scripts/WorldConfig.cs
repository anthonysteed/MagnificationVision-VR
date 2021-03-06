﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldConfig : MonoBehaviour
{
    public Permutation WorldConfiguration { get { return _permutation; } }

    public static WorldConfig Instance { get; private set; }

    // Item placements
    public enum Permutation { PERM_1, PERM_2, PERM_3 }

    [SerializeField]
    private Permutation _permutation;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one WorldConfig!!!");
        }
        Instance = this;
    }

    public void SetPosition(Transform item, HiddenItem.Type type)
    {
        switch (_permutation)
        {
            case Permutation.PERM_1:
                switch (type)
                {
                    case HiddenItem.Type.BROWN_CHEESE:
                        item.position = new Vector3(1.767f, 0.4242516f, -21.158f);
                        item.rotation = Quaternion.Euler(2.316f, 536.898f, 0f);
                        break;
                    case HiddenItem.Type.BROKEN_BOTTLE:
                        item.position = new Vector3(3.935f, 0.807f, -9.871f);
                        item.rotation = Quaternion.Euler(-3.476f, -20.452f, 1.295f);
                        break;
                    case HiddenItem.Type.HEADLESS_FISH:
                        item.position = new Vector3(7.363f, 0.809f, -40.293f);
                        item.rotation = Quaternion.Euler(0f, -82.833f, 0f);
                        break;
                    case HiddenItem.Type.MARGARINE:
                        item.position = new Vector3(3.2713f, 0.459f, -35.316f);
                        item.rotation = Quaternion.Euler(0f, 0f, 0f);
                        break;
                    case HiddenItem.Type.PINEAPPLE_PIZZA:
                        item.position = new Vector3(-0.498f, 0.538f, -27.656f);
                        item.rotation = Quaternion.Euler(0f, 110.59f, 0f);
                        break;
                }
                break;
            case Permutation.PERM_2:
                switch (type)
                {
                    case HiddenItem.Type.PINEAPPLE_PIZZA:
                        item.position = new Vector3(-8.399718f, 0.54f, -44.4046f);
                        item.rotation = Quaternion.Euler(0f, 266.413f, 0f);
                        break;
                    case HiddenItem.Type.HEADLESS_FISH:
                        item.position = new Vector3(6.901f, 0.409f, -36.793f);
                        item.rotation = Quaternion.Euler(0f, -0.108f, 0f);
                        break;
                    case HiddenItem.Type.BROKEN_BOTTLE:
                        item.position = new Vector3(-5.976f, 1.19f, -26.372f);
                        item.rotation = Quaternion.Euler(23.433f, -220.234f, -16.476f);
                        break;
                    case HiddenItem.Type.BROWN_CHEESE:
                        item.position = new Vector3(7.058f, 0.797f, -15.881f);
                        item.rotation = Quaternion.Euler(0f, 543.318f, 0f);
                        break;
                    case HiddenItem.Type.MARGARINE:
                        item.position = new Vector3(1.587f, 0.275f, -37.903f);
                        item.rotation = Quaternion.Euler(0f, 184.748f, 0f);
                        break;
                }
                break;
            case Permutation.PERM_3:
                switch (type)
                {
                    case HiddenItem.Type.PINEAPPLE_PIZZA:
                        item.position = new Vector3(-4.396f, 1.074f, -8.886f);
                        item.rotation = Quaternion.Euler(-180f, 308.005f, 180f);
                        break;
                    case HiddenItem.Type.BROWN_CHEESE:
                        item.position = new Vector3(6.732431f, 0.4201571f, -32.80882f);
                        item.rotation = Quaternion.Euler(-1.83f, 543.318f, 0f);
                        break;
                    case HiddenItem.Type.HEADLESS_FISH:
                        item.position = new Vector3(8.204f, 0.409f, -16.535f);
                        item.rotation = Quaternion.Euler(0f, 180f, 0f);
                        break;
                    case HiddenItem.Type.MARGARINE:
                        item.position = new Vector3(1.618f, 0.282f, -19.763f);
                        item.rotation = Quaternion.Euler(0f, 184.748f, 0f);
                        break;
                    case HiddenItem.Type.BROKEN_BOTTLE:
                        item.position = new Vector3(-1.489f, 0.157f, -31.217f);
                        item.rotation = Quaternion.Euler(30.855f, -84.075f, 15.287f);
                        break;
                }
                break;
        }

    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        HiddenItem[] items = FindObjectsOfType<HiddenItem>();
        foreach (HiddenItem item in items)
        {
            SetPosition(item.transform, item.ItemType);
        }

    }


}
