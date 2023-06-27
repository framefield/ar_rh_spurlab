using System.Collections.Generic;
using System.Linq;
using ff.ar_rh_spurlab._content.Timelines.ImageItem;
using ff.ar_rh_spurlab.Localization;
using ff.common.entity;
using ff.common.ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Gallery
{
    public class GalleryUi : AbstractLocalizable
    {
        [Header("Settings")]
        [SerializeField]
        private float _maxImageWidth = 950;

        [SerializeField]
        private float _maxImageHeight = 550;

        [SerializeField]
        private float _galleryImageWidth = 1194;

        [SerializeField]
        private float _imageSpacing = 1020;

        [Header("Prefab References")]
        [SerializeField]
        private Hidable _hidable;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private Button _showButton;

        [SerializeField]
        private RectTransform _imageContainer;

        [SerializeField]
        private ScrollSlotSnap _scrollSlotSnap;

        [SerializeField]
        private Hidable _imageTitleHidable;

        [SerializeField]
        private TMP_Text _imageTitleText;

        [Header("Asset References")]
        [SerializeField]
        private GalleryImageUi _galleryImageUiPrefab;

        protected override void OnLocaleChangedHandler(string locale)
        {
            UpdateText();
        }

        public void SetImages(List<ImageData> imageDataList)
        {
            foreach (Transform child in _imageContainer.transform)
            {
                Destroy(child.gameObject);
            }

            _galleryImageUiList.Clear();

            var sumOfWidth = 0f;
            for (var i = 0; i < imageDataList.Count; i++)
            {
                var imageData = imageDataList[i];
                var newGalleryImageUi = Instantiate(_galleryImageUiPrefab, _imageContainer);
                newGalleryImageUi.Initialize(imageData, _maxImageWidth, _maxImageHeight, sumOfWidth, i);
                newGalleryImageUi.OnClick += GalleryImageClickHandler;
                _galleryImageUiList.Add(newGalleryImageUi);
                sumOfWidth += _imageSpacing;
            }


            _scrollSlotSnap.SetSlotPositions(_galleryImageUiList.Select(x => -x.PositionX).ToArray());
            _imageContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                sumOfWidth + _galleryImageWidth - _imageSpacing);

            _activeImageData = null;
            UpdateText();
        }

        private void GalleryImageClickHandler(int index)
        {
            ScrollTo(index);
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
            if (_galleryImageUiList.Count > 0)
            {
                ScrollTo(0);
            }

            SetVisibility(true);
        }


        private void Start()
        {
            _closeButton.onClick.AddListener(Hide);
            _showButton.onClick.AddListener(GalleryController.ShowGallery);

            if (GalleryController.GalleryUiInstance != null)
                Debug.LogError(
                    "GalleryController: Multiple instances of GalleryUi detected! Ignoring all but first.",
                    gameObject);
            else
                GalleryController.GalleryUiInstance = this;

            _scrollSlotSnap.OnScrollingChanged += OnScrollingChangedHandler;
            _scrollSlotSnap.OnSlotChanged += OnSlotChangedHandler;
        }

        private void OnScrollingChangedHandler(bool isScrolling)
        {
            if (!isScrolling)
                return;

            _imageTitleHidable.IsVisible = false;

            ResetRelativeIndices();
        }

        private void OnSlotChangedHandler(int index)
        {
            if (index < 0 || index >= _galleryImageUiList.Count)
            {
                Debug.LogError($"OnSlotChangedHandler({index}): Index out of range!");
                return;
            }


            _activeImageData = _galleryImageUiList[index].Data;
            UpdateText();
            _imageTitleHidable.IsVisible = true;

            for (var i = 0; i < _galleryImageUiList.Count; i++)
            {
                var galleryImageUi = _galleryImageUiList[i];
                var relativeIndex = i - index;
                galleryImageUi.SetRelativeIndex(relativeIndex);
            }
        }

        private void UpdateText()
        {
            if (_activeImageData == null)
            {
                _imageTitleText.text = string.Empty;
                return;
            }


            _activeImageData.Title.TryGetValue(ApplicationLocale.Instance.CurrentLocale, out var title);

            _imageTitleText.text = $"{title} {_activeImageData.Year} / {_activeImageData.Copyright}";
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
            ResetRelativeIndices();
        }

        private void ResetRelativeIndices()
        {
            foreach (var galleryImageUi in _galleryImageUiList)
            {
                galleryImageUi.SetRelativeIndex(0);
            }
        }

        private readonly List<GalleryImageUi> _galleryImageUiList = new();
        private int _currentImageIndex = 0;
        private ImageData _activeImageData;
    }
}