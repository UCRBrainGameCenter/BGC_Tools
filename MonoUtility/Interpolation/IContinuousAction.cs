namespace BGC.MonoUtility.Interpolation
{
    public interface IContinuousAction<T>
    {
        void Initialize(T target, float time);
        void CallAction(float time);
    }
}