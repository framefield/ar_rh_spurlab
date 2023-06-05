using ff.common.TimelineReveal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionGroup : MonoBehaviour
    {
        [SerializeField]
        private RevealTransitionAutomatic[] _reveals = default;

        public RevealTransitionAutomatic[] Reveals => _reveals;

        public void DeactivateAll()
        {
            foreach (var reveal in _reveals)
            {
                if (reveal) reveal.SetVisibility(false);
            }
        }

        public void SetGroupDefinition(RevealTransitionGroupAsset.GroupDefinition[] definitions)
        {
            var setImmediate = !Application.isPlaying;

            var i = 0;
            for (; i < _reveals.Length; i++)
            {
                if (!_reveals[i]) continue;
                _reveals[i].SetVisibility(definitions[i].IsActive, setImmediate);
            }
        }


        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            foreach (var reveal in _reveals)
            {
                if (reveal)
                    driver.AddFromClip(reveal.gameObject, reveal.GetClip());
            }
        }
    }
}