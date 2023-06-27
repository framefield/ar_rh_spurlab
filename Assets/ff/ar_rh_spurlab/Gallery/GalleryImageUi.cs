using System;
using ff.ar_rh_spurlab._content.Timelines.ImageItem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Gallery
{
    public class GalleryImageUi : MonoBehaviour, IPointerClickHandler
    {
        public event Action<int> OnClick;

        [SerializeField]
        private RawImage _rawImage;

        [SerializeField]
        private ScrollTransition _imageScrollTransition;

        public ImageData Data { get; private set; }
        public float PositionX => _rectTransform.anchoredPosition.x;

        public void Initialize(ImageData imageData, float maxWidth, float maxHeight, float positionX, int index)
        {
            Data = imageData;
            _index = index;
            _maxWidth = maxWidth;
            _rawImage.texture = Data.ImageTexture;

            var imageTransform = _rawImage.GetComponent<RectTransform>();

            _imageWidth = Data.ImageTexture.width;
            float imageHeight = Data.ImageTexture.height;

            if (_imageWidth > maxWidth || imageHeight > maxHeight)
            {
                var widthRatio = maxWidth / _imageWidth;
                var heightRatio = maxHeight / imageHeight;
                var ratio = Mathf.Min(widthRatio, heightRatio);
                _imageWidth *= ratio;
                imageHeight *= ratio;
            }


            imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _imageWidth);
            imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageHeight);

            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.anchoredPosition = new Vector2(positionX, _rectTransform.anchoredPosition.y);
        }

        public void SetRelativeIndex(int relativeIndex)
        {
            var offset = 0f;

            if (relativeIndex is -1 or 1)
            {
                offset = (_imageWidth - _maxWidth) / 2f * relativeIndex;
            }

            _imageScrollTransition.ScrollTo(offset);
        }


        private RectTransform _rectTransform;
        private int _index;
        private float _maxWidth;
        private float _imageWidth;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(_index);
        }
    }
}