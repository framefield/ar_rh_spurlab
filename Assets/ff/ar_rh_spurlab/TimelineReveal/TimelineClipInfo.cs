using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.common.TimelineReveal
{
    public static class TimelineClipInfo
    {
        public static (float weight, State state) GetRelativeWeightAndState(TimelineClip clip, Playable playable, FrameData info)
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

        public static (float weight, State state) GetAbsoluteWeightAndState(TimelineClip clip, Playable playable, FrameData info)
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
    }
    
    public enum State
    {
        None,
        FadeIn,
        Idle,
        FadeOut
    }
}