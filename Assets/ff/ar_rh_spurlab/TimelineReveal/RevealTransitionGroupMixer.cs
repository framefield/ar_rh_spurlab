using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionGroupMixer : PlayableBehaviour
    {
        public TimelineClip[] Clips;
        private RevealTransitionGroup _group;

        private class RevealFrameInfo
        {
            public float Weight;
            public State State;
            public int OffsetIndex;

            public override string ToString()
            {
                return $"Weight: {Weight:F2}, State: {State}, OffsetIndex: {OffsetIndex}";
            }
        }

        public static float Remap(float aMin, float aMax, float bMin, float bMax, float value)
        {
            var t = Mathf.InverseLerp(aMin, aMax, value);
            return Mathf.Lerp(bMin, bMax, t);
        }

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

            var revealStateTuplesById = new Dictionary<string, RevealFrameInfo>();
            var count = playable.GetInputCount();

            var fadeInCount = 0;
            var fadeOutCount = 0;

            var fadeInSequential = new SequentialOptions();
            var fadeOutSequential = new SequentialOptions();

            for (var i = 0; i < count; ++i)
            {
                var inputWeight = playable.GetInputWeight(i);
                if (inputWeight > 0)
                {
                    //Debug.Log($"Clip {i} {Clips[i].displayName} weight: {inputWeight}");
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
                    if (nextClip != null && nextClip.start > currentClip.start && currentClip.end < nextClip.end
                        && (currentClip.end > nextClip.start || Math.Abs(currentClip.end - nextClip.start) < 0.001d))
                    {
                        var nextInput = (ScriptPlayable<RevealTransitionGroupPlayable>)playable.GetInput(i + 1);
                        var nextTransitionGroupPlayable = nextInput.GetBehaviour();
                        nextDefinitionsById = nextTransitionGroupPlayable.GetActiveDefinitions();
                        //Debug.Log("nextClip: " + nextClip.displayName);
                    }

                    var prevClip = i > 0 ? Clips[i - 1] : null;
                    Dictionary<string, RevealTransitionGroupAsset.GroupDefinitionWithReveal> prevDefinitionsById =
                        null;
                    if (prevClip != null && prevClip.start < currentClip.start && prevClip.end < currentClip.end &&
                        (currentClip.start < prevClip.end || Math.Abs(currentClip.start - prevClip.end) < 0.001d))
                    {
                        var prevInput = (ScriptPlayable<RevealTransitionGroupPlayable>)playable.GetInput(i - 1);
                        var prevTransitionGroupPlayable = prevInput.GetBehaviour();
                        prevDefinitionsById = prevTransitionGroupPlayable.GetActiveDefinitions();
                        //Debug.Log("prevClip: " + prevClip.displayName);
                    }

                    var fadeInOffsetIndex = 0;
                    var fadeOutOffsetIndex = 0;
                    var (weight, state) = TimelineClipInfo.GetAbsoluteWeightAndState(currentClip, playable, info);

                    foreach (var (id, value) in definitionAndRevealById.OrderBy(d => d.Value.Index))
                    {
                        var existInPrev = prevDefinitionsById != null && prevDefinitionsById.ContainsKey(id);
                        var existsInNext = nextDefinitionsById != null && nextDefinitionsById.ContainsKey(id);

                        var instanceState = existInPrev && state == State.FadeIn ? State.Idle : state;
                        instanceState = existsInNext && instanceState == State.FadeOut ? State.Idle : instanceState;

                        var offsetIndex = instanceState switch
                        {
                            State.FadeIn => fadeInOffsetIndex,
                            State.FadeOut => fadeOutOffsetIndex,
                            _ => -1
                        };

                        // Debug.Log($"XXX {Clips[i].displayName} {id} {instanceState} {inputWeight}");
                        revealStateTuplesById.TryAdd(id, new RevealFrameInfo
                        {
                            Weight = inputWeight,
                            State = instanceState,
                            OffsetIndex = offsetIndex
                        });

                        if (instanceState is State.FadeIn)
                        {
                            fadeInOffsetIndex++;
                            fadeInCount++;
                        }
                        else if (instanceState is State.FadeOut)
                        {
                            fadeOutOffsetIndex++;
                            fadeOutCount++;
                        }

                        //Debug.Log(
                        //    $"{Clips[i].displayName} - definition: {definition}, reveal: {reveal} existInPrev: {existInPrev}, existsInNext: {existsInNext}");
                    }

                    if (fadeOutOffsetIndex > 0 && sequentialOptions.PlaySequentially)
                    {
                        fadeOutSequential = sequentialOptions;
                    }

                    if (fadeInOffsetIndex > 0 && sequentialOptions.PlaySequentially)
                    {
                        fadeInSequential = sequentialOptions;
                    }
                }
            }

            // spread sequential animations
            if (fadeInSequential.PlaySequentially || fadeOutSequential.PlaySequentially)
            {
                foreach (var (key, revealFrameInfo) in revealStateTuplesById)
                {
                    if (revealFrameInfo.State is not (State.FadeIn or State.FadeOut))
                    {
                        continue;
                    }

                    var entryCount = revealFrameInfo.State switch
                    {
                        State.FadeIn => fadeInCount,
                        State.FadeOut => fadeOutCount,
                        _ => -1
                    };

                    var spacing = revealFrameInfo.State switch
                    {
                        State.FadeIn => fadeInSequential.SequentialFadeInSpacing,
                        State.FadeOut => fadeOutSequential.SequentialFadeOutSpacing,
                        _ => 0f
                    };


                    var individualDuration = 1f / (entryCount + (entryCount - 1) * spacing);
                    var spacingDuration = spacing * individualDuration;
                    var individualStart = revealFrameInfo.OffsetIndex * (individualDuration + spacingDuration);

                    var remapped = Remap(individualStart,
                        (individualStart + individualDuration),
                        0, 1,
                        revealFrameInfo.Weight);

                    // Debug.Log(
                    //    $"key: {key} {revealFrameInfo} spacing:{spacing} start:{individualStart:F2} duration:{individualDuration:F2} spacingDuration:{spacingDuration:F2} remapped:{remapped:F2}");

                    revealFrameInfo.Weight = remapped;
                }
            }


            foreach (var (key, value) in _group.RevealsById)
            {
                if (revealStateTuplesById.TryGetValue(key, out var revealFrameInfo))
                {
                    // Debug.Log($"key: {key} {revealFrameInfo} fi:{fadeInCount} fo:{fadeOutCount}");
                    value.SetNormalizedTime(revealFrameInfo.Weight, revealFrameInfo.State);
                }
                else
                {
                    value.SetNormalizedTime(0, State.None);
                }
            }
        }
    }
}
