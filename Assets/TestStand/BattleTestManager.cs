using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using nsMilitary;
using nsCombat;
using nsWorld;
using NaughtyAttributes;

public class BattleTestManager : MonoBehaviour
{
    public List<UnitClass> UnitClasses;
    [Header("Наши")]
    public List<NeutralMilitaryUnit> MyArmy;
    [Header("Враги")]
    public List<NeutralMilitaryUnit> EnemyArmy;

    [Space(10)]
    public int MovementValue;

    public bool ShowAdditional;

    [ShowIf("ShowAdditional")] public GameObject cuPrefab;
    [ShowIf("ShowAdditional")] public List<Transform> MyLines, EnemyLines;
    [ShowIf("ShowAdditional")] public Color ActiveUnitColor, TargetColor, InitialColor;

    [Space(10)]
    [ShowIf("ShowAdditional")] public GameObject goWinScreen;
    [ShowIf("ShowAdditional")] public GameObject goLooseScreen;

    [Space(10)]
    [ShowIf("ShowAdditional")] public Toggle tgManual;
    [ShowIf("ShowAdditional")] public Text txtName;
    [ShowIf("ShowAdditional")] public Text txtArmor, txtAmount, txtSupply, txtHitponts, txtFireCost, txtRange, txtRadar, txtStealth, txtTargetClasses, txtCountermeasures, txtManouver;
    [ShowIf("ShowAdditional")] public Button btnRecharge, btnMoveBack, btnMoveFor, btnAttack;

    private CombatData _combat = new CombatData();
    private nsAI.AI AI;
    private Dictionary<int, List<Transform>> cuLines = new Dictionary<int, List<Transform>>();  // Связь CombatUnit и списка линий, по которым она перемещается.
    private Dictionary<int, ViewCU> cuViews = new Dictionary<int, ViewCU>();    // Связь CombatUnit и её вьюшки.
    int myActiveCU = -1, selectedTarget = -1;

    void Awake()
    {
        MilitaryManager.CreateMilitaryManager(new MilitaryManager_Ds());
        for (int i = 0; i < UnitClasses.Count; i++)
        {
            MilitaryManager.Instance.UnitClasses[UnitClasses[i].ClassID] = UnitClasses[i];
        }

        for (int i = 0; i < UnitClasses.Count; i++)
        {
            MilitaryManager.Instance.UnitClasses[i] = UnitClasses[i];
        }

        AI = new nsAI.AI();

        _combat.Active = true;
        _combat.AttackerRegID = 0;
        _combat.RegID = 1;
        _combat.AttackerMoral = 1000;
        _combat.DefenderMoral = 1000;
        _combat.AttackerUnits = new Dictionary<int, CombatUnit>();
        _combat.DefenderUnits = new Dictionary<int, CombatUnit>();
        _combat.CombatArea = 6;
        _combat.CenterCombatArea = 3;
        _combat.MovementValue = MovementValue;

        CombatManager.Instance.AddCombat(_combat);

        //Заполнение списков линий
        foreach (Transform item in GameObject.Find("LeftPanel").transform)
        {
            MyLines.Add(item);
        }
        MyLines.Reverse();
        foreach (Transform item in GameObject.Find("RightPanel").transform)
        {
            EnemyLines.Add(item);
        }

        for (int i = 0; i < MyArmy.Count; i++)
        {
            MyArmy[i].Authority = 0;
            int UnitID = MilitaryManager.Instance.NewMilitaryUnit(MyArmy[i]);
            _combat.AttackerUnits[i] = new CombatUnit(i, UnitID, MyArmy[i]._count, _combat.MovementValue);

            cuLines[_combat.AttackerUnits[i].ID] = MyLines;
            //GameObject go = Instantiate(cuPrefab, MyLines[_combat.AttackerUnits[i].Position - 1].transform);
            GameObject go = Instantiate(cuPrefab, MyLines[MilitaryManager.Instance.UnitClasses[_combat.AttackerUnits[i].Class].StartPosition - 1].transform);
            var view = go.GetComponent<ViewCU>();
            view.cu = _combat.AttackerUnits[i];
            view.IsEnemy = false;
            view.Color = ActiveUnitColor;
            view.Clicked = OnClick;
            cuViews[_combat.AttackerUnits[i].ID] = view;
            UpdateView(view);
        }

        int offset = 1000;
        for (int i = 0; i < EnemyArmy.Count; i++)
        {
            EnemyArmy[i].Authority = 1;
            int UnitID = MilitaryManager.Instance.NewMilitaryUnit(EnemyArmy[i]);
            _combat.DefenderUnits[i + offset] = new CombatUnit(i + offset, UnitID, EnemyArmy[i]._count, _combat.MovementValue);

            cuLines[_combat.DefenderUnits[i + offset].ID] = EnemyLines;
            
            //GameObject go = Instantiate(cuPrefab, EnemyLines[_combat.DefenderUnits[i + offset].Position - 1].transform);
            GameObject go = Instantiate(cuPrefab, EnemyLines[MilitaryManager.Instance.UnitClasses[_combat.DefenderUnits[i + offset].Class].StartPosition - 1].transform);
            var view = go.GetComponent<ViewCU>();
            view.cu = _combat.DefenderUnits[i + offset];
            view.IsEnemy = true;
            view.Clicked = OnClick;
            cuViews[i + offset] = view;
            UpdateView(view);
        }

        UpdateData(true, null);
    }

