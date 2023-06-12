using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Playables;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionGroupPlayable : PlayableBehaviour
    {
        public string Name => _asset.name;

        public void Initialize(RevealTransitionGroupAsset asset)
        {
            _asset = asset;
        }

        [CanBeNull]
        public Dictionary<string, RevealTransitionGroupAsset.GroupDefinitionWithReveal> GetActiveDefinitions()
        {
            return _asset.ActiveResolvedDefinitions;
        }

        public SequentialOptions GetSequentialOptions()
        {
            return _asset.SequentialOptions;
        }

        private RevealTransitionGroup _group;
        private RevealTransitionGroupAsset _asset;
    }
}
