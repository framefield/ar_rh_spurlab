using System.Collections.Generic;
using ff.ar_rh_spurlab._content.Timelines.ImageItem;
using ff.common.ui;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Gallery
{
    public class GalleryUi : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private Hidable _hidable;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private Transform _imageContainer;

        [Header("Asset References")]
        [SerializeField]
        private GalleryImageUi _galleryImageUiPrefab;

        public void SetImages(List<ImageData> imageDataList)
        {
            foreach (Transform child in _imageContainer.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var imageData in imageDataList)
            {
                var newGalleryImageUi = Instantiate(_galleryImageUiPrefab, _imageContainer);
                newGalleryImageUi.Initialize(imageData);
            }
        }

        public void ShowImage(ImageData image, bool isZoomedIn)
        {
            Debug.Log($"Gallery Ui: ShowImage({image.Title}, {isZoomedIn})");
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

        private void OnDestroy()
        {
            if (GalleryController.GalleryUiInstance == this)
            {
                GalleryController.GalleryUiInstance = null;
            }
        }

        private void Hide()
        {
            Debug.Log($"GalleryController: Hide");
            SetVisibility(false);
        }

        private void SetVisibility(bool isVisible)
        {
            if (_hidable)
                _hidable.IsVisible = isVisible;
        }

        private void Previous()
        {
        }

        private void Next()
        {
        }
    }
}