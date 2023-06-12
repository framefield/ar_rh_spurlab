using System;
using System.Linq;
using ff.ar_rh_spurlab.Locations;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Gallery
{
    public class ScrollSlotSnap : MonoBehaviour
    {
        public event Action<int> OnSlotChanged;
        public event Action<bool> OnScrollingChanged;

        [SerializeField]
        private float _snapVelocityThreshold = 1f;

        [SerializeField]
        private float _snapTolerance = 10f;


        [Header("Prefab References")]
        [SerializeField]
        private ScrollTransition _scrollTransition;

        [SerializeField]
        private ScrollRect _scrollRect;


        private void Start()
        {
            _isUserScrolling.OnValueChanged += OnScrollingChanged;
        }

        public void ScrollTo(int index)
        {
            // todo slot positions are 0 when this is called just after SetImages.
            var slotPositions = CollectSlotPositions();
            _scrollTransition.ScrollTo(slotPositions[index]);
            _hasFoundSnapPosition = true;
        }

        private void Update()
        {
            _isUserScrolling.Value = Mathf.Abs(_scrollRect.velocity.x) > _snapVelocityThreshold;

            if (_isUserScrolling.Value)
            {
                _scrollTransition.enabled = false;
                _hasFoundSnapPosition = false;
                return;
            }

            if (_hasFoundSnapPosition)
            {
                return;
            }

            _scrollRect.velocity = Vector2.zero;

            var currentPosition = _scrollRect.content.anchoredPosition.x;

            var closestDistance = Mathf.Infinity;
            var closestIndex = -1;

            var slotPositions = CollectSlotPositions();
            for (var i = 0; i < slotPositions.Length; i++)
            {
                var distance = Mathf.Abs(currentPosition - slotPositions[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            if (closestIndex == -1)
                return;

            _hasFoundSnapPosition = true;

            if (Mathf.Abs(slotPositions[closestIndex] - currentPosition) < _snapTolerance)
            {
                return;
            }


            _scrollTransition.ScrollTo(slotPositions[closestIndex]);
            OnSlotChanged?.Invoke(closestIndex);
        }

        private float[] CollectSlotPositions()
        {
            // todo cache it.
            // needs to be set dirty when list changes.
            // cannot collect in the beginning because layout is updated late.
            return _scrollRect.content.GetComponentsInChildren<RectTransform>()
                .Select(t => -t.anchoredPosition.x).ToArray();
        }

        private bool _hasFoundSnapPosition;
        private readonly ReactiveProperty<bool> _isUserScrolling = new();
    }
}