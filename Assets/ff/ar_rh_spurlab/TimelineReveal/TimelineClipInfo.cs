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
            if (time >= clip.start && time <= clip.start + fadeInDuration && !existInPrev)
            {
                state = State.FadeIn;
                weight = (time - clip.start) / fadeInDuration;
            }
            else if (time < clip.end && time >= (clip.end - fadeOutDuration) && !existsInNext)
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
    }

    public enum State
    {
        None,
        FadeIn,
        Idle,
        FadeOut
    }
}
