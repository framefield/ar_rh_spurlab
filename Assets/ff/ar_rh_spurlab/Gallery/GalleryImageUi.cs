using ff.ar_rh_spurlab._content.Timelines.ImageItem;
using ff.ar_rh_spurlab.Localization;
using ff.common.entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ff.ar_rh_spurlab.Gallery
{
    public class GalleryImageUi : AbstractLocalizable
    {
        [SerializeField]
        private RawImage _rawImage;

        [SerializeField]
        private TMP_Text _title;

        public void Initialize(ImageData imageData)
        {
            _imageData = imageData;
            _rawImage.texture = _imageData.ImageTexture;
            OnLocaleChangedHandler(ApplicationLocale.Instance.CurrentLocale);
        }

        protected override void OnLocaleChangedHandler(string locale)
        {
            if (_imageData == null)
                return;

            if (_imageData.Title.TryGetValue(locale, out var title))
            {
                _title.text = title;
            }
        }

        private ImageData _imageData;
    }
}