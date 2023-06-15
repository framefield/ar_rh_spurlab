using System;
using ff.ar_rh_spurlab.Gallery;
using ff.common.entity;
using UnityEngine;

namespace ff.ar_rh_spurlab._content.Timelines.ImageItem
{
    public class ImageItem : MonoBehaviour
    {
        [SerializeField]
        private LocalizedString _title;

        [SerializeField]
        private string _year;

        [SerializeField]
        private string _copyright;

        [SerializeField]
        private Renderer _imageRenderer;

        [SerializeField]
        private ImageItemButton _imageItemButton;

        private void Start()
        {
            _imageItemButton.OnClicked += OnClickedHandler;
        }

        private void OnEnable()
        {
            GalleryController.AddImageData(GetImageData());
        }

        private void OnDisable()
        {
            GalleryController.RemoveImageData(GetImageData());
        }

        private void OnClickedHandler()
        {
            GalleryController.ShowImage(GetImageData());
        }

        private ImageData GetImageData()
        {
            return _imageData ??= new ImageData
            {
                Title = _title,
                Year = _year,
                Copyright = _copyright,
                ImageTexture = _imageRenderer.material.mainTexture
            };
        }

        private ImageData _imageData;
    }
}