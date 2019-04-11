namespace BGC.MonoUtility.Interaction
{
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
