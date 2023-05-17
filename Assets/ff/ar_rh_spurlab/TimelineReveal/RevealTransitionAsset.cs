using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.common.TimelineReveal
{
    [Serializable]
    public class RevealTransitionAsset : PlayableAsset, ITimelineClipAsset, IPropertyPreview
    {
        public ExposedReference<RevealTransition> _exposedBinding;
        public ClipCaps clipCaps => ClipCaps.Blending;

        public void SetClip(TimelineClip clip)
        {
            _clip = clip;
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<RevealTransitionPlayable>.Create(graph);
            var revealPlayable = playable.GetBehaviour();

            _binding = _exposedBinding.Resolve(graph.GetResolver());

            revealPlayable.Initialize(_clip, _binding);

            if (_binding && _clip != null)
            {
                _clip.displayName = _binding.gameObject.name;
            }

            return playable;
        }

   
        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (!director.playableGraph.IsValid())
            {
                director.RebuildGraph();
            }

            if (!_binding)
            {
                _binding = _exposedBinding.Resolve(director.playableGraph.GetResolver());
            }
            if (_binding)
            {
                driver.AddFromClip(_binding.gameObject, _binding.GetClip());
            }
        }

        private RevealTransition _binding = default;
        private TimelineClip _clip;
    }
}
