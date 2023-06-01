using ff.common.statemachine;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ff.ar_rh_spurlab.Locations
{
    public class LocationRunning : MonoBehaviour, IActiveInStateContent
    {
        [SerializeField]
        private Canvas _mainMenuCanvas;

        public void Initialize()
        {
        }

        public void Activate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _mainMenuCanvas.gameObject.SetActive(false);
        }

        public void Deactivate(StateMachine stateMachine, State from, State to, ITriggerSource source, Trigger trigger)
        {
            _mainMenuCanvas.gameObject.SetActive(true);
        }
    }
}