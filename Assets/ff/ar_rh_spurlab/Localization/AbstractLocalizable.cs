using ff.common.entity;
using UnityEngine;

namespace ff.ar_rh_spurlab.Localization
{
    public abstract class AbstractLocalizable : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            ApplicationLocale.Instance.OnLocaleChange += OnLocaleChangedHandler;
            OnLocaleChangedHandler(ApplicationLocale.Instance.CurrentLocale);
        }

        protected virtual void OnDisable()
        {
            ApplicationLocale.Instance.OnLocaleChange -= OnLocaleChangedHandler;
        }

        protected abstract void OnLocaleChangedHandler(string locale);
    }
}
