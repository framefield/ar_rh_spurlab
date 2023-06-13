using System;
using ff.ar_rh_spurlab.Locations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Gallery
{
    public class ScrollSlotSnap : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public event Action<int> OnSlotChanged;
        public event Action<bool> OnScrollingChanged;

        [SerializeField]
        private float _snapVelocityThreshold = 1f;


        [Header("Prefab References")]
        [SerializeField]
        private ScrollTransition _scrollTransition;

        [SerializeField]
        private ScrollRect _scrollRect;


        private void Start()
        {
            _isUserScrolling.OnValueChanged += OnScrollingChanged;

            _scrollTransition.OnArrived += OnArrivedHandler;
        }

        private void OnArrivedHandler()
        {
            OnSlotChanged?.Invoke(_targetIndex);
        }

        public void SetSlotPositions(float[] slotPositions)
        {
            _slotPositions = slotPositions;
        }

        public void ScrollTo(int index)
        {
            if (index < 0 || index >= _slotPositions.Length)
            {
                Debug.LogError($"ScrollTo({index}) is out of bounds!", this);
                return;
            }

            _hasFoundSnapPosition = true;
            _targetIndex = index;
            _scrollTransition.ScrollTo(_slotPositions[index]);
        }

        private void Update()
        {
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

            if (_slotPositions == null)
            {
                return;
            }

            _scrollRect.velocity = Vector2.zero;

            var currentPosition = _scrollRect.content.anchoredPosition.x;

            var closestDistance = Mathf.Infinity;
            var closestIndex = -1;

            for (var i = 0; i < _slotPositions.Length; i++)
            {
                var distance = Mathf.Abs(currentPosition - _slotPositions[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            if (closestIndex == -1)
                return;

            _hasFoundSnapPosition = true;

            ScrollTo(closestIndex);
        }

        private bool _hasFoundSnapPosition;
        private readonly ReactiveProperty<bool> _isUserScrolling = new();
        private float[] _slotPositions;
        private int _targetIndex;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isUserScrolling.Value = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isUserScrolling.Value = false;
        }
    }
}