using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;


public class BattleCard : MonoBehaviour
{
    [SerializeField] Image UnitImage, Flag;
    [SerializeField] Image SupplyIndicator, EngineIndicator;
    [SerializeField] Text UnitName, UnitVersion;
    [SerializeField] Text Amount;
    [SerializeField] GameObject UserControlButton, AIControlButton;

    bool _userControl;

    public float Supply
    {
        set { SupplyIndicator.fillAmount = value; }
    }

    public float Engine
    {
        set { EngineIndicator.fillAmount = value; }
    }

    public string Amounts
    {
        set { Amount.text = value; }
    }

    public bool UserControl
    {
        get { return _userControl; }

        set
        {
            _userControl = value;
            UserControlButton.SetActive(_userControl);
            AIControlButton.SetActive(!_userControl);
        }
    }

    public void Init(string name, string version, Sprite flag, Sprite Image)
    {
        UnitName.text = name;
        UnitVersion.text = version;
        Flag.sprite = flag;
        UnitImage.sprite = Image;
    }
}
