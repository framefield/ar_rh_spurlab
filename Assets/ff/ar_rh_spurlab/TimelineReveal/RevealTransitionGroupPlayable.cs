using UnityEngine.Playables;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionGroupPlayable : PlayableBehaviour
    {
        public void Initialize(RevealTransitionGroupAsset.GroupDefinition[] definitions)
        {
            _definitions = definitions;
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

            if (_definitions != null && _group)
            {
                _group.SetGroupDefinition(_definitions);
            }
        }

        private RevealTransitionGroupAsset.GroupDefinition[] _definitions;
        private RevealTransitionGroup _group;
    }
}