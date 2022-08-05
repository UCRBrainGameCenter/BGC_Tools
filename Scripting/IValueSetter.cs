namespace BGC.Scripting
{
    public interface IValueSetter : IExpression, ITypedValue
    {
        void Set(RuntimeContext context, object value);
        void SetAs<T>(RuntimeContext context, T value);
    }
}