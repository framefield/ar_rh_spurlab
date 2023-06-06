using System;
using ff.common.entity;
using ff.common.tools;
using TMPro;
using UnityEngine;

namespace ff.ar_rh_spurlab.Localization
{
    public class SetLocalizedText : MonoBehaviour
    {
        [SerializeField]
        private LocalizedString _localizedString;

        [SerializeField]
        [OptionalField]
        private TMP_Text _textIfNotFirstFoundChild;

        private void OnEnable()
        {
            ApplicationLocale.Instance.OnLocaleChange += OnLocaleChangedHandler;
            OnLocaleChangedHandler(ApplicationLocale.Instance.CurrentLocale);
        }

        private void OnDisable()
        {
            ApplicationLocale.Instance.OnLocaleChange -= OnLocaleChangedHandler;
        }

        private TMP_Text ResolveText()
        {
            if (_textIfNotFirstFoundChild)
            {
                return _textIfNotFirstFoundChild;
            }

            return GetComponentInChildren<TMP_Text>();
        }

        private void OnLocaleChangedHandler(string locale)
        {
            var text = ResolveText();
            if (!text)
            {
                Debug.LogWarning($"No text component found on {gameObject.name}", this);
                return;
            }

            if (_localizedString.TryGetValue(locale, out var value))
            {
                text.text = value;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Set Text From Default Localization")]
        public void ApplyDefaultLocalizationToText()
        {
            var text = ResolveText();
            if (text)
            {
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                text.text = _localizedString.DefaultValue();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        [ContextMenu("Set Default Localization From Text")]
        public void ApplyTextToDefaultLocalization()
        {
            var text = ResolveText();
            if (text)
            {
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                _localizedString.SetDefault(text.text);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        private void OnValidate()
        {
            if (!_localizedString.HasDefaultValue())
            {
                ApplyTextToDefaultLocalization();
            }
        }

        [ContextMenu("Toggle Locale de", true)]
        private bool ToggleLocaleDeValid()
        {
            return Application.isPlaying;
        }

        [ContextMenu("Toggle Locale de")]
        private void ToggleLocaleDe()
        {
            if (Application.isPlaying)
            {
                ApplicationLocale.Instance.SetLocale("de");
            }
        }

        [ContextMenu("Toggle Locale en", true)]
        private bool ToggleLocaleEnValid()
        {
            return Application.isPlaying;
        }

        [ContextMenu("Toggle Locale en")]
        private void ToggleLocaleEn()
        {
            if (Application.isPlaying)
            {
                ApplicationLocale.Instance.SetLocale("de");
            }
        }
#endif
    }
}
