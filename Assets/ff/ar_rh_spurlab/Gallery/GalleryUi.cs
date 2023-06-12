using System.Collections.Generic;
using System.Linq;
using ff.ar_rh_spurlab._content.Timelines.ImageItem;
using ff.common.ui;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Gallery
{
    public class GalleryUi : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private float _maxImageWidth = 950;

        [SerializeField]
        private float _maxImageHeight = 550;

        [SerializeField]
        private float _gapBetweenImages = 70;

        [Header("Prefab References")]
        [SerializeField]
        private Hidable _hidable;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private RectTransform _imageContainer;

        [SerializeField]
        private ScrollSlotSnap _scrollSlotSnap;

        [Header("Asset References")]
        [SerializeField]
        private GalleryImageUi _galleryImageUiPrefab;


        public void SetImages(List<ImageData> imageDataList)
        {
            foreach (Transform child in _imageContainer.transform)
            {
                Destroy(child.gameObject);
            }

            _galleryImageUiList.Clear();

            var sumOfWidth = 0f;
            foreach (var imageData in imageDataList)
            {
                var newGalleryImageUi = Instantiate(_galleryImageUiPrefab, _imageContainer);
                newGalleryImageUi.Initialize(imageData, _maxImageWidth, _maxImageHeight, sumOfWidth);
                _galleryImageUiList.Add(newGalleryImageUi);
                sumOfWidth += _maxImageWidth + _gapBetweenImages;
            }
        }

        public void ShowImage(ImageData image, bool isZoomedIn)
        {
            for (int i = 0; i < _galleryImageUiList.Count; i++)
            {
                if (_galleryImageUiList[i].Data == image)
                {
                    ScrollTo(i);
                    break;
                }
            }


            SetVisibility(true);
        }

        public void Show()
        {
            ShowImage(_galleryImageUiList[0].Data, false);
            SetVisibility(true);
        }


        private void Start()
        {
            _closeButton.onClick.AddListener(Hide);

            if (GalleryController.GalleryUiInstance != null)
                Debug.LogError(
                    "GalleryController: Multiple instances of GalleryUi detected! Ignoring all but first.",
                    gameObject);
            else
                GalleryController.GalleryUiInstance = this;
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                GalleryController.ShowGallery();
            }

            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                Next();
            }

            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                Previous();
            }
        }

        private void OnDestroy()
        {
            if (GalleryController.GalleryUiInstance == this)
            {
                GalleryController.GalleryUiInstance = null;
            }
        }

        private void Hide()
        {
            SetVisibility(false);
        }

        private void SetVisibility(bool isVisible)
        {
            if (_hidable)
                _hidable.IsVisible = isVisible;
        }

        private void Previous()
        {
            _currentImageIndex = Mathf.Clamp(_currentImageIndex - 1, 0, _galleryImageUiList.Count - 1);
            ScrollTo(_currentImageIndex);
        }

        private void Next()
        {
            _currentImageIndex = Mathf.Clamp(_currentImageIndex + 1, 0, _galleryImageUiList.Count - 1);
            ScrollTo(_currentImageIndex);
        }

        private void ScrollTo(int imageIndex)
        {
            _currentImageIndex = imageIndex;
            _scrollSlotSnap.ScrollTo(_currentImageIndex);
        }

        private readonly List<GalleryImageUi> _galleryImageUiList = new();
        private int _currentImageIndex = 0;
    }
}