using UnityEngine;

namespace ff.ar_rh_spurlab.Gallery
{
    public class ScrollTransition : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _rectTransform;

        [SerializeField]
        private float _duration = 0.25f;

        [SerializeField]
        private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


        public void ScrollTo(float targetPositionX)
        {
            _startPositionX = _rectTransform.anchoredPosition.x;
            _targetPositionX = targetPositionX;
            _time = 0;
            enabled = true;
        }

        public void Update()
        {
            _time += Time.deltaTime;
            enabled = _time < _duration;

            if (!enabled)
            {
                _time = _duration;
            }

            var t = _time / _duration;
            var curveValue = _transitionCurve.Evaluate(t);
            var positionX = Mathf.Lerp(_startPositionX, _targetPositionX, curveValue);
            _rectTransform.anchoredPosition = new Vector2(positionX, _rectTransform.anchoredPosition.y);
        }

        public void Start()
        {
            enabled = false;
        }

        private float _targetPositionX;
        private float _time;
        private float _startPositionX;
    }
}