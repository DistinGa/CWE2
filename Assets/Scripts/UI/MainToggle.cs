using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CWE2UI
{
    [RequireComponent(typeof(Toggle))]
    public class MainToggle : MonoBehaviour
    {
        public GameObject DepartmmentGO;

        Toggle toggle;

        void Start()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(toggle); });
        }

        void ToggleValueChanged(Toggle tg)
        {
            if (tg.isOn)
            {
                //GameManager.GM.MainWindow.ChangeDepartment(DepartmmentGO);
                FindObjectOfType<MainWindow>().ChangeDepartment(DepartmmentGO);
            }
        }
    }
}