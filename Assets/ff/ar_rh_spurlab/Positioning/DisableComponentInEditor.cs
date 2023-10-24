using UnityEngine;

namespace ff.ar_rh_spurlab.Positioning
{
    public class DisableComponentInEditor : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _component = default;

        private void Awake()
        {
#if UNITY_EDITOR
            _component.enabled = false;
#endif
        }

    }
}