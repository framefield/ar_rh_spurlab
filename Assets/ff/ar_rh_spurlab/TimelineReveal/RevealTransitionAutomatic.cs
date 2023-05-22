﻿using System;
using UnityEngine;

namespace ff.common.TimelineReveal
{
    /// <summary>
    /// Control of <see cref="TimelineReveal.RevealTransition"/> without timeline.
    /// </summary>
    public class RevealTransitionAutomatic : MonoBehaviour
    {
        public RevealTransition RevealTransition => _revealTransition;
        
        [SerializeField]
        private RevealTransition _revealTransition = default;

        [SerializeField]
        private float _fadeInDuration = 0.3f;

        [SerializeField]
        private float _fadeOutDuration = 0.3f;

        public AnimationClip GetClip()
        {
            return  _revealTransition.GetClip();
        }
        
        public void SetVisibility(bool isVisible, bool immediate = false)
        {
            if (isVisible)
            {
                FadeIn(immediate);
            }
            else
            {
                FadeOut(immediate);
            }
        }

        public void FadeIn(bool immediate = false)
        {
            if (immediate)
            {
                _revealTransition.UpdateNormalizedTime(1, State.FadeIn);
                return;
            }
            
            _state = State.FadeIn;
            _fadeDuration = _fadeInDuration;
            _normalizedTimePosition = _revealTransition.GetNormalizedPosition();

            // Do nothing if it is already Fade in
            if (Mathf.Approximately(_normalizedTimePosition, 1f))
                _state = State.None;
        }
        
        public void FadeOut(bool immediate = false)
        {
            if (immediate)
            {
                _revealTransition.UpdateNormalizedTime(0, State.FadeOut);
                return;
            }

            _state = State.FadeOut;
            _fadeDuration = _fadeOutDuration;
            _normalizedTimePosition = _revealTransition.GetNormalizedPosition();
            
            // Do nothing if it is already Fade out
            if (Mathf.Approximately(_normalizedTimePosition, 0f))
                _state = State.None;
        }

        public void Stop()
        {
            _state = State.None;
        }

        private void Update()
        {
            if (_state == State.None)
                return;

            var delta = Time.deltaTime / _fadeDuration;
            _normalizedTimePosition += _state == State.FadeIn ? delta : -delta;
            _revealTransition.UpdateNormalizedTime(_normalizedTimePosition, _state);

            if (_normalizedTimePosition < 0 || _normalizedTimePosition > 1)
            {
                _state = State.None;
            }
        }

        private float _fadeDuration;
        private float _normalizedTimePosition;
        private State _state = State.None;

    
    }
}