using System;
using System.Collections.Generic;

namespace ff.ar_rh_spurlab.Locations
{
    public class ReactiveProperty<T> where T : IEquatable<T>
    {
        public event Action<T> OnValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;

                _value = value;
                OnValueChanged?.Invoke(value);
            }
        }

        private T _value;
    }
}