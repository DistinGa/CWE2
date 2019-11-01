using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    Text _text;
    [SerializeField] string Key;

    public string LocalizationKey
    {
        get { return Key; }

        set
        {
            Key = value;
            UpdateText();
        }
    }

    void Awake()
    {
        _text = GetComponent<Text>();
        UpdateText();
    }

    void UpdateText()
    {
        _text.text = LocalizationManager.Localize(LocalizationKey);
    }
}
