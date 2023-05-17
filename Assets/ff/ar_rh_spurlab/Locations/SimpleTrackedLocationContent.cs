using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ff.ar_rh_spurlab.Locations
{
    public class SimpleTrackedLocationContent : MonoBehaviour, ITrackedLocationContent
    {
        public void Initialize()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _colliders = GetComponentsInChildren<Collider>();
            _canvas = GetComponentsInChildren<Canvas>();

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
            _rendererStateTuples = _renderers.Select(r => (r, r.enabled)).ToArray();
            _colliderStateTuples = _colliders.Select(c => (c, c.enabled)).ToArray();
            _canvasStateTuples = _canvas.Select(c => (c, c.enabled)).ToArray();

            foreach (var r in _renderers)
            {
                r.enabled = false;
            }

            foreach (var c in _colliders)
            {
                c.enabled = false;
            }

            foreach (var c in _canvas)
            {
                c.enabled = false;
            }
        }

        private void ShowAllChildren()
        {
            foreach (var (r, rEnabled) in _rendererStateTuples)
            {
                r.enabled = rEnabled;
            }

            foreach (var (c, cEnabled) in _colliderStateTuples)
            {
                c.enabled = cEnabled;
            }

            foreach (var (c, cEnabled) in _canvasStateTuples)
            {
                c.enabled = cEnabled;
            }
        }

        private bool _isTracked;

        private (Renderer renderer, bool enabled)[] _rendererStateTuples;
        private (Collider collider, bool enabled)[] _colliderStateTuples;
        private (Canvas canvas, bool enabled)[] _canvasStateTuples;

        private Renderer[] _renderers;
        private Collider[] _colliders;
        private Canvas[] _canvas;
    }
}