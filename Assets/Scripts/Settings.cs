using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using WPM;

[CreateAssetMenu(fileName = "Settings", menuName = "ScriptableObjects/Settings")]
public class Settings :SerializedScriptableObject
{
    [Button("Load countries")]
    private void FillCountriesFromMap()
    {
        WorldMapGlobe _map = WorldMapGlobe.instance;

        ModProperties.Regions.Clear();
        ModProperties.Regions_Originals.Clear();

        for (int i = 0; i < _map.countries.Length; i++)
        {
            ModProperties.Regions[i] = new Region_Prop() { RegID = i, RegName = _map.countries[i].name };
            ModProperties.Regions_Originals[i] = new nsWorld.Region_Ds();
        }
    }

    [OdinSerialize, System.NonSerialized]
    public ModEditor.ModProperties ModProperties;

}
