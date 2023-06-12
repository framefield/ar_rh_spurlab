using ff.ar_rh_spurlab._content.Timelines.ImageItem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Gallery
{
    public class GalleryImageUi : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private RawImage _rawImage;

        [SerializeField]
        private LayoutElement _layoutElement;

        public ImageData Data { get; private set; }
        public float PositionX => _rectTransform.anchoredPosition.x;

        public void Initialize(ImageData imageData, float maxWidth, float maxHeight, float positionX)
        {
            Data = imageData;
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
            _layoutElement.minWidth = imageWidth;
            _layoutElement.minHeight = imageHeight;

            _rectTransform = GetComponent<RectTransform>();
            // _rectTransform.anchoredPosition = new Vector2(positionX, _rectTransform.anchoredPosition.y);
        }


        private RectTransform _rectTransform;

        public void OnPointerClick(PointerEventData eventData)
        {
        }
    }
}