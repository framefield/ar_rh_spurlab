using System;
using UnityEngine;

namespace ff.ar_rh_spurlab._content.Timelines.ImageItem
{
    [RequireComponent(typeof(Collider))]
    public class ImageItemButton : MonoBehaviour
    {
        public event Action OnClicked;

        private void OnMouseUpAsButton()
        {
            OnClicked?.Invoke();
        }
    }
}