using UnityEngine;
using UnityEngine.Playables;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionGroupPlayable : PlayableBehaviour
    {
        public void Initialize(RevealTransitionGroupAsset.GroupDefinition[] definitions,
            RevealTransitionGroupAsset asset)
        {
            _definitions = definitions;
            _asset = asset;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_group)
            {
                _group.DeactivateAll();
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _group = playerData as RevealTransitionGroup;

            if (!_group || _definitions == null)
                return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (_definitions.Length != _group.Reveals.Length)
                {
                    _asset.UpdateDefinitions(_group);
                }
            }
#endif
            _group.SetGroupDefinition(_definitions);
        }

        private RevealTransitionGroupAsset.GroupDefinition[] _definitions;
        private RevealTransitionGroup _group;
        private RevealTransitionGroupAsset _asset;
    }
}