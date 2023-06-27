using ff.ar_rh_spurlab.TimelineReveal;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.common.TimelineReveal.Editor
{
    [CustomTimelineEditor(typeof(RevealTransitionAsset))]
    public class RevealTransitionAssetEditor : UnresolvedReferenceClipEditorBase
    {
        protected override bool IsFullyResolved(TimelineClip clip, PlayableDirector director)
        {
            var revealAsset = clip.asset as RevealTransitionAsset;
            if (revealAsset != null)
            {
                var resolved = revealAsset._exposedBinding.Resolve(director);
                return resolved;
            }

            return true;
        }
    }
}