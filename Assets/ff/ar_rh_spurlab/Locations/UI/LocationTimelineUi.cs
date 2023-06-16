using System;
using System.Collections.Generic;
using ff.ar_rh_spurlab.Map;
using ff.ar_rh_spurlab.UI.Site_Ui;
using ff.common.ui;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Locations.UI
{
    public class LocationTimelineUi : MonoBehaviour
    {
        public event Action<Chapter> OnChapterClicked;

        [Header("Prefab References")]
        [SerializeField]
        private Hidable _playingPanel;

        [SerializeField]
        private Hidable _pausedPanel;

        [SerializeField]
        private Button _pauseButton;

        [SerializeField]
        private Button _resumeButton;

        [SerializeField]
        private Button _mapButton;

        [Header("Chapter UI")]
        [SerializeField]
        private Transform _chaptersContainer;

        [SerializeField]
        private ChapterUi _chapterUiPrefab;

        [SerializeField]
        private GameObject _chapterUiSeparatorPrefab;


        public void Initialize(LocationTimelineManager timelineManager, Chapter[] chapters)
        {
            _timelineManager = timelineManager;
            _pauseButton.onClick.AddListener(timelineManager.Pause);
            _resumeButton.onClick.AddListener(timelineManager.Resume);
            _mapButton.onClick.AddListener(OnMapButtonClickedHandler);

            BuildChaptersUi(chapters);

            OnIsPlayingChanged(timelineManager.IsPlaying.Value);
            timelineManager.IsPlaying.OnValueChanged += OnIsPlayingChanged;
            timelineManager.ActiveChapter.OnValueChanged += OnActiveChapterChangedHandler;
        }

        private void OnActiveChapterChangedHandler(Chapter chapter)
        {
            if (chapter != null)
            {
                if (_playingPanel)
                    _playingPanel.IsVisible = !chapter.IsWelcomeChapter && _timelineManager.IsPlaying.Value;

                if (_pausedPanel)
                    _pausedPanel.IsVisible = !chapter.IsWelcomeChapter && !_timelineManager.IsPlaying.Value;
            }
            else
            {
                if (_playingPanel)
                    _playingPanel.IsVisible = false;
                if (_pausedPanel)
                    _pausedPanel.IsVisible = false;
            }
        }

        private void OnMapButtonClickedHandler()
        {
            MapUiController.ShowLocationMap(SharedLocationContext.ActiveLocation.Id);
        }

        private void OnDestroy()
        {
            if (_timelineManager)
            {
                _timelineManager.IsPlaying.OnValueChanged -= OnIsPlayingChanged;
                _timelineManager.ActiveChapter.OnValueChanged -= OnActiveChapterChangedHandler;
            }
        }

        private void OnIsPlayingChanged(bool isPlaying)
        {
            var shouldShowChapterUi = _timelineManager.ActiveChapter.Value != null
                ? !_timelineManager.ActiveChapter.Value.IsWelcomeChapter
                : false;
            if (_playingPanel)
                _playingPanel.IsVisible = shouldShowChapterUi && isPlaying;

            if (_pausedPanel)
                _pausedPanel.IsVisible = shouldShowChapterUi && !isPlaying;
        }


        private void BuildChaptersUi(IReadOnlyList<Chapter> chapters)
        {
            for (var i = 0; i < chapters.Count; i++)
            {
                var newChapterUi = Instantiate(_chapterUiPrefab, _chaptersContainer);
                newChapterUi.Initialize(chapters[i]);
                newChapterUi.OnChapterClicked += c => OnChapterClicked?.Invoke(c);

                if (i != chapters.Count - 1)
                {
                    Instantiate(_chapterUiSeparatorPrefab, _chaptersContainer);
                }
            }
        }

        private LocationTimelineManager _timelineManager;
    }
}