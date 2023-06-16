using UnityEngine;

namespace ff.ar_rh_spurlab.Map
{
    [RequireComponent(typeof(Camera))]
    public class MapCamera : MonoBehaviour
    {
        [SerializeField]
        private float _panAmount = 0.1f;

        [SerializeField]
        private float _zoomAmount = 1f;

        [SerializeField]
        private Bounds _movementBounds;

        public Camera Camera
        {
            get
            {
                if (!_camera)
                {
                    _camera = GetComponent<Camera>();
                    _originLocalPosition = transform.localPosition;
                }

                return _camera;
            }
        }

        public float TiltAngle => Camera.transform.eulerAngles.x;

        public void SetVisibility(bool isVisible)
        {
            Camera.enabled = isVisible;

            if (!isVisible)
            {
                ResetPosition();
            }
        }

        public void BeginDrag(Vector2 screenPosition)
        {
            _prevTouchScreenPosition = screenPosition;
        }

        public void Drag(Vector2 screenPosition)
        {
            var screenDelta = screenPosition - _prevTouchScreenPosition;
            var localDelta = transform.TransformVector(-screenDelta);
            var newOffset = _localOffset + localDelta * _panAmount;

            SetOffset(newOffset);

            _prevTouchScreenPosition = screenPosition;
        }

        public void EndDrag(Vector2 eventDataPosition)
        {
        }

        public void Zoom(float delta)
        {
            var localDelta = transform.TransformVector(new Vector3(0, 0, -delta));
            var newOffset = _localOffset + localDelta * _zoomAmount;
            SetOffset(newOffset);
        }

        private void ResetPosition()
        {
            SetOffset(Vector3.zero);
        }

        private Camera _camera;


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_movementBounds.center, _movementBounds.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif

        private void SetOffset(Vector3 offset)
        {
            var offsetRelativeToCamera = transform.InverseTransformVector(offset);
            if (!_movementBounds.Contains(offsetRelativeToCamera))
            {
                return;
            }

            _localOffset = offset;
            transform.localPosition = _originLocalPosition + _localOffset;
        }


        private Vector3 _localOffset;
        private Vector3 _originLocalPosition;
        private Vector2 _prevTouchScreenPosition;
    }
}