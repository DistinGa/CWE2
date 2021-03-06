﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmbassyListRow : MonoBehaviour
{
    public Image flag, regimeIcon, focusIcon;
    public Text countryName, prosperity, points;
    public GameObject warButton, SelectBack;

    public int RegID;

    public EmbassyUI EmbassyUI;

    public bool IsSelected
    {
        get { return SelectBack.activeSelf; }
        set { SelectBack.SetActive(value); }
    }

    public void SelectCountry()
    {
        EmbassyUI.SelectCountry(RegID);
    }

}
