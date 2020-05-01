using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmbassyUI : MonoBehaviour
{
    [SerializeField] GameObject prefab_EmbassyListRow;

    [SerializeField] Text Moral;
    [SerializeField] Text Budget;
    [SerializeField] Text MovementValue;
    [SerializeField] Text DevLevel;
    [SerializeField] Text Points;
    [SerializeField] Text MilBaseLoad;
    [SerializeField] Text MilBaseCost;
    [SerializeField] Image MilBaseFlag;
    [SerializeField] Button MilBaseBtn;
    [SerializeField] Text EmbassyLevel;
    [SerializeField] Text EmbassyCost;
    [SerializeField] Image EmbassyFlag;
    [SerializeField] Button EmbassyBtn;
    [SerializeField] Image Leader;
    [SerializeField] Image Focus;
    [SerializeField] Image PartyFlag;
    [SerializeField] Image RegimeIcon;
    [SerializeField] Image AuthorityIcon;
    [SerializeField] Text Label;

    [SerializeField] Transform transformCountryList;

    int curRegID = -1;
    List<EmbassyListRow> countryList;

    public void SelectCountry(int regID)
    {
        EmbassyListRow tmpRow = null;

        tmpRow = countryList.Find(e => e.RegID == curRegID);
        if (tmpRow != null)
            tmpRow.IsSelected = false;

        tmpRow = countryList.Find(e => e.RegID == regID);
        tmpRow.IsSelected = true;
        curRegID = regID;
    }

    /// <summary>
    /// Сортировка по стране.
    /// </summary>
    public void SortByName()
    {

    }

    /// <summary>
    /// Сортировка по режиму, в порядке индексов.
    /// </summary>
    public void SortByRegime()
    {

    }

    /// <summary>
    /// Сортировка по величине Prosperity в порядке убывания.
    /// </summary>
    public void SortByProsperity()
    {

    }

    /// <summary>
    /// Сортировка по очкам в порядке убывания.
    /// </summary>
    public void SortByPoints()
    {

    }

    /// <summary>
    /// Сортировка по фокусамм.
    /// </summary>
    public void SortByFocus()
    {

    }

    /// <summary>
    /// Сортировака по состоянию войны (сначала с войной).
    /// </summary>
    public void SortByWar()
    {

    }

    /// <summary>
    /// Заполнение строк с данным стран в левой панели.
    /// </summary>
    void UpdateRows()
    {
        EmbassyListRow tmpRow = null;
        nsWorld.Region_Op region = null;

        for (int i = 0; i < countryList.Count; i++)
        {
            if (transformCountryList.childCount > i)
                tmpRow = transformCountryList.GetChild(i).GetComponent<EmbassyListRow>();
            else
            {
                var go = Instantiate(prefab_EmbassyListRow, transformCountryList);
                tmpRow = go.GetComponent<EmbassyListRow>();
            }

            //region = ;

            //image
            //tmpRow.flag = ;
            //tmpRow.regimeIcon = region.RegionController.;
            //tmpRow.focusIcon = ;
            //text
            tmpRow.countryName.text = region.RegName;
            tmpRow.prosperity.text = "";
            tmpRow.points.text = "";

            tmpRow.warButton.SetActive(true);

            tmpRow.RegID = region.RegID;

            tmpRow.EmbassyUI = this;
}
    }
}
