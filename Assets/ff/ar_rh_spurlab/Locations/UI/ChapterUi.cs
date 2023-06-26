using System;
using ff.ar_rh_spurlab.Localization;
using ff.common.entity;
using ff.common.ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Locations.UI
{
    public class ChapterUi : AbstractLocalizable
    {
        public event Action<Chapter> OnChapterClicked;

        [Header("Prefab References")]
        [SerializeField]
        private Button _button;

        [SerializeField]
        private TMP_Text _titleText;

        [SerializeField]
        private Hidable _visitedIcon;

        [SerializeField]
        private Hidable _nextIcon;

        [Header("Asset References")]
        [SerializeField]
        private TMP_FontAsset _activeFont;

        [SerializeField]
        private TMP_FontAsset _inactiveFont;

        public void Initialize(Chapter chapter)
        {
            _button.onClick.AddListener(OnButtonClickedHandler);

            _chapter = chapter;
            _chapter.IsActive.OnValueChanged += _ => UpdateChapterUiWithState();
            _chapter.IsVisited.OnValueChanged += _ => UpdateChapterUiWithState();
            _chapter.IsNext.OnValueChanged += _ => UpdateChapterUiWithState();
            UpdateChapterUiWithState();
            OnLocaleChangedHandler(ApplicationLocale.Instance.CurrentLocale);
        }

        private void OnButtonClickedHandler()
        {
            if (_chapter != null)
            {
                OnChapterClicked?.Invoke(_chapter);
            }
        }

        protected override void OnLocaleChangedHandler(string locale)
        {
            if (_chapter == null)
                return;

            if (_chapter.Title.TryGetValue(locale, out var title))
            {
                _titleText.text = title;
            }
        }

        private void UpdateChapterUiWithState()
        {
            if (_chapter == null)
                return;

            if (!_visitedIcon.gameObject || !_nextIcon.gameObject || !_titleText.gameObject)
                return;

            _visitedIcon.IsVisible = _chapter.IsVisited.Value && !_chapter.IsNext.Value;
            _nextIcon.IsVisible = _chapter.IsNext.Value;
            _titleText.font = _chapter.IsActive.Value ? _activeFont : _inactiveFont;
        }

        private Chapter _chapter;
    }
}