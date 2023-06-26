using System;
using ff.common.entity;
using UnityEngine;

namespace ff.ar_rh_spurlab.Localization
{
    public class DefaultLocaleController : MonoBehaviour
    {
        [SerializeField]
        private string _defaultLocale = "en";

        private void Awake()
        {
            ApplicationLocale.Instance.SetLocale(_defaultLocale);
        }
    }
}