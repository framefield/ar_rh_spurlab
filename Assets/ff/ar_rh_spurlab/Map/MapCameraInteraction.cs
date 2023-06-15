using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Map
{
    public class MapCameraInteraction : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public void SetMapCamera(MapCamera mapCamera)
        {
            _mapCamera = mapCamera;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_mapCamera)
            {
                _mapCamera.BeginDrag(eventData.position);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_mapCamera)
            {
                _mapCamera.Drag(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_mapCamera)
            {
                _mapCamera.EndDrag(eventData.position);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_mapCamera)
            {
                _mapCamera.Zoom(eventData.scrollDelta.y * 10);
            }
        }

        private void Update()
        {
            if (Input.touchCount == 2)
            {
                var newDelta = (Input.GetTouch(0).position - Input.GetTouch(1).position).magnitude;

                if (_isPinching)
                {
                    var delta = newDelta - _prevDelta;
                    _mapCamera.Zoom(-delta);
                }

                _prevDelta = newDelta;
                _isPinching = true;
            }
            else
            {
                _isPinching = false;
            }
        }

        private bool _isPinching;
        private MapCamera _mapCamera;
        private float _prevDelta;
    }
}