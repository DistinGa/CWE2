using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Неизменяемые свойства региона в настройках мода (ModEditor.ModProperties)
/// </summary>
[System.Serializable]
public class Region_Prop
{
    public int RegID;  //индекс в ассете Political Map
    public string RegName;  //индекс в системе локализации (совпадает с названием страны из ассета)
    public int SeaPoolID;
    public Sprite Flag;
    public Sprite MetaRegion;
}
