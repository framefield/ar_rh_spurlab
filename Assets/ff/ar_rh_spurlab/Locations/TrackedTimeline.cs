using ff.ar_rh_spurlab.Locations;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TrackedTimeline : MonoBehaviour, ITrackedLocationContent
{
    private PlayableDirector _director;
    private double _lastPlayTime;

    public void Initialize()
    {
        _director = GetComponent<PlayableDirector>();
    }

    public void SetIsTracked(bool isTracked)
    {
        if (!isTracked)
        {
            _lastPlayTime = _director.time;
            _director.time = 0;
            _director.Evaluate();
            _director.Pause();
        }
        else
        {
            _director.time = _lastPlayTime;
            _director.Evaluate();
            _director.Play();
        }
    }
}
