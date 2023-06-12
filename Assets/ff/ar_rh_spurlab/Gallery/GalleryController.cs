using System.Collections.Generic;
using ff.ar_rh_spurlab._content.Timelines.ImageItem;
using UnityEngine;

namespace ff.ar_rh_spurlab.Gallery
{
    public static class GalleryController
    {
        public static void ShowImage(ImageData image, bool isZoomedIn = false)
        {
            if (!GalleryUiInstance)
                return;

            if (image == null)
            {
                Debug.LogWarning($"GalleryController: ShowImage({image}, {isZoomedIn}) image is null!");
                return;
            }

            GalleryUiInstance.SetImages(ImageDataList);
            GalleryUiInstance.ShowImage(image, isZoomedIn);
        }

        public static void ShowGallery()
        {
            GalleryUiInstance.SetImages(ImageDataList);
            GalleryUiInstance.Show();
        }

        public static void AddImageData(ImageData imageData)
        {
            ImageDataList.Add(imageData);
        }

        public static void RemoveImageData(ImageData imageData)
        {
            ImageDataList.Remove(imageData);
        }

        public static GalleryUi GalleryUiInstance;

        private static readonly List<ImageData> ImageDataList = new();
    }
}