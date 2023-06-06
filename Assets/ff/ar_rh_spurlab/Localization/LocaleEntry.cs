using System;
using ff.common.entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Localization
{
    public class LocaleEntry : AbstractLocalizable
    {
        [SerializeField]
        private string _locale = "en";

        [SerializeField]
        private TMP_Text _text;

        [SerializeField]
        private TextStyle _selectedStyle;

        [SerializeField]
        private TextStyle _normalStyle;

        protected override void OnEnable()
        {
            base.OnEnable();
            GetComponent<Button>().onClick.AddListener(OnClickedHandler);
        }

        protected override void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(OnClickedHandler);
            base.OnDisable();
        }

        private void OnClickedHandler()
        {
            ApplicationLocale.Instance.SetLocale(_locale);
        }

        protected override void OnLocaleChangedHandler(string locale)
        {
            if (locale == _locale)
            {
                _selectedStyle.Apply(_text);
            }
            else
            {
                _normalStyle.Apply(_text);
            }
        }
    }


    [Serializable]
    struct TextStyle
    {
        public TMP_FontAsset font;

        public void Apply(TMP_Text text)
        {
            text.font = font;
        }
    }
}
