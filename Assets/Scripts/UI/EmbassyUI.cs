using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using nsEventSystem;

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

    [SerializeField] Image FlagTop;
    [SerializeField] Image AuthorityIconTop;
    [SerializeField] Image PartyFlagTop;
    [SerializeField] Image MetaRegion;
    [SerializeField] Text CountryNameTop;

    [SerializeField] Transform parentCountryList;

    int curRegID = -1;
    nsWorld.Region_Op region;
    List<EmbassyListRow> countryList;
    private bool _prestigeMilBase;
    private bool _natFundMilBase;
    private bool _prestigeEmbassy;
    private bool _natFundEmbassy;

    public bool prestigeMilBase
    {
        get
        {
            return _prestigeMilBase;
        }
        set
        {
            _prestigeMilBase = value;
            UpdateMilBase();
        }
    }

    public bool natFundMilBase
    {
        get
        {
            return _natFundMilBase;
        }
        set
        {
            _natFundMilBase = value;
            UpdateMilBase();
        }
    }

    public bool prestigeEmbassy
    {
        get
        {
            return _prestigeEmbassy;
        }
        set
        {
            _prestigeEmbassy = value;
            UpdateEmbassy();
        }
    }

    public bool natFundEmbassy
    {
        get
        {
            return _natFundEmbassy;
        }
        set
        {
            _natFundEmbassy = value;
            UpdateEmbassy();
        }
    }

    private void Start()
    {
        countryList = new List<EmbassyListRow>();
        InitialFillRows();
        SelectCountry(countryList[0].RegID);
    }

    public void SelectCountry(int regID)
    {
        EmbassyListRow tmpRow = null;

        tmpRow = countryList.Find(e => e.RegID == curRegID);
        if (tmpRow != null)
            tmpRow.IsSelected = false;

        tmpRow = countryList.Find(e => e.RegID == regID);
        tmpRow.IsSelected = true;
        curRegID = regID;
        region = nsWorld.World.TheWorld.Regions[curRegID];

        //Заполнение данных в правой панели.
        Moral.text = region.Moral.ToString();
        Budget.text = region.RegionController == null ? region.GNP.ToString() : region.RegionController.NatFund.ToString();
        MovementValue.text = region.MovementValue.ToString();
        DevLevel.text = region.ProsperityLevel.ToString();
        Points.text = region.Score.ToString();

        UpdateMilBase();
        UpdateEmbassy();

        //Label.text = ;
        //Leader.sprite = ;
        //Focus.sprite = ;
        //PartyFlag.sprite = ;

        //Top
        FlagTop.sprite = GameManager.GM.GameProperties.Regions[regID].Flag;
        AuthorityIconTop.sprite = GameManager.GM.GameProperties.AuthorityIcons[region.Authority];
        CountryNameTop.text = region.RegName;
        MetaRegion.sprite = GameManager.GM.GameProperties.Regions[regID].MetaRegion;
        //PartyFlagTop.sprite = ;
    }

    /// <summary>
    /// Обновление контрола военной базы
    /// </summary>
    /// <param name="region"></param>
    void UpdateMilBase()
    {
        var _milBase = nsMilitary.MilitaryManager.Instance.GetMilitaryBase(region.MilitaryBaseID);
        bool _MilBaseInteractable = false;
        if (_milBase != null)
        {
            MilBaseLoad.text = _milBase.FreeCapacity.ToString() + "/" + _milBase.Capacity.ToString();
            MilBaseCost.text = _milBase.UpgradeCost.ToString();
            MilBaseFlag.sprite = GameManager.GM.GameProperties.Regions[nsWorld.World.TheWorld.GetRegionController(_milBase.AuthID).HomelandID].Flag;
            if (_milBase.AuthID == GameManager.GM.PlayerAuthority)
            {
                if ((_prestigeMilBase && nsWorld.World.TheWorld.GetRegionController(GameManager.GM.PlayerAuthority).Prestige >= _milBase.UpgradeCost) || (_natFundMilBase && nsWorld.World.TheWorld.GetRegionController(GameManager.GM.PlayerAuthority).NatFund >= _milBase.UpgradeCost))
                    _MilBaseInteractable = true;
            }
        }
        else
        {
            //Нет базы, можно построить свою.
            MilBaseLoad.text = "";
            MilBaseCost.text = GameManager.GM.GameProperties.InitMilBaseCost.ToString();
            MilBaseFlag.sprite = GameManager.GM.GameProperties.Regions[region.RegID].Flag;
            if (region.GetInfluence(GameManager.GM.PlayerAuthority) >= GameManager.GM.GameProperties.SelfInflToBuildBase)    //Если своё влияние больше требуемого для постройки
            {
                if ((_prestigeMilBase && nsWorld.World.TheWorld.GetRegionController(GameManager.GM.PlayerAuthority).Prestige >= GameManager.GM.GameProperties.InitMilBaseCost) || (_natFundMilBase && nsWorld.World.TheWorld.GetRegionController(GameManager.GM.PlayerAuthority).NatFund >= GameManager.GM.GameProperties.InitMilBaseCost))
                    _MilBaseInteractable = true;
            }
        }
        MilBaseBtn.interactable = _MilBaseInteractable;
    }

    /// <summary>
    /// Обновление контрола посольства
    /// </summary>
    /// <param name="region"></param>
    void UpdateEmbassy()
    {
        var _embassy = nsWorld.World.TheWorld.Embassies[region.RegID][GameManager.GM.PlayerAuthority];

        EmbassyLevel.text = _embassy.EmbassyLevel.ToString();
        EmbassyCost.text = _embassy.EmbassyUpgradeCost.ToString();

        EmbassyFlag.sprite = GameManager.GM.GameProperties.Regions[region.RegID].Flag;
        RegimeIcon.sprite = GameManager.GM.GameProperties.AuthorityIcons[region.Authority];
        AuthorityIcon.sprite = GameManager.GM.GameProperties.AuthorityIcons[region.Authority];

        bool _EmbassyInteractable = false;
        if ((_prestigeEmbassy && nsWorld.World.TheWorld.GetRegionController(GameManager.GM.PlayerAuthority).Prestige >= _embassy.EmbassyUpgradeCost) || (_natFundEmbassy && nsWorld.World.TheWorld.GetRegionController(GameManager.GM.PlayerAuthority).NatFund >= _embassy.EmbassyUpgradeCost))
            _EmbassyInteractable = true;

        EmbassyBtn.interactable = _EmbassyInteractable;
    }

    /// <summary>
    /// Сортировка дочерних строк списка стран.
    /// </summary>
    /// <param name="list"></param>
    private void rowsSort(List<EmbassyListRow> list)
    {
        int i = 0;
        foreach (var item in list)
        {
            item.transform.SetSiblingIndex(i);
            i++;
        }
    }

    /// <summary>
    /// Сортировка по стране.
    /// </summary>
    public void SortByName()
    {
        var _regions = nsWorld.World.TheWorld.Regions;
        rowsSort(countryList.OrderBy(r => _regions[r.RegID].RegName).ToList());
    }

    /// <summary>
    /// Сортировка по режиму, в порядке индексов.
    /// </summary>
    public void SortByRegime()
    {
        var _regions = nsWorld.World.TheWorld.Regions;
        rowsSort(countryList.OrderBy(r => _regions[r.RegID].Authority).ToList());
    }

    /// <summary>
    /// Сортировка по величине Prosperity в порядке убывания.
    /// </summary>
    public void SortByProsperity()
    {
        var _regions = nsWorld.World.TheWorld.Regions;
        rowsSort(countryList.OrderBy(r => _regions[r.RegID].ProsperityLevel).ToList());
    }

    /// <summary>
    /// Сортировка по очкам в порядке убывания.
    /// </summary>
    public void SortByPoints()
    {
        var _regions = nsWorld.World.TheWorld.Regions;
        rowsSort(countryList.OrderBy(r => _regions[r.RegID].Score).ToList());
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
    /// Заполнение строк с данными стран в левой панели.
    /// </summary>
    void InitialFillRows()
    {
        EmbassyListRow tmpRow = null;

        var childCount = parentCountryList.childCount;
        int i = 0;
        foreach (var reg in nsWorld.World.TheWorld.Regions.Values)
        {
            if (childCount > i)
                tmpRow = parentCountryList.GetChild(i).GetComponent<EmbassyListRow>();
            else
            {
                var go = Instantiate(prefab_EmbassyListRow, parentCountryList);
                tmpRow = go.GetComponent<EmbassyListRow>();
            }

            countryList.Add(tmpRow);
            UpdateRow(tmpRow, reg.RegID);
            i++;
        }
    }

    void UpdateRow(EmbassyListRow tmpRow, int RegID)
    {
        nsWorld.Region_Op region = null;
        region = nsWorld.World.TheWorld.Regions[RegID];

        //images:
        tmpRow.flag.sprite = GameManager.GM.GameProperties.Regions[tmpRow.RegID].Flag;
        tmpRow.regimeIcon.sprite = GameManager.GM.GameProperties.GetRegimeIcon(region.Authority);
        //tmpRow.focusIcon = ;
        //texts:
        tmpRow.countryName.text = region.RegName;
        tmpRow.prosperity.text = region.ProsperityLevel.ToString();
        tmpRow.points.text = region.Score.ToString();

        tmpRow.warButton.SetActive(true);

        tmpRow.RegID = region.RegID;

        tmpRow.EmbassyUI = this;
    }

    public void UpgradeMilBase()
    {
        GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.UpgradeMilBaseOuter, new ThreeInt_EventArgs() { int1 = region.RegID, int2 = GameManager.GM.PlayerAuthority, int3 = natFundEmbassy ? 0 : 1 });
        UpdateMilBase();
    }

    public void UpgradeEmbassy()
    {
        GameEventSystem.InvokeEvents(GameEventSystem.MyEventsTypes.UpgradeEmbassyOuter, new ThreeInt_EventArgs() { int1 = region.RegID, int2 = GameManager.GM.PlayerAuthority, int3 = natFundEmbassy ? 0 : 1 });
        UpdateEmbassy();
    }
}
