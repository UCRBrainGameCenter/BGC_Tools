namespace BGC.MonoUtility.Interaction
{
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
    public abstract class ClickComponentChannel<T> : ClickChannel<T>
    {
        protected T _component = default;

        protected override T Target
        {
            get
            {
                if (_component == null)
                {
                    _component = gameObject.GetComponent<T>();
                }

                return _component;
            }
        }
    }
}

#pragma warning restore UNT0014 // Invalid type for call to GetComponent
