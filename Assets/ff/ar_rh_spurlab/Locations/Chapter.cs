using System;
using ff.common.entity;
using UnityEngine.Playables;

namespace ff.ar_rh_spurlab.Locations
{
    [Serializable]
    public class Chapter
    {
        public LocalizedString Title;
        public PlayableDirector Timeline;

        public ReactiveProperty<bool> IsActive = new();
        public ReactiveProperty<bool> IsVisited = new();
        public ReactiveProperty<bool> IsNext = new();
    }
}