    private void OnEnable()
    {
        nsEventSystem.GameEventSystem.Subscribe(nsEventSystem.GameEventSystem.MyEventsTypes.EndOfCombat, EndOfCombat);
    }

    private void OnDisable()
    {
        nsEventSystem.GameEventSystem.UnSubscribe(nsEventSystem.GameEventSystem.MyEventsTypes.EndOfCombat, EndOfCombat);
    }

    void OnClick(bool isEnemy, int cuID)
    {
        if (isEnemy)
        {
            if (cuViews[cuID].Color == TargetColor)
            {
                if(selectedTarget != -1)
                    cuViews[selectedTarget].Active = false;

                cuViews[cuID].Active = true;
                selectedTarget = cuID;
            }
        }
        else
        {
            if(myActiveCU != -1)
                cuViews[myActiveCU].Active = false;

            cuViews[cuID].Active = true;

            myActiveCU = cuID;
            if(selectedTarget != -1)
                cuViews[selectedTarget].Active = false;

            selectedTarget = -1;

            // Выделение потенциальных целей.
            foreach (var item in cuViews.Values)
            {
                if (item.IsEnemy)
                {
                    item.Color = InitialColor;
                    item.Active = false;
                }
            }
            var targets = cuViews[myActiveCU].cu.GetTargetsInrange(_combat.DefenderUnits.Values.ToList(), 0);
            foreach (var item in targets)
            {
                cuViews[item.ID].Color = TargetColor;
            }
        }

        UpdateData(isEnemy, cuID);
    }

    void UpdateView(int cuID)
    {
        if (cuViews[cuID].transform != cuLines[cuID][cuViews[cuID].cu.Position - 1].transform)
            cuViews[cuID].transform.SetParent(cuLines[cuID][cuViews[cuID].cu.Position - 1].transform);

        if (cuViews[cuID].cu.ActionSelected)
            cuViews[cuID].Color = InitialColor;
        else
            cuViews[cuID].Color = ActiveUnitColor;

        cuViews[cuID].UpdateData();
    }

    void UpdateView(ViewCU cuView)
    {
        UpdateView(cuView.cu.ID);
    }

    public void NextTurn()
    {
        CombatManager.Instance.CommonTurn(_combat, 0);
        AI.CombatProcessing(_combat, 1);
        CombatManager.Instance.CommonTurn(_combat, 1);

        nsEventSystem.GameEventSystem.InvokeEvents(nsEventSystem.GameEventSystem.MyEventsTypes.TurnActions);

        // Обновление визуальных данных
        List<int> _tmpList = new List<int>();
        foreach (var cuView in cuViews.Values)
        {
            var cu = cuView.cu;
            if (cu.Armor <= 0)
                _tmpList.Add(cu.ID);
            else
                UpdateView(cu.ID);
        }

        foreach (var item in _tmpList)
        {
            DeleteCombatUnit(cuViews[item].IsEnemy, cuViews[item].cu);
        }

        UpdateData(false, myActiveCU);
    }

    public void MoveForward()
    {
        if (myActiveCU != -1)
        {
            CombatManager.Instance.MoveForward(_combat, _combat.AttackerUnits[myActiveCU]);
            UpdateView(myActiveCU);
            UpdateData(false, myActiveCU);
        }
    }

    public void MoveBackward()
    {
        if (myActiveCU != -1)
        {
            CombatManager.Instance.MoveBackward(_combat, _combat.AttackerUnits[myActiveCU]);
            UpdateView(myActiveCU);
            UpdateData(false, myActiveCU);
        }
    }

