using System;
using ff.ar_rh_spurlab.GrayScaler;
using UnityEngine;


[Serializable]
public enum TriggerType
{
    Front,
    Center,
    Back
}

public class PortalTrigger : MonoBehaviour
{
    public TriggerType TriggerType => _triggerType;

    [SerializeField]
    private TriggerType _triggerType = TriggerType.Center;

    private Portal _portal;

    private void Awake()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer)
        {
            Destroy(renderer);
        }

        _portal = GetComponentInParent<Portal>();
        if (!_portal)
        {
            Debug.LogError("PortalTrigger must be a child of a Portal", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        _portal.HandleTrigger(this, otherCollider, true);
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        _portal.HandleTrigger(this, otherCollider, false);
    }
}
