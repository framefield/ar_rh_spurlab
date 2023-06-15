using System.Linq;
using ff.ar_rh_spurlab.GrayScaler;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class SimpleTrackedLocationContent : MonoBehaviour, ITrackedLocationContent
    {
        public void Initialize()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _colliders = GetComponentsInChildren<Collider>();
            _behaviours = GetComponentsInChildren<Behaviour>()
                .Where(b => b is Portal or Canvas or GrayScaleScenePointOfInterest).ToArray();

            _rendererStateTuples = new (Renderer renderer, bool enabled)[_renderers.Length];
            _colliderStateTuples = new (Collider renderer, bool enabled)[_colliders.Length];
            _behavioursStateTuples = new (Behaviour renderer, bool enabled)[_behaviours.Length];

            _isTracked = false;
            HideAllChildren();
        }

        public void SetIsTracked(bool isTracked)
        {
            if (_isTracked == isTracked)
                return;

            _isTracked = isTracked;

            if (_isTracked)
            {
                ShowAllChildren();
            }
            else
            {
                HideAllChildren();
            }
        }

        private void HideAllChildren()
        {
            for (var index = 0; index < _renderers.Length; index++)
            {
                var r = _renderers[index];
                if (r)
                {
                    _rendererStateTuples[index] = (r, r.enabled);
                    r.enabled = false;
                }
                else
                {
                    _rendererStateTuples[index] = (null, false);
                }
            }

            for (var index = 0; index < _colliders.Length; index++)
            {
                var c = _colliders[index];
                if (c)
                {
                    _colliderStateTuples[index] = (c, c.enabled);
                    c.enabled = false;
                }
                else
                {
                    _colliderStateTuples[index] = (null, false);
                }
            }

            for (var index = 0; index < _behaviours.Length; index++)
            {
                var b = _behaviours[index];
                if (b)
                {
                    _behavioursStateTuples[index] = (b, b.enabled);
                    b.enabled = false;
                }
                else
                {
                    _behavioursStateTuples[index] = (null, false);
                }
            }
        }

        private void ShowAllChildren()
        {
            foreach (var (r, rEnabled) in _rendererStateTuples)
            {
                if (r)
                {
                    r.enabled = rEnabled;
                }
            }

            foreach (var (c, cEnabled) in _colliderStateTuples)
            {
                if (c)
                {
                    c.enabled = cEnabled;
                }
            }


            foreach (var (b, cEnabled) in _behavioursStateTuples)
            {
                if (b)
                {
                    b.enabled = cEnabled;
                }
            }
        }

        private bool _isTracked;

        private (Renderer renderer, bool enabled)[] _rendererStateTuples;
        private (Collider collider, bool enabled)[] _colliderStateTuples;
        private (Behaviour portal, bool enabled)[] _behavioursStateTuples;

        private Renderer[] _renderers;
        private Collider[] _colliders;
        private Behaviour[] _behaviours;
    }
}