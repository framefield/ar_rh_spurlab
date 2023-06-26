using System;
using System.Collections.Generic;
using System.Linq;
using ff.ar_rh_spurlab.GrayScaler;
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

        // TODO: replace awake with a meaningful initialization method
        // setting gray scale scene point of interest should be done after SimpleTrackedLocation Content is initialized
        private void Awake()
        {
            _ui.OnChapterClicked += OnChapterClickedHandler;

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

                var portal = story.TimelineManager.Portal;
                var poi = portal.GetComponentInChildren<GrayScaleScenePointOfInterest>();
                if (poi)
                {
                    _storyPOIs.Add((story, poi));
                }
            }

            SetFocusToNextNotVisitedStory();
        }

        private void OnChapterClickedHandler(Chapter chapter)
        {
            if (_activeStory == null)
                return;

            _activeStory.TimelineManager.PlayChapter(chapter);
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
                    SetFocusToNextNotVisitedStory();
                    _activeStory = null;
                }
            }

            if (_activeStory != null)
            {
                _ui.SetTimelineManager(_activeStory.TimelineManager, _activeStory.TimelineManager.Chapters);
            }

            _ui.UpdateVisibility();
        }


        private void SetFocusToNextNotVisitedStory()
        {
            var nextAvailableStory = _stories.FirstOrDefault(story => !story.IsVisited);
            if (nextAvailableStory == null)
                return;

            PlayAudio(nextAvailableStory.CtaAudio);
            SetFocusToStoryPoi(nextAvailableStory);
        }

        private void PlayAudio(LocalizedAudioClip localizedAudioClip)
        {
            if (!localizedAudioClip.TryGetAudioClip(ApplicationLocale.Instance.CurrentLocale, out var audioClip))
                return;

            if (audioClip)
                _audioSource.PlayOneShot(audioClip);
        }

        private void SetFocusToStoryPoi(Story story)
        {
            foreach (var (s, poi) in _storyPOIs)
            {
                poi.enabled = s == story;
            }
        }

        private Story _activeStory;

        private List<(Story, GrayScaleScenePointOfInterest)> _storyPOIs = new();
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