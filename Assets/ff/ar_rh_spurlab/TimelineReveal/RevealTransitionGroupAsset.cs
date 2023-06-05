using System;
using System.Linq;
using ff.utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionGroupAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;

        [SerializeField]
        private GroupDefinition[] _definitions = default;


        public string GetActiveInfoText()
        {
            var actives = string.Empty;
            if (_definitions == null)
                return actives;

            return _definitions.Where(definition => definition.IsActive).Aggregate(actives,
                (current, definition) => $"{current} {definition.Name}\n");
        }

        public void SetActiveAll(bool isActive)
        {
            if (_definitions == null)
                return;

            for (var i = 0; i < _definitions.Length; i++)
            {
                _definitions[i].IsActive = isActive;
            }
        }

        public void SetBinding(RevealTransitionGroup group)
        {
#if UNITY_EDITOR
            if (!group || Application.isPlaying)
            {
                return;
            }

            UpdateDefinitions(group);
#endif
        }

        public void UpdateDefinitions(RevealTransitionGroup group)
        {
#if UNITY_EDITOR
            var newDefinitions = new GroupDefinition[group.Reveals.Length];

            for (var i = 0; i < group.Reveals.Length; i++)
            {
                var reveal = group.Reveals[i];
                if (!reveal)
                    continue;

                newDefinitions[i] = CreateDefinition(reveal);
            }

            _definitions = newDefinitions;
#endif
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<RevealTransitionGroupPlayable>.Create(graph);
            var groupPlayable = playable.GetBehaviour();
            groupPlayable.Initialize(_definitions, this);
            return playable;
        }

#if UNITY_EDITOR
        private GroupDefinition CreateDefinition(Component component)
        {
            var componentName = component.name;
            var id = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString();
            var isActive = SetPreviousOrDefaultSettings(id);
            return new GroupDefinition()
            {
                IsActive = isActive,
                Name = componentName,
                Id = id
            };
        }
#endif

        private bool SetPreviousOrDefaultSettings(string id)
        {
            if (_definitions == null)
            {
                return false;
            }

            return (from definition in _definitions
                    where definition.Id == id
                    select (definition.IsActive))
                .FirstOrDefault();
        }

        [Serializable]
        public struct GroupDefinition
        {
            [ReadOnly]
            public string Name;

            public bool IsActive;

            [HideInInspector]
            public string Id;
        }
    }
}