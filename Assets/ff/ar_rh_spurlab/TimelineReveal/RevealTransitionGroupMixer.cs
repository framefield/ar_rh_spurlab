using System;
using System.Collections.Generic;
using System.Linq;
using ff.common.TimelineReveal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionGroupMixer : PlayableBehaviour
    {
        public TimelineClip[] Clips;
        private RevealTransitionGroup _group;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!_group)
            {
                _group = playerData as RevealTransitionGroup;
            }

            if (!_group)
            {
                return;
            }

            var revealStateTuplesById = new Dictionary<string, (float weight, State state)>();
            var count = playable.GetInputCount();
            for (var i = 0; i < count; ++i)
            {
                var inputWeight = playable.GetInputWeight(i);
                if (inputWeight > 0)
                {
                    var input = (ScriptPlayable<RevealTransitionGroupPlayable>)playable.GetInput(i);
                    var revealTransitionGroupPlayable = input.GetBehaviour();

                    var currentClip = Clips[i];
                    var definitionAndRevealById = revealTransitionGroupPlayable.GetActiveDefinitions();
                    var sequentialOptions = revealTransitionGroupPlayable.GetSequentialOptions();

                    if (definitionAndRevealById == null)
                    {
                        continue;
                    }

                    var nextClip = i < count - 1 ? Clips[i + 1] : null;
                    Dictionary<string, RevealTransitionGroupAsset.GroupDefinitionWithReveal> nextDefinitionsById =
                        null;
                    if (nextClip != null && Math.Abs(nextClip.start - currentClip.end) < 0.01d)
                    {
                        var nextInput = (ScriptPlayable<RevealTransitionGroupPlayable>)playable.GetInput(i + 1);
                        var nextTransitionGroupPlayable = nextInput.GetBehaviour();
                        nextDefinitionsById = nextTransitionGroupPlayable.GetActiveDefinitions();
                    }

                    var prevClip = i > 0 ? Clips[i - 1] : null;
                    Dictionary<string, RevealTransitionGroupAsset.GroupDefinitionWithReveal> prevDefinitionsById =
                        null;
                    if (prevClip != null && Math.Abs(prevClip.end - currentClip.start) < 0.01d)
                    {
                        var prevInput = (ScriptPlayable<RevealTransitionGroupPlayable>)playable.GetInput(i - 1);
                        var prevTransitionGroupPlayable = prevInput.GetBehaviour();
                        prevDefinitionsById = prevTransitionGroupPlayable.GetActiveDefinitions();
                    }

                    var offsetIndex = 0;
                    foreach (var (id, value) in definitionAndRevealById.OrderBy(d => d.Value.Index))
                    {
                        var definition = value.Definition;
                        var reveal = value.Reveal;

                        var existInPrev = prevDefinitionsById != null && prevDefinitionsById.ContainsKey(id);
                        var existsInNext = nextDefinitionsById != null && nextDefinitionsById.ContainsKey(id);

                        var (weight, state) =
                            sequentialOptions.PlaySequentially
                                ? TimelineClipInfo.CalculateSequentialWeightAndState(currentClip, playable,
                                    sequentialOptions,
                                    offsetIndex, reveal.FadeInDuration, reveal.FadeOutDuration,
                                    existInPrev, existsInNext)
                                : TimelineClipInfo.CalculateAbsoluteWeightAndState(currentClip, playable,
                                    reveal.FadeInDuration, reveal.FadeOutDuration,
                                    existInPrev, existsInNext);

                        revealStateTuplesById.Add(id, (weight, state));
                        offsetIndex++;

                        // Debug.Log(
                        //     $"{Clips[i].displayName} - definition: {definition}, reveal: {reveal} existInPrev: {existInPrev}, existsInNext: {existsInNext}");
                    }
                }
            }

            foreach (var (key, value) in _group.RevealsById)
            {
                if (revealStateTuplesById.TryGetValue(key, out var stateTuple))
                {
                    var (weight, state) = stateTuple;
                    // Debug.Log($"key: {key}, weight: {weight}, state: {state}");
                    value.SetNormalizedTime(weight, state);
                }
                else
                {
                    value.SetNormalizedTime(0, State.None);
                }
            }
        }
    }
}
