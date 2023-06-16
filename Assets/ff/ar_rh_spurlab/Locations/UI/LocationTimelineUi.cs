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
            _pauseButton.onClick.AddListener(timelineManager.Pause);
            _resumeButton.onClick.AddListener(timelineManager.Resume);
            _mapButton.onClick.AddListener(OnMapButtonClickedHandler);

            BuildChaptersUi(chapters);

            OnIsPlayingChanged(LocationTimelineManager.IsPlaying.Value);
            LocationTimelineManager.IsPlaying.OnValueChanged += OnIsPlayingChanged;
        }

        private void OnMapButtonClickedHandler()
        {
            MapUiController.ShowLocationMap(SharedCalibrationContext.ActiveLocation.Id);
        }

        private void OnDestroy()
        {
            LocationTimelineManager.IsPlaying.OnValueChanged -= OnIsPlayingChanged;
        }

        private void OnIsPlayingChanged(bool isPlaying)
        {
            if (_playingPanel)
                _playingPanel.IsVisible = isPlaying;

            if (_pausedPanel)
                _pausedPanel.IsVisible = !isPlaying;
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
    }
}