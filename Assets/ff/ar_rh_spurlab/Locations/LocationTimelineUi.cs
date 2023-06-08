using System;
using ff.common.ui;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationTimelineUi : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private Hidable _playingPanel;

        [SerializeField]
        private Hidable _pausedPanel;

        [SerializeField]
        private Button _pauseButton;

        [SerializeField]
        private Button _resumeButton;
        
        

        public void Initialize(LocationTimelineManager timelineManager, LocationTimelineManager.Chapter[] chapters)
        {
            _pauseButton.onClick.AddListener(timelineManager.Pause);
            _resumeButton.onClick.AddListener(timelineManager.Resume);

            OnIsPlayingChanged(LocationTimelineManager.IsPlaying);
            LocationTimelineManager.OnIsPlayingChanged += OnIsPlayingChanged;
        }

        private void OnDestroy()
        {
            LocationTimelineManager.OnIsPlayingChanged -= OnIsPlayingChanged;
        }

        private void OnIsPlayingChanged(bool isPlaying)
        {
            if (_playingPanel)
                _playingPanel.IsVisible = isPlaying;

            if (_pausedPanel)
                _pausedPanel.IsVisible = !isPlaying;
        }
    }
}