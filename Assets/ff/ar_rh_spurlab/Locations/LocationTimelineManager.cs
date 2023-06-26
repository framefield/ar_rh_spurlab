using System;
using System.Collections.Generic;
using ff.ar_rh_spurlab.GrayScaler;
using ff.ar_rh_spurlab.Localization;
using ff.ar_rh_spurlab.Locations.UI;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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
        private bool _autoPlay = false;


        [Header("Prefab references")]
        [SerializeField]
        private Chapter[] _chapters;

        [SerializeField]
        private Portal _portal;

        public bool AutoPlay
        {
            get => _autoPlay;
            // todo trigger play if nothing is playing
            set => _autoPlay = value;
        }

        public Chapter[] Chapters => _chapters;

        public readonly static ReactiveProperty<bool> IsAnyTimelineMgrPlaying = new();

        public readonly ReactiveProperty<bool> IsPlaying = new();
        public readonly ReactiveProperty<Chapter> ActiveChapter = new();
        public readonly ReactiveProperty<bool> IsActive = new();

        public void Initialize()
        {
            if (!_portal)
            {
                Debug.LogError("Portal is not set", this);
                enabled = false;
                return;
            }

            _portal.OnExit += OnPortalExitHandler;
            _portal.OnEnter += OnPortalEnterHandler;

            IsPlaying.Value = false;
            SetTimelinePlaying(this, false);
        }

        private void OnPortalEnterHandler()
        {
            Activate();
        }

        private void OnPortalExitHandler()
        {
            Deactivate();
        }

        public void Activate()
        {
            AutoPlay = true;
            PlayNextChapter();
            IsActive.Value = true;
        }

        public void Deactivate()
        {
            AutoPlay = false;
            Stop();
            IsActive.Value = false;
        }

        public void SetIsTracked(bool isTracked)
        {
            _isTracked = isTracked;

            if (_isPausedByUser)
            {
                return;
            }

            if (!_activePlayableDirector)
            {
                //  if (isTracked)
                //  {
                //      PlayNextChapter();
                //  }

                return;
            }

            if (isTracked)
            {
                if (_autoPlay && _activePlayableDirector.time >= _activePlayableDirector.duration)
                {
                    PlayNextChapter();
                }
                else
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
                pair.Key.muted = pair.Value.isMuted;
            }

            IsPlaying.Value = false;
            SetTimelinePlaying(this, IsPlaying.Value);
        }

        private void Update()
        {
            if (_activePlayableDirector)
            {
                IsPlaying.Value = _activePlayableDirector.state == PlayState.Playing;
            }
            else
            {
                IsPlaying.Value = false;
            }

            SetTimelinePlaying(this, IsPlaying.Value);
            _initialMuteStates.Clear();
        }

        protected override void OnLocaleChangedHandler(string locale)
        {
            foreach (var chapter in _chapters)
            {
                SetLocalizedTracksMuted(chapter.Timeline, locale);
            }
        }

        private void SetLocalizedTracksMuted(PlayableDirector director, string locale)
        {
            if (!director)
            {
                Debug.LogWarning("LocationTimelineManager: Trying to set muted state on null director", this);
                return;
            }

            if (!director.playableGraph.IsValid())
            {
                director.RebuildGraph();
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
                    _initialMuteStates.TryAdd(track, (track.muted, director));

                    track.muted = locale != null && localeSpecificContent.Locale != locale;
                    needsRebuild = true;
                }
            }

            if (needsRebuild)
            {
                director.RebuildGraph();
            }
        }

        public void PlayChapter(Chapter chapter)
        {
            // todo check if user is in portal
            // todo OR do not show chapter ui if user is not in portal
            var i = 0;
            var chapterIndex = -1;

            foreach (var c in _chapters)
            {
                c.IsActive.Value = false;
                if (c == chapter)
                {
                    chapterIndex = i;
                }

                c.IsNext.Value = chapterIndex != -1 && i == chapterIndex + 1;
                i++;
            }

            chapter.IsActive.Value = true;
            chapter.IsVisited.Value = true;

            ActiveChapter.Value = chapter;
            Play(chapter.Timeline);
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
            var (currentIndex, isFound) = FindIndex(_activePlayableDirector, _chapters);
            currentIndex++;
            if (currentIndex < _chapters.Length)
            {
                PlayChapter(_chapters[currentIndex]);
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
            ActiveChapter.Value = null;
        }

        private void PlayableDirectorStoppedHandler(PlayableDirector director)
        {
            if (!_autoPlay || !_isTracked)
            {
                return;
            }

            var (currentIndex, isFound) = FindIndex(director, _chapters);

            if (!isFound)
            {
                Debug.LogError(
                    $"LocationTimelineManager: PlayableDirector {director.name} not found in chapter timelines",
                    director);
                return;
            }

            currentIndex++;
            if (currentIndex < _chapters.Length)
            {
                PlayChapter(_chapters[currentIndex]);
            }
        }

        private static (int, bool) FindIndex(PlayableDirector timeline, Chapter[] chapters)
        {
            var currentIndex = -1;
            for (var i = 0; i < chapters.Length; i++)
            {
                if (chapters[i].Timeline == timeline)
                {
                    currentIndex = i;
                    break;
                }
            }

            return currentIndex == -1 ? (currentIndex, false) : (currentIndex, true);
        }


        public void PlayChapters()
        {
            if (!_activePlayableDirector)
            {
                Debug.LogError("LocationTimelineManager: Play chapters are called before waiting timeline is played",
                    this);
                return;
            }

            if (_activePlayableDirector)
            {
                Debug.LogError("LocationTimelineManager: Play chapters are called when chapters are already playing",
                    this);
                return;
            }


            if (+_chapters.Length == 0)
            {
                Debug.LogError("No chapter timelines are assigned.", this);
                return;
            }

            PlayChapter(_chapters[0]);
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

        private static void SetTimelinePlaying(LocationTimelineManager mgr, bool isPlaying)
        {
            if (IsAnyTimelineMgrPlaying.Value == isPlaying)
                return;

            if (isPlaying)
            {
                _currentTimelineMgr = mgr;
                IsAnyTimelineMgrPlaying.Value = true;
            }
            else
            {
                if (_currentTimelineMgr == mgr)
                {
                    _currentTimelineMgr = null;
                    IsAnyTimelineMgrPlaying.Value = false;
                }
            }
        }

        private static LocationTimelineManager _currentTimelineMgr = null;

        private readonly Dictionary<TrackAsset, (bool isMuted, PlayableDirector director)> _initialMuteStates =
            new();

        private PlayableDirector _activePlayableDirector;
        private PlayableDirector[] _playableDirectors;

        private bool _isTracked;
        private bool _isPausedByUser;
        private static bool _isPlaying;
    }
}