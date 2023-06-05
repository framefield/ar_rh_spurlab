using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    [TrackClipType(typeof(RevealTransitionGroupAsset))]
    [TrackBindingType(typeof(RevealTransitionGroup))]
    [TrackColor(0.4f, 0.7f, 0.6f)]
    public class RevealTransitionGroupTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var binding = go.GetComponent<PlayableDirector>().GetGenericBinding(this) as RevealTransitionGroup;
            foreach (var clip in m_Clips)
            {
                if (clip.asset is RevealTransitionGroupAsset asset)
                {
                    asset.SetBinding(binding);
                }
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            base.GatherProperties(director, driver);
            var binding = director.GetGenericBinding(this);

            if (binding == null)
                return;

            var group = binding as RevealTransitionGroup;

            if (group)
            {
                group.GatherProperties(director, driver);
            }
        }
    }
}