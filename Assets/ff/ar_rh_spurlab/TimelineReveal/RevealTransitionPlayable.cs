using ff.common.TimelineReveal;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionPlayable : PlayableBehaviour
    {
        public void Initialize(TimelineClip clip, RevealTransition binding)
        {
            _binding = binding;
            _clip = clip;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            _state = State.None;
            UpdateContent();
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            (_weight, _state) = TimelineClipInfo.GetRelativeWeightAndState(_clip, playable, info);

            UpdateContent();
        }

        private void UpdateContent()
        {
            if (!_binding)
            {
                return;
            }

            _binding.UpdateNormalizedTime(_weight, _state);
        }


        private RevealTransition _binding;
        private float _weight;
        private State _state;
        private TimelineClip _clip;
    }
}
