using System;
using UnityEngine;
using UnityEngine.UI;
using nsCombat;

public class ViewCU : MonoBehaviour
{
    public CombatUnit cu;
    public bool IsEnemy = false;
    public Action<bool, int> Clicked;

    public Image imgMoveBar;
    public Text txtClass, txtName, txtCount, txtArmor;

    bool active;
    Color color;

    public bool Active
    {
        get { return active; }

        set
        {
            active = value;
            GetComponent<Image>().fillCenter = value;
        }
    }

    public Color Color
    {
        get { return color; }

        set
        {
            color = value;
            GetComponent<Image>().color = value;
        }
    }

    private void Start()
    {
        txtClass.text = nsMilitary.MilitaryManager.Instance.UnitClasses[cu.Class].Name;
    }

    private void OnDisable()
    {
        Clicked = null;
    }

    public void MouseClick()
    {
        Clicked(IsEnemy, cu.ID);
    }

    public void UpdateData()
    {
        txtName.text = cu.Name;
        txtCount.text = cu.Amount.ToString();
        txtArmor.text = cu.Armor.ToString();
        imgMoveBar.fillAmount = 1 - cu.MovementPct;
    }
}
