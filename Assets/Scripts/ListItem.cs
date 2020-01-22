using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListItem : MonoBehaviour
{
    [SerializeField]
    private HiddenItem.Type _type;

    public bool Found { get; private set; }

    private Image _icon;

    private Image _checkmark;

    private void OnEnable()
    {
        HiddenItem.OnItemFound += OnItemFound;
    }

    private void Awake()
    {
            
    }

    private void OnItemFound(HiddenItem.Type type)
    {
        if (type == _type && !Found)
        {
            // TODO
            Found = true;
        }
    }


    private void OnDisable()
    {
        HiddenItem.OnItemFound -= OnItemFound;
    }


}
