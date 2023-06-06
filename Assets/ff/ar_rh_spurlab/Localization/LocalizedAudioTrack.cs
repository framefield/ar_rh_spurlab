using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace ff.ar_rh_spurlab.Localization
{
    [Serializable]
    [ExcludeFromPreset]
    public class LocalizedAudioTrack : AudioTrack, ILocaleSpecificContent
    {
        public string Locale => _locale;

        [SerializeField]
        private string _locale = "en";

#if UNITY_EDITOR
        // do not play during edit mode
        public override bool CanCreateTrackMixer()
        {
            if (!name.Contains($"({_locale})"))
            {
                name = $"LocalizedAudioTrack ({_locale})";
            }

            if (!Application.isPlaying && _locale != "de")
            {
                return false;
            }

            return base.CanCreateTrackMixer();
        }
#endif
    }
}
