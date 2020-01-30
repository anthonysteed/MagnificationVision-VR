using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListItem : MonoBehaviour
{
    [SerializeField]
    private HiddenItem.Type _type;

    public bool Found { get; private set; }

    private Image _checkmark;

    private Text _itemText;

    private void OnEnable()
    {
        HiddenItem.OnItemFound += OnItemFound;
    }

    private void Awake()
    {
        _itemText = GetComponentInChildren<Text>();
        _checkmark = GetComponentInChildren<Image>();
    }

    private void OnItemFound(HiddenItem.Type type)
    {
        if (type == _type && !Found)
        {
            // TODO
            Found = true;
            _itemText.color = Color.green;
            _itemText.FontTextureChanged();
            Debug.Log(type + " was found");
            _checkmark.enabled = true;
        }
    }


    private void OnDisable()
    {
        HiddenItem.OnItemFound -= OnItemFound;
    }


}
