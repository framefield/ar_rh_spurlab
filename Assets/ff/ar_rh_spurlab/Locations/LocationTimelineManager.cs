using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace ff.ar_rh_spurlab.Locations
{
    /// <summary>
    ///     activates only one timeline at a time
    ///     active timeline is played or paused
    /// </summary>
    public class LocationTimelineManager : MonoBehaviour, ITrackedLocationContent
    {
        [Header("Settings")]
        [SerializeField]
        private bool _autoTriggerChapters = true;

        [SerializeField]
        private bool _autoPlay = true;

        [Header("Prefab references")]
        [SerializeField]
        private PlayableDirector _waitingTimeline;

        [SerializeField]
        private PlayableDirector[] _chapterTimelines;


        public void Initialize()
        {
            if (!_waitingTimeline)
            {
                Debug.LogError("LocationTimelineManager: Waiting timeline is not assigned", this);
                return;
            }

            Play(_waitingTimeline);
        }

        public void SetIsTracked(bool isTracked)
        {
            if (!_activePlayableDirector)
            {
                return;
            }

            if (isTracked)
            {
                _activePlayableDirector.Resume();
                // todo if the timeline is paused by user do not resume
            }
            else
            {
                _activePlayableDirector.Pause();
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
                if (_autoTriggerChapters)
                    PlayChapters();
                return;
            }

            if (!_autoPlay)
                return;

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
    }
}