    public void Recharge()
    {
        if (myActiveCU != -1)
        {
            _combat.AttackerUnits[myActiveCU].Resupply();
            UpdateData(false, myActiveCU);
        }
    }

    public void Attack()
    {
        if (myActiveCU != -1 && selectedTarget != -1)
        {
            CombatManager.Instance.Attack(cuViews[myActiveCU].cu, cuViews[selectedTarget].cu, 0);
            UpdateView(myActiveCU);
            UpdateView(selectedTarget);
            UpdateData(false, myActiveCU);

            if (cuViews[selectedTarget].cu.Amount <= 0)
            {
                DeleteCombatUnit(true, cuViews[selectedTarget].cu);
                UpdateData(true, null);
                CombatManager.Instance.CheckCombatResult(_combat);
                return;
            }
            else
                UpdateData(true, selectedTarget);
        }
    }

    /// <summary>
    /// Обновление отображения параметров выбранной CombatUnit и кнопок.
    /// </summary>
    /// <param name="isEnemy"></param>
    /// <param name="cu"></param>
    void UpdateData(bool isEnemy, CombatUnit cu)
    {
        btnRecharge.interactable = false;
        btnMoveBack.interactable = false;
        btnMoveFor.interactable = false;
        btnAttack.interactable = false;
        tgManual.gameObject.SetActive(false);

        if (cu == null)
        {
            txtName.text = "--";
            txtArmor.text = "--";
            txtAmount.text = "--";
            txtSupply.text = "--";
            txtHitponts.text = "--";
            txtFireCost.text = "--";
            txtRange.text = "--";
            txtRadar.text = "--";
            txtStealth.text = "--";
            txtTargetClasses.text = "";
            txtCountermeasures.text = "";
            txtManouver.text = "";
        }
        else
        {
            txtName.text = cu.Name;
            txtArmor.text = cu.Armor.ToString();
            txtAmount.text = cu.Amount.ToString();
            txtSupply.text = cu.Supply.ToString();
            txtHitponts.text = cu.GetHitpoints(0).ToString();
            txtFireCost.text = cu.Unit.GetFireCost(0).ToString();
            txtRange.text = cu.Unit.GetRange(0).ToString();
            txtRadar.text = cu.Radar.ToString();
            txtStealth.text = cu.Stealth.ToString();
            txtCountermeasures.text = cu.Countermeasures.ToString();
            txtManouver.text = cu.Maneuver.ToString();

            var l = cu.GetTargetClasses(0);
            string str = "";
            foreach (var item in l)
            {
                str += (str == ""? "": "\n") + MilitaryManager.Instance.UnitClasses[item].Name;
            }
            txtTargetClasses.text = str;

            // Кнопки
            if (myActiveCU != -1)
            {
                CombatUnit myCU = cuViews[myActiveCU].cu;

                if (!myCU.ActionSelected)
                {
                    btnRecharge.interactable = myCU.Supply < myCU.Unit.Supply;
                    btnMoveBack.interactable = myCU.MovementCnt <= 0;
                    btnMoveFor.interactable = myCU.MovementCnt <= 0;
                    btnAttack.interactable = myCU.Supply >= myCU.Unit.GetFireCost(0) && selectedTarget != -1;
                }
            }

            // Чекбокс "Manual".
            if (!isEnemy)
            {
                tgManual.gameObject.SetActive(true);
                tgManual.isOn = cu.Manual;
            }
        }
    }

    void UpdateData(bool isEnemy, int cuID)
    {
        if(cuID == -1)
            UpdateData(isEnemy, null);
        else
            UpdateData(isEnemy, cuViews[cuID].cu);
    }

    void DeleteCombatUnit(bool isEnemy, CombatUnit cu)
    {
        Destroy(cuViews[cu.ID].gameObject);
        cuViews.Remove(cu.ID);
        cuLines.Remove(cu.ID);
        CombatManager.Instance.DeleteCombatUnit(_combat, !isEnemy, cu.ID);

        if (isEnemy)
            selectedTarget = -1;
        else
            myActiveCU = -1;
    }

    public void SetCUManual(bool value)
    {
        if(myActiveCU != -1)
            cuViews[myActiveCU].cu.Manual = value;
    }

    void EndOfCombat(object sender, EventArgs e)
    {
        int WinnerRegID = (e as nsEventSystem.EndOfCombat_EventArgs).WinnerRegID;
        int combatID = (e as nsEventSystem.EndOfCombat_EventArgs).CombatID;

        if (WinnerRegID == 0)
            goWinScreen.SetActive(true);
        else
            goLooseScreen.SetActive(true);
    }
}
