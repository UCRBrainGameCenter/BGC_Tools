using System;

namespace BGC.Scripting
{
    public class DoubleArgumentMathFunction : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly MathMethod mathMethod;
        private readonly Type valueType;

        public DoubleArgumentMathFunction(
            IValueGetter arg1,
            IValueGetter arg2,
            MathMethod mathMethod,
            Token source)
        {
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            this.arg1 = arg1;
            this.arg2 = arg2;
            this.mathMethod = mathMethod;

            if (!(arg1Type == typeof(double) || arg1Type == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"First Argument of Math.{mathMethod} not of expected type int or bool: {arg1.GetValueType().Name}");
            }

            if (!(arg2Type == typeof(double) || arg2Type == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Second Argument of Math.{mathMethod} not of expected type int or bool: {arg2.GetValueType().Name}");
            }

            if (arg1Type == arg2Type)
            {
                valueType = arg1Type;
            }
            else
            {
                valueType = typeof(double);
            }

            switch (mathMethod)
            {
                case MathMethod.Min:
                case MathMethod.Max:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected MathMethod: {mathMethod}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to implicitly cast the results of {this} to type {returnType}");
            }

            if (valueType == typeof(int))
            {
                //Integer type
                switch (mathMethod)
                {
                    case MathMethod.Min: return (T)Convert.ChangeType(Math.Min(arg1.GetAs<int>(context), arg2.GetAs<int>(context)), returnType);
                    case MathMethod.Max: return (T)Convert.ChangeType(Math.Max(arg1.GetAs<int>(context), arg2.GetAs<int>(context)), returnType);

                    default: throw new ArgumentException($"Unexpected MathMethod: {mathMethod}");
                }
            }

            //Double type
            switch (mathMethod)
            {
                case MathMethod.Min: return (T)Convert.ChangeType(Math.Min(arg1.GetAs<double>(context), arg2.GetAs<double>(context)), returnType);
                case MathMethod.Max: return (T)Convert.ChangeType(Math.Max(arg1.GetAs<double>(context), arg2.GetAs<double>(context)), returnType);

                default: throw new ArgumentException($"Unexpected MathMethod: {mathMethod}");
            }
        }

        public Type GetValueType() => valueType;
    }
}
