using System;
using System.Collections.Generic;
using System.Linq;
using ff.common.TimelineReveal;
using ff.utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace ff.ar_rh_spurlab.TimelineReveal
{
    [Serializable]
    public struct SequentialOptions
    {
        public bool PlaySequentially;
        public float SequentialDelay;
    }

    public class RevealTransitionGroupAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;

        [SerializeField]
        private GroupDefinition[] _definitions;

        [SerializeField]
        private SequentialOptions _sequentialOptions = new SequentialOptions
        {
            SequentialDelay = 0.2f
        };

        public GroupDefinition[] Definitions => _definitions;
        public SequentialOptions SequentialOptions => _sequentialOptions;

        [CanBeNull] public Dictionary<string, GroupDefinitionWithReveal> ActiveResolvedDefinitions { get; private set; }


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<RevealTransitionGroupPlayable>.Create(graph);
            var groupPlayable = playable.GetBehaviour();
            groupPlayable.Initialize(this);
            return playable;
        }

        public void SetBinding(RevealTransitionGroup group)
        {
            _group = group;
#if UNITY_EDITOR
            if (group && !Application.isPlaying)
            {
                UpdateDefinitions(group);
            }
#endif
            if (Application.isPlaying)
            {
                RefreshCache();
            }
        }

        public string GetActiveInfoText()
        {
            var actives = string.Empty;
            if (_definitions == null)
            {
                return actives;
            }

            return _definitions.Where(definition => definition.IsActive).Aggregate(actives,
                (current, definition) => $"{current} {definition.Name}\n");
        }

        public void SetActiveAll(bool isActive)
        {
            if (_definitions == null)
            {
                return;
            }

            for (var i = 0; i < _definitions.Length; i++)
            {
                _definitions[i].IsActive = isActive;
            }
        }

        public void UpdateDefinitions()
        {
            if (_group)
            {
                UpdateDefinitions(_group);
            }
        }

        public void UpdateDefinitions(RevealTransitionGroup group)
        {
            var existingDefinitions = new Dictionary<string, (int index, GroupDefinition definition)>();
            if (_definitions != null)
            {
                for (var index = 0; index < _definitions.Length; index++)
                {
                    var definition = _definitions[index];
                    existingDefinitions.TryAdd(definition.Id, (index, definition));
                }
            }

            var newDefinitions = new GroupDefinition[group.Reveals.Length];
            for (var i = 0; i < group.Reveals.Length; i++)
            {
                var reveal = group.Reveals[i];
                if (!reveal)
                {
                    continue;
                }

                var id = reveal.name;
                if (existingDefinitions.TryGetValue(id, out var existingDefinition))
                {
                    newDefinitions[i] = existingDefinition.definition;
                }
                else
                {
                    newDefinitions[i] = new GroupDefinition()
                    {
                        Id = reveal.name,
                        Name = reveal.name,
                        IsActive = false,
                    };
                    existingDefinitions.Add(id, (int.MaxValue, newDefinitions[i]));
                }
            }

            Array.Sort(newDefinitions,
                (a, b) => existingDefinitions[a.Id].index.CompareTo(existingDefinitions[b.Id].index));
            _definitions = newDefinitions;
            RefreshCache();
        }

        private void RefreshCache()
        {
            if (_group && _definitions != null)
            {
                var revealById = _group.RevealsById;
                ActiveResolvedDefinitions = _definitions
                    .Where(definition => definition.IsActive && revealById.ContainsKey(definition.Id))
                    .Select((d, i) => new GroupDefinitionWithReveal
                    {
                        Definition = d,
                        Reveal = revealById[d.Id],
                        Index = i
                    })
                    .ToDictionary(d => d.Definition.Id);
            }
            else
            {
                ActiveResolvedDefinitions = null;
            }
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

        public struct GroupDefinitionWithReveal
        {
            public GroupDefinition Definition;
            public RevealTransitionAutomatic Reveal;
            public int Index;
        }

        private RevealTransitionGroup _group;
    }
}