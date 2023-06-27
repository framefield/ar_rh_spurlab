using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public static class TimelineClipInfo
    {
        public static (float weight, State state) GetRelativeWeightAndState(TimelineClip clip, Playable playable,
            FrameData info)
        {
            var time = playable.GetTime();
            var weight = info.weight;

            State state;
            if (time >= 0 && time <= clip.easeInDuration)
            {
                state = State.FadeIn;
            }
            else if (time < clip.duration && time >= (clip.duration - clip.easeOutDuration))
            {
                state = State.FadeOut;
            }
            else if (time >= 0 && time <= clip.duration)
            {
                state = State.Idle;
            }
            else
            {
                state = State.None;
            }

            return (weight, state);
        }

        public static (float weight, State state) GetAbsoluteWeightAndState(TimelineClip clip, Playable playable,
            FrameData info)
        {
            var time = playable.GetTime();
            var weight = info.weight;

            State state;
            if (time >= clip.start && time <= clip.start + clip.easeInDuration)
            {
                state = State.FadeIn;
            }
            else if (time < clip.end && time >= (clip.end - clip.easeOutDuration))
            {
                state = State.FadeOut;
            }
            else if (time >= clip.start && time <= clip.end)
            {
                state = State.Idle;
            }
            else
            {
                state = State.None;
            }

            return (weight, state);
        }

        public static (float weight, State state) CalculateAbsoluteWeightAndState(TimelineClip clip, Playable playable,
            double fadeInDuration, double fadeOutDuration, bool existInPrev, bool existsInNext)
        {
            // playable.GetTime() is not resetting when the clip is looping
            var time = playable.GetGraph().GetRootPlayable(0).GetTime();
            var weight = 0d;

            State state;
            if (time >= clip.start + fadeInDuration && existsInNext)
            {
                state = State.Idle;
                weight = 1;
            }
            else if (time < (clip.end - fadeOutDuration) && existInPrev)
            {
                state = State.Idle;
                weight = 1;
            }
            else if (time >= clip.start && time <= clip.start + fadeInDuration)
            {
                state = State.FadeIn;
                weight = (time - clip.start) / fadeInDuration;
            }
            else if (time < clip.end && time >= (clip.end - fadeOutDuration))
            {
                state = State.FadeOut;
                weight = (clip.end - time) / fadeOutDuration;
            }
            else if (time >= clip.start && time <= clip.end)
            {
                state = State.Idle;
                weight = 1;
            }
            else
            {
                state = 0;
            }

            return ((float)weight, state);
        }

        public static (float weight, State state) CalculateSequentialWeightAndState(TimelineClip clip,
            Playable playable, SequentialOptions sequentialOptions, int offsetIndex, double fadeInDuration,
            double fadeOutDuration, bool existInPrev,
            bool existsInNext)
        {
            // playable.GetTime() is not resetting when the clip is looping
            var time = playable.GetGraph().GetRootPlayable(0).GetTime();
            var weight = 0d;

            var startOffsetTime = time - sequentialOptions.SequentialFadeInDelay * offsetIndex;
            var endOffsetTime = time + sequentialOptions.SequentialFadeOutDelay * offsetIndex;

            State state;
            if (startOffsetTime >= clip.start && existsInNext)
            {
                state = State.Idle;
                weight = 1;
            }
            else if (endOffsetTime < (clip.end - fadeOutDuration) && existInPrev)
            {
                state = State.Idle;
                weight = 1;
            }
            else if (startOffsetTime >= clip.start && startOffsetTime <= clip.start + fadeInDuration)
            {
                state = State.FadeIn;
                weight = (startOffsetTime - clip.start) / fadeInDuration;
            }
            else if (endOffsetTime < clip.end && endOffsetTime >= (clip.end - fadeOutDuration))
            {
                state = State.FadeOut;
                weight = (clip.end - endOffsetTime) / fadeOutDuration;
            }
            else if (startOffsetTime >= clip.start && endOffsetTime <= clip.end)
            {
                state = State.Idle;
                weight = 1;
            }
            else
            {
                state = 0;
            }

            return ((float)weight, state);
        }
    }

    public enum State
    {
        None,
        FadeIn,
        Idle,
        FadeOut
    }
}
