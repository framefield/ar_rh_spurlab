using System;
using UnityEngine;


namespace ff.ar_rh_spurlab.Localization
{
    [Serializable]
    public class LocalizedAudioClip
    {
        [SerializeField]
        private AudioLocalePair[] _audioLocalePairs;

        [Serializable]
        public struct AudioLocalePair
        {
            public string Locale;
            public AudioClip AudioClip;
        }

        public bool TryGetAudioClip(string locale, out AudioClip audioClip)
        {
            foreach (var audioLocalePair in _audioLocalePairs)
            {
                if (audioLocalePair.Locale == locale)
                {
                    audioClip = audioLocalePair.AudioClip;
                    return true;
                }
            }

            audioClip = null;
            return false;
        }
    }
}