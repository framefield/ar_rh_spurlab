using ff.ar_rh_spurlab.Locations;
using TMPro;
using UnityEngine;

namespace ff.ar_rh_spurlab.Map
{
    public class MapLocationUi : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _labelText;

        public void Initialize(LocationData locationData, char label, PlaceableUIContainer placeableUiContainer,
            Vector3 worldPosition)
        {
            var screenPosition = placeableUiContainer.WorldToUiPosition(worldPosition);
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = screenPosition;
            gameObject.name = $"{locationData.Id} - MapLocationUi";
            _labelText.text = label.ToString();
        }
    }
}