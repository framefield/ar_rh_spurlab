using System;
using ff.ar_rh_spurlab.Localization;
using ff.ar_rh_spurlab.Locations.UI;
using ff.common.entity;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationStoryManager : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _audioSource;

        [SerializeField]
        private LocationTimelineUi _ui;

        [SerializeField]
        private Story[] _stories;

        private void Start()
        {
            foreach (var story in _stories)
            {
                if (!story.TimelineManager)
                {
                    Debug.LogError($"TimelineManager is not set for {story.Id}", this);
                    continue;
                }

                story.TimelineManager.IsActive.OnValueChanged += isActive =>
                {
                    OnStoryIsActiveChangedHandler(story, isActive);
                };
            }

            PlayCtaAudio();
        }

        private void OnStoryIsActiveChangedHandler(Story story, bool isActive)
        {
            if (isActive)
            {
                Story storyToDeactivate = null;
                if (_activeStory != null && _activeStory != story)
                {
                    storyToDeactivate = _activeStory;
                }

                _audioSource.Stop();
                _activeStory = story;
                _activeStory.IsVisited = true;

                storyToDeactivate?.TimelineManager.Deactivate();
            }
            else
            {
                if (_activeStory == story)
                {
                    PlayCtaAudio();
                    _activeStory = null;
                }
            }

            if (_activeStory != null)
            {
                _ui.SetTimelineManager(_activeStory.TimelineManager, _activeStory.TimelineManager.Chapters);
            }

            _ui.UpdateVisibility();
        }

        private void PlayCtaAudio()
        {
            foreach (var story in _stories)
            {
                if (story.IsVisited)
                    continue;

                if (!story.CtaAudio.TryGetAudioClip(ApplicationLocale.Instance.CurrentLocale, out var audioClip))
                    continue;

                if (audioClip)
                    _audioSource.PlayOneShot(audioClip);

                return;
            }
        }

        private Story _activeStory;
    }

    [Serializable]
    public class Story
    {
        public string Id;
        public LocationTimelineManager TimelineManager;
        public LocalizedAudioClip CtaAudio;

        [NonSerialized]
        public bool IsVisited;
    }
}