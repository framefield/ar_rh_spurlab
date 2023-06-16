using System;
using ff.common.entity;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace ff.ar_rh_spurlab.Locations
{
    [Serializable]
    public class Chapter : IEquatable<Chapter>
    {
        public LocalizedString Title;
        public PlayableDirector Timeline;
        public bool IsWelcomeChapter = false;

        public ReactiveProperty<bool> IsActive = new();
        public ReactiveProperty<bool> IsVisited = new();
        public ReactiveProperty<bool> IsNext = new();

        public bool Equals(Chapter other)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Chapter)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}