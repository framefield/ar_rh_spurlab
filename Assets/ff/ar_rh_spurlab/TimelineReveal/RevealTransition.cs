using System;
using System.Collections.Generic;
using ff.ar_rh_spurlab.TimelineReveal;
using ff.common.animation;
using UnityEngine;

namespace ff.common.TimelineReveal
{
    [RequireComponent(typeof(Animator))]
    public class RevealTransition : MonoBehaviour
    {
        public State State { get; private set; }

        [SerializeField]
        private string _layerName = "visible";

        [SerializeField]
        private bool _playIdle = false;

        [SerializeField]
        private string _idleLayer = "idle";

        [SerializeField]
        private bool _disableExpensiveChildComponentsWhenHidden = true;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            var animator = GetComponent<Animator>();
            if (!animator.isInitialized)
            {
                animator.Rebind();
            }

            _controller = new TimedAnimationController(animator, _layerName);
            _clipDuration = _controller.ClipLength;
            var clipIncludesFadeOut = _clipDuration > 1.2;

            _fadeInRange = new FadeRange()
            {
                StartNormalized = 0f,
                EndNormalized = clipIncludesFadeOut ? 0.5f : 1f
            };

            _fadeOutRange = new FadeRange()
            {
                StartNormalized = clipIncludesFadeOut ? 1f : 0f,
                EndNormalized = clipIncludesFadeOut ? 0.5f : 1f
            };

            if (_playIdle)
            {
                _idleController = new TimedAnimationController(animator, _idleLayer);
            }

            _initialized = true;
            _isRevealed = false;

            if (_disableExpensiveChildComponentsWhenHidden)
            {
                _expensiveComponents = new List<Component>();

                _expensiveComponents.AddRange(GetComponentsInChildren<Renderer>());
                _expensiveComponents.AddRange(GetComponentsInChildren<Canvas>());

                _expensiveComponents.RemoveAll(c => c switch
                {
                    Behaviour b => !b.enabled,
                    Renderer r => !r.enabled,
                    _ => false
                });

                UpdateRenderers(false, true);
            }
        }

        private void Update()
        {
            if (_isRevealed && _playIdle && _idleController != null)
            {
                _idleController.AdvanceTimePosition(Time.deltaTime);
            }
        }

        public AnimationClip GetClip()
        {
            Initialize();
            return _controller.PlayedClip;
        }

        public void UpdateNormalizedTime(float weight, State state)
        {
            Initialize();
            float t;
            State = state;

            switch (state)
            {
                case State.None:
                    t = _fadeInRange.StartNormalized;
                    break;
                case State.FadeIn:
                    t = Mathf.Lerp(_fadeInRange.StartNormalized, _fadeInRange.EndNormalized, weight);
                    break;
                case State.FadeOut:
                    t = Mathf.Lerp(_fadeOutRange.StartNormalized, _fadeOutRange.EndNormalized, weight);
                    break;
                case State.Idle:
                    t = _fadeInRange.EndNormalized;
                    break;
                default:
                    return;
            }

            if (_disableExpensiveChildComponentsWhenHidden && _expensiveComponents != null)
            {
                UpdateRenderers(t > 0);
            }

            _controller.SetNormalizedPosition(t);
        }

        private void UpdateRenderers(bool isRevealed, bool force = false)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_isRevealed != isRevealed || force)
            {
                _isRevealed = isRevealed;
                foreach (var expensiveComponent in _expensiveComponents)
                {
                    _ = expensiveComponent switch
                    {
                        Behaviour b => b.enabled = isRevealed,
                        Renderer r => r.enabled = isRevealed,
                        _ => throw new ArgumentOutOfRangeException(nameof(expensiveComponent))
                    };
                }
            }
        }

        private void OnValidate()
        {
            _initialized = false;
        }

        private struct FadeRange
        {
            public float StartNormalized;
            public float EndNormalized;
        }

        private bool _initialized;
        private TimedAnimationController _controller;
        private float _clipDuration;
        private FadeRange _fadeInRange;
        private FadeRange _fadeOutRange;

        private TimedAnimationController _idleController;
        private bool _isRevealed;
        private List<Component> _expensiveComponents;
    }
}