using System;
using UnityEngine;

namespace ff.ar_rh_spurlab.Gallery
{
    public class ScrollTransition : MonoBehaviour
    {
        public event Action OnArrived;

        [SerializeField]
        private RectTransform _rectTransform;

        [SerializeField]
        private float _duration = 0.25f;

        [SerializeField]
        private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public void ScrollTo(float targetPositionX, int index = 0)
        {
            _startPositionX = _rectTransform.anchoredPosition.x;
            _targetPositionX = targetPositionX;
            _time = 0;
            enabled = true;
        }

        public void Update()
        {
            _time += Time.deltaTime;
            var isArrived = _time >= _duration;

            if (isArrived)
            {
                _time = _duration;
            }

            var t = _time / _duration;
            var curveValue = _transitionCurve.Evaluate(t);
            var positionX = Mathf.Lerp(_startPositionX, _targetPositionX, curveValue);
            _rectTransform.anchoredPosition = new Vector2(positionX, _rectTransform.anchoredPosition.y);

            if (!isArrived)
                return;

            enabled = false;
            OnArrived?.Invoke();
        }

        private float _targetPositionX;
        private float _time;
        private float _startPositionX;
    }
}