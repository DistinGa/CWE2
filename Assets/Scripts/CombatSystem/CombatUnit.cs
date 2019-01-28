using System;
using System.Collections;
using System.Collections.Generic;
using nsMilitary;

public class CombatUnit: ISavable
{
    bool _Manual;    //Группа управляется игроком
    bool _ActionSelected;    //Игрок отдал приказ группе
    int _Armor, _InitArmor;
    int _MovementCnt; //Счётчик перемещения (когда доходит до нуля, юнит может перемещаться)
    int _Position; //Как далеко от центра находится группа (начинается с 1)
    int _Supply;

    int _Amount;
    public int ID { get; private set; }
    public int UnitID { get; private set; }
    public Dictionary<int, bool> WeaponReady { get; private set; }  //Готовность оружия к стрельбе (после выстрела сбрасывается)
    public int HomeBaseID { get; private set; }     //Из какой базы пришли юниты и куда уюдут после окончания боя (-1 - домашний пул)

    public CombatUnit(int UnitID, int Amount, int maxID, int HomeBaseID = -1)
    {
        ID = ++maxID;
        this.UnitID = UnitID;
        this.HomeBaseID = HomeBaseID;
        _Amount = Amount;
        _InitArmor = Amount * Unit.Armor;
        _Armor = Amount * Unit.Armor;
        Supply = Unit.Supply;
        Position = Unit.StartPosition;
        _MovementCnt = 0;
        _ActionSelected = false;

        WeaponReady = new Dictionary<int, bool>();
        foreach (var item in Unit.Weapon)
        {
            WeaponReady.Add(item, true);
        }
    }

    #region Properties
    public int RestArmorPercent
    {
        get { return _Armor / _InitArmor * 100; }
    }

    public MilitaryUnit Unit
    {
        get { return MilitaryManager.Instance.GetMilitaryUnit(UnitID); }
    }

    public bool Manual
    {
        get { return _Manual; }
        set { _Manual = value; }
    }

    public string Name
    {
        get { return Unit.UnitName; }
    }

    public int Class
    {
        get { return Unit.UnitClass; }
    }

    public int Armor
    {
        get { return _Armor; }

        set { _Armor = value; }
    }

    public int MovementCnt
    {
        get { return _MovementCnt; }

        set { _MovementCnt = value; }
    }

    public int Position
    {
        get { return _Position; }

        set { _Position = value; }
    }

    public int Supply
    {
        get { return _Supply; }

        set { _Supply = value; }
    }

    public int Amount
    {
        get { return _Amount; }

        set { _Amount = value; }
    }

    public int Stealth
    {
        get { return Unit.Stealth; }
    }

    public Dictionary<int, int> HitPoints
    {
        get { return Unit.HitPoints; }
    }

    public int Maneuver
    {
        get { return Unit.Maneuver; }
    }

    public int Engine
    {
        get { return Unit.Engine; }
    }

    public int Countermeasures
    {
        get { return Unit.Countermeasures; }
    }

    public int Radar
    {
        get { return Unit.Radar; }
    }

    public bool ActionSelected
    {
        get { return _ActionSelected; }
        set { _ActionSelected = value; }
    }
    #endregion

    public List<int> GetTargetClasses(int WeaponID)
    {
        return Unit.GetTargetClasses(WeaponID);
    }

    public int GetHitpoints(int WeaponID)
    {
        return Unit.GetHitPoints(WeaponID);
    }

    public void Fire(int WeaponID)
    {
        WeaponReady[WeaponID] = false;
        Supply -= MilitaryManager.Instance.GetSystemWeapon(WeaponID).FireCost;
        _ActionSelected = true;
    }

    /// <summary>
    /// Получение урона и сокращение количества юнитов в группе.
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(int amount)
    {
        Armor -= amount;
        Amount = Armor / Unit.Armor;
        if (Armor % Unit.Armor > 0) Amount++;
    }

    /// <summary>
    /// Перезарядка
    /// </summary>
    public void Resupply()
    {
        Supply = Unit.Supply;
        _ActionSelected = true;
    }

    /// <summary>
    /// Перемещение боевой группы
    /// </summary>
    /// <param name="CU"></param>
    /// <param name="dir">-1 - к центру; 1 - от центра</param>
    public void Move(int dir, int MovementValue)
    {
        if (MovementCnt <= 0)
        {
            dir = Math.Sign(dir);
            Position += dir;
            MovementCnt = MovementValue;
            _ActionSelected = true;
        }
    }

}
