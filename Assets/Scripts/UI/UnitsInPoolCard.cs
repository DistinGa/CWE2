using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitsInPoolCard : MonoBehaviour
{
    int _fullAmount = 1000;
    [SerializeField] Scrollbar ScrollBar;
    [SerializeField] InputField InputField;
    [SerializeField] Image UnitIcon;
    int _toSendAmount;

	// Use this for initialization
	void Start () {
		
	}

    public void ScrollSync()
    {
        _toSendAmount = (int)Mathf.RoundToInt(ScrollBar.value * _fullAmount);
        InputField.text = _toSendAmount.ToString();
    }

    public void HandInputSync()
    {
        if (!int.TryParse(InputField.text, out _toSendAmount))
            _toSendAmount = 0;

        ScrollBar.value = (float)_toSendAmount / (float)_fullAmount;
    }
}
