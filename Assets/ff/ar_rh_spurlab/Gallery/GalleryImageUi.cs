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

        public ImageData Data { get; private set; }
        public float PositionX => _rectTransform.anchoredPosition.x;

        public void Initialize(ImageData imageData, float maxWidth, float maxHeight, float positionX, int index)
        {
            Data = imageData;
            _index = index;
            _rawImage.texture = Data.ImageTexture;
            var imageTransform = _rawImage.GetComponent<RectTransform>();

            float imageWidth = Data.ImageTexture.width;
            float imageHeight = Data.ImageTexture.height;

            if (imageWidth > maxWidth || imageHeight > maxHeight)
            {
                var widthRatio = maxWidth / imageWidth;
                var heightRatio = maxHeight / imageHeight;
                var ratio = Mathf.Min(widthRatio, heightRatio);
                imageWidth *= ratio;
                imageHeight *= ratio;
            }

            imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageWidth);
            imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageHeight);

            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.anchoredPosition = new Vector2(positionX, _rectTransform.anchoredPosition.y);
        }


        private RectTransform _rectTransform;
        private int _index;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(_index);
        }
    }
}