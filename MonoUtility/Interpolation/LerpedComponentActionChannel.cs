namespace BGC.MonoUtility.Interpolation
{
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
    /// <summary>
    /// Monobehavior to execute and manage animation-like actions on a GameObject Component.
    /// T does not have to be a MonoBehavior.  It still works if it's an interface that a
    /// monobehavior implements
    /// </summary>
    /// <typeparam name="T">The class or interface to retrieve from the GameObject</typeparam>
    public abstract class LerpedComponentActionChannel<T> : LerpChannel<T>
    {
        protected T _target = default;

        protected override T Target
        {
            get
            {
                if (_target == null)
                {
                    _target = gameObject.GetComponent<T>();
                }

                return _target;
            }
        }
    }
}

#pragma warning restore UNT0014 // Invalid type for call to GetComponent
