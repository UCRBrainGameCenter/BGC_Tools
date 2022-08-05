namespace BGC.Scripting
{
    public interface IValueGetter : IExpression, ITypedValue
    {
        T GetAs<T>(RuntimeContext context);
    }
}