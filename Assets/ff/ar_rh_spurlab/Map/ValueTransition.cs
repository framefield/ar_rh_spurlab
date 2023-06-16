using System;

namespace ff.ar_rh_spurlab.Map
{
    public class ValueTransition<T>
    {
        public T TargetValue;
        public T CurrentValue;

        public ValueTransition(T value)
        {
            CurrentValue = value;
            TargetValue = value;
        }

        public T Update(float deltaTime, float speed, Func<T, T, float, T> lerpFunc)
        {
            CurrentValue = lerpFunc(CurrentValue, TargetValue, deltaTime * speed);
            return CurrentValue;
        }
    }
}