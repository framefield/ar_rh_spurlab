using System;
using ff.ar_rh_spurlab.TimelineReveal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.common.TimelineReveal
{
    [Serializable]
    [TrackClipType(typeof(RevealTransitionAsset))]
    [TrackColor(0.53f, 0.1f, 0.08f)]
#if UNITY_EDITOR
    [System.ComponentModel.DisplayName("Reveal Transition Clip")]
#endif
    public class RevealTransitionTrack : TrackAsset
    {
        [SerializeField]
        private float _defaultFadeDuration = 1f;

        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.easeInDuration = _defaultFadeDuration;
            clip.easeOutDuration = _defaultFadeDuration;
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var clips = GetClips();
            foreach (var clip in clips)
            {
                var asset = clip.asset as RevealTransitionAsset;
                if (!asset)
                {
                    continue;
                }

                asset.SetClip(clip);
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}
