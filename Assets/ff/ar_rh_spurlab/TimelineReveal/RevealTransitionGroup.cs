using System;
using System.Collections.Generic;
using ff.common.TimelineReveal;
using ff.utils;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ff.ar_rh_spurlab.TimelineReveal
{
    public class RevealTransitionGroup : MonoBehaviour
    {
        public RevealTransitionAutomatic[] Reveals
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    _reveals = GetComponentsInChildren<RevealTransitionAutomatic>();
                    var names = new HashSet<string>();
                    foreach (var reveal in _reveals)
                    {
                        if (names.Contains(reveal.name))
                        {
                            Debug.LogError(
                                $"RevealTransitionGroup {name} has multiple children with the same name '{reveal.name}'");
                        }

                        names.Add(reveal.name);
                    }
                }
#endif
                return _reveals;
            }
        }

        public void DeactivateAll()
        {
            var setImmediate = !Application.isPlaying;

            foreach (var reveal in Reveals)
            {
                if (reveal)
                    reveal.SetVisibility(false, setImmediate);
            }
        }

        public void SetGroupDefinition(RevealTransitionGroupAsset.GroupDefinition[] definitions)
        {
            var setImmediate = !Application.isPlaying;

            var i = 0;
            for (; i < Reveals.Length; i++)
            {
                if (!Reveals[i]) continue;
                Reveals[i].SetVisibility(definitions[i].IsActive, setImmediate);
            }
        }

        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            foreach (var reveal in Reveals)
            {
                if (reveal)
                    driver.AddFromClip(reveal.gameObject, reveal.GetClip());
            }
        }

        private bool _isInitialized;

        [ReadOnly]
        [SerializeField]
        private RevealTransitionAutomatic[] _reveals;
    }
}