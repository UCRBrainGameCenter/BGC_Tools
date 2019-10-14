using System;

namespace BGC.Scripting
{
    public abstract class LiteralToken : Token, IValueGetter
    {
        protected readonly Type valueType;

        public LiteralToken(int line, int column, Type valueType)
            : base(line, column)
        {
            this.valueType = valueType;
        }

        public LiteralToken(Token source, Type valueType)
            : base(source)
        {
            this.valueType = valueType;
        }

        public Type GetValueType() => valueType;

        public abstract T GetAs<T>();
        public T GetAs<T>(RuntimeContext context) => GetAs<T>();
    }

    public class ConstantToken : LiteralToken
    {
        private readonly object value;

        public ConstantToken(int line, int column, object value, Type valueType)
            : base(line, column, valueType)
        {
            this.value = value;
        }

        public ConstantToken(Token source, object value, Type valueType)
            : base(source, valueType)
        {
            this.value = value;
        }

        public override T GetAs<T>()
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(valueType))
            {
                throw new ScriptParsingException(this, $"Tried to use a {valueType.Name} literal as {returnType.Name}");
            }

            if (returnType.IsAssignableFrom(valueType))
            {
                return (T)value;
            }

            return (T)Convert.ChangeType(value, returnType);
        }

        public override string ToString() => value.ToString();
    }

    public class LiteralToken<TLiteral> : LiteralToken
    {
        private readonly TLiteral value;

        public LiteralToken(int line, int column, TLiteral value)
            : base(line, column, typeof(TLiteral))
        {
            this.value = value;
        }

        public LiteralToken(Token source, TLiteral value)
            : base(source, typeof(TLiteral))
        {
            this.value = value;
        }

        public override T GetAs<T>()
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(valueType))
            {
                throw new ScriptParsingException(this, $"Tried to use a {valueType.Name} literal as {returnType.Name}");
            }

            if (returnType.IsAssignableFrom(valueType))
            {
                return (T)(object)value;
            }

            return (T)Convert.ChangeType(value, returnType);
        }

        public override string ToString() => value.ToString();
    }

    public class NullLiteralToken : LiteralToken
    {
        public NullLiteralToken(int line, int column)
            : base(line, column, typeof(NullLiteralToken))
        {
        }

        public NullLiteralToken(Token source)
            : base(source, typeof(NullLiteralToken))
        {
        }

        public override T GetAs<T>() => (T)(object)null;
    }
}
