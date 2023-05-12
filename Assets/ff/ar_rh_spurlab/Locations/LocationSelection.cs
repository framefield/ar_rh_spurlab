using ff.ar_rh_spurlab.Locations;
using ff.common.statemachine;
using UnityEngine;

public class LocationSelection : MonoBehaviour, IActiveInStateContent
{
    public void Initialize()
    {
    }

    public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
    {
        var locationController = stateMachine.GetComponent<LocationController>();

        if (locationController)
        {
            locationController.SetLocation("FF Office");
        }

        stateMachine.Continue();
    }

    public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
    {
    }
}