using System;
using System.Collections.Generic;
using ff.ar_rh_spurlab.Localization;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace ff.ar_rh_spurlab.Locations
{
    /// <summary>
    ///     activates only one timeline at a time
    ///     active timeline is played or paused
    /// </summary>
    public class LocationTimelineManager : AbstractLocalizable, ITrackedLocationContent
    {
        [Header("Settings")]
        [SerializeField]
        private bool _autoTriggerChapters = true;

        [SerializeField]
        private bool _autoPlay = true;

        [Header("Prefab references")]
        [SerializeField]
        private LocationTimelineUi _ui;

        [SerializeField]
        private PlayableDirector _waitingTimeline;

        [SerializeField]
        private PlayableDirector[] _chapterTimelines;


        public bool AutoPlay
        {
            get => _autoPlay;
            // todo trigger play if nothing is playing
            set => _autoPlay = value;
        }

        public static bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (value != _isPlaying)
                {
                    OnIsPlayingChanged?.Invoke(value);
                }

                _isPlaying = value;
            }
        }

        public static event Action<bool> OnIsPlayingChanged;

        public void Initialize()
        {
            if (!_waitingTimeline)
            {
                Debug.LogError("LocationTimelineManager: Waiting timeline is not assigned", this);
                return;
            }

            if (_ui)
                _ui.Initialize(this);

            IsPlaying = false;

            Play(_waitingTimeline);
        }

        public void SetIsTracked(bool isTracked)
        {
            _isTracked = isTracked;

            if (!_activePlayableDirector)
            {
                if (isTracked)
                {
                    PlayNextChapter();
                }

                return;
            }

            if (isTracked)
            {
                if (_activePlayableDirector.time >= _activePlayableDirector.duration)
                {
                    PlayNextChapter();
                }
                else if (!_isPausedByUser)
                {
                    _activePlayableDirector.Resume();
                }
            }
            else
            {
                _activePlayableDirector.Pause();
            }
        }

        private void OnDestroy()
        {
            foreach (var pair in _initialMuteStates)
            {
                pair.Key.muted = pair.Value;
            }

            IsPlaying = false;
        }

        private void Update()
        {
            if (_activePlayableDirector)
            {
                IsPlaying = _activePlayableDirector.state == PlayState.Playing;
            }
            else
            {
                IsPlaying = false;
            }
        }

        protected override void OnLocaleChangedHandler(string locale)
        {
            SetLocalizedTracksMuted(_waitingTimeline, locale);
            foreach (var chapterTimeline in _chapterTimelines)
            {
                SetLocalizedTracksMuted(chapterTimeline, locale);
            }
        }

        private void SetLocalizedTracksMuted(PlayableDirector director, string locale)
        {
            if (!director)
            {
                Debug.LogWarning("LocationTimelineManager: Trying to set muted state on null director", this);
                return;
            }

            var timelineAsset = director.playableAsset as TimelineAsset;

            if (!timelineAsset)
            {
                Debug.LogWarning("LocationTimelineManager: Trying to set muted state on null timeline asset", this);
                return;
            }

            var needsRebuild = false;
            for (var trackIndex = 0; trackIndex < timelineAsset.outputTrackCount; trackIndex++)
            {
                var track = timelineAsset.GetOutputTrack(trackIndex);

                if (track is ILocaleSpecificContent localeSpecificContent)
                {
                    _initialMuteStates.TryAdd(track, track.muted);

                    track.muted = locale != null && localeSpecificContent.Locale != locale;
                    needsRebuild = true;
                }
            }

            if (needsRebuild)
            {
                director.RebuildGraph();
            }
        }

        private void Play(PlayableDirector director)
        {
            if (_activePlayableDirector)
            {
                _activePlayableDirector.stopped -= PlayableDirectorStoppedHandler;
                _activePlayableDirector.Stop();
            }

            _activePlayableDirector = director;
            _activePlayableDirector.Play();
            _activePlayableDirector.stopped += PlayableDirectorStoppedHandler;

            Debug.Log($"LocationTimelineManager: Playing timeline {director.name}", director);
        }

        public void PlayNextChapter()
        {
            var (currentIndex, isFound) = FindIndex(_activePlayableDirector, _chapterTimelines);
            currentIndex++;
            if (currentIndex < _chapterTimelines.Length)
            {
                Play(_chapterTimelines[currentIndex]);
            }
        }

        public void Stop()
        {
            if (_activePlayableDirector)
            {
                _activePlayableDirector.stopped -= PlayableDirectorStoppedHandler;
                _activePlayableDirector.Stop();
            }

            _activePlayableDirector = null;
            Play(_waitingTimeline);
        }

        private void PlayableDirectorStoppedHandler(PlayableDirector director)
        {
            var isWaitingForTrigger = director == _waitingTimeline;
            if (isWaitingForTrigger)
            {
                if (_autoTriggerChapters && _isTracked)
                {
                    PlayChapters();
                }

                return;
            }

            if (!_autoPlay || !_isTracked)
            {
                return;
            }

            var (currentIndex, isFound) = FindIndex(director, _chapterTimelines);

            if (!isFound)
            {
                Debug.LogError(
                    $"LocationTimelineManager: PlayableDirector {director.name} not found in chapter timelines",
                    director);
                return;
            }

            currentIndex++;
            if (currentIndex < _chapterTimelines.Length)
            {
                Play(_chapterTimelines[currentIndex]);
            }
        }

        private static (int, bool) FindIndex(Object searchedObject, IReadOnlyList<Object> objects)
        {
            var currentIndex = -1;
            for (var i = 0; i < objects.Count; i++)
            {
                if (objects[i] == searchedObject)
                {
                    currentIndex = i;
                    break;
                }
            }

            return currentIndex == -1 ? (currentIndex, false) : (currentIndex, true);
        }

        private PlayableDirector _activePlayableDirector;
        private PlayableDirector[] _playableDirectors;

        public void PlayChapters()
        {
            if (!_activePlayableDirector)
            {
                Debug.LogError("LocationTimelineManager: Play chapters are called before waiting timeline is played",
                    this);
                return;
            }

            if (_activePlayableDirector != _waitingTimeline)
            {
                Debug.LogError("LocationTimelineManager: Play chapters are called when chapters are already playing",
                    this);
                return;
            }


            if (_chapterTimelines.Length == 0)
            {
                Debug.LogError("No chapter timelines are assigned.", this);
                return;
            }

            Play(_chapterTimelines[0]);
        }

        public void Pause()
        {
            if (!_activePlayableDirector)
                return;

            _activePlayableDirector.Pause();
            _isPausedByUser = true;
        }

        public void Resume()
        {
            if (!_activePlayableDirector)
                return;

            _activePlayableDirector.Resume();
            _isPausedByUser = false;
        }

        private static readonly Dictionary<TrackAsset, bool> _initialMuteStates = new();
        private bool _isTracked;
        private bool _isPausedByUser;
        private static bool _isPlaying;
    }
}