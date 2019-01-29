using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleLocalization
{
	/// <summary>
	/// Localize text component.
	/// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField]
        string _LocalizationKey;

        Text txt;

        public string LocalizationKey
        {
            get { return _LocalizationKey; }

            set
            {
                _LocalizationKey = value;
                Localize();
            }
        }

        public void Start()
        {
            txt = GetComponent<Text>();
            Localize();
            LocalizationManager.LocalizationChanged += Localize;
        }

        public void OnDestroy()
        {
            LocalizationManager.LocalizationChanged -= Localize;
        }

        private void Localize()
        {
            txt.text = LocalizationManager.Localize(_LocalizationKey);
        }
    }
}