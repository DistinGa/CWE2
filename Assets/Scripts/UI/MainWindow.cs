using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CWE2UI
{
    public class MainWindow : MonoBehaviour
    {
        public List<GameObject> Departments;

        public void ChangeDepartment(GameObject dep)
        {
            foreach (var item in Departments)
            {
                item.SetActive(item == dep);
            }
        }
    }
}
