using System;

namespace BGC.Scripting
{
    public class SingleArgumentMathFunction : IValueGetter
    {
        private readonly IValueGetter arg;
        private readonly MathMethod mathMethod;
        private readonly Type valueType;

        public SingleArgumentMathFunction(
            IValueGetter arg,
            MathMethod mathMethod,
            Token source)
        {
            this.arg = arg;
            this.mathMethod = mathMethod;

            Type argType = arg.GetValueType();

            if (!(argType == typeof(double) || argType == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Cannot perform Math.{mathMethod} on non-numerical value {arg} of type {argType.Name}");
            }

            switch (mathMethod)
            {
                case MathMethod.Floor:
                case MathMethod.Ceiling:
                case MathMethod.Round:
                case MathMethod.Abs:
                    valueType = argType;
                    break;

                case MathMethod.Sign:
                    valueType = typeof(int);
                    break;

                case MathMethod.Ln:
                case MathMethod.Log10:
                case MathMethod.Exp:
                case MathMethod.Sqrt:
                case MathMethod.Sin:
                case MathMethod.Cos:
                case MathMethod.Tan:
                case MathMethod.Asin:
                case MathMethod.Acos:
                case MathMethod.Atan:
                case MathMethod.Sinh:
                case MathMethod.Cosh:
                case MathMethod.Tanh:
                    //Only return doubles.
                    valueType = typeof(double);
                    break;

                default:
                    throw new ArgumentException($"Unexpected Operator: {mathMethod}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (returnType == typeof(object))
            {
                returnType = valueType;
            }

            if (!(returnType == typeof(int) || returnType == typeof(double)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying Math.{mathMethod} to {arg} of type {valueType.Name} as type {returnType.Name}");
            }

            if (returnType == typeof(int) && valueType != typeof(int))
            {
                throw new ScriptRuntimeException($"Tried to implicitly cast the results of {this} to type {returnType.Name}");
            }

            if (returnType == typeof(int))
            {
                switch (mathMethod)
                {
                    case MathMethod.Floor: return (T)(object)(int)Math.Floor(arg.GetAs<double>(context));
                    case MathMethod.Ceiling: return (T)(object)(int)Math.Ceiling(arg.GetAs<double>(context));
                    case MathMethod.Round: return (T)(object)(int)Math.Round(arg.GetAs<double>(context));
                    case MathMethod.Abs: return (T)(object)Math.Abs(arg.GetAs<int>(context)); 
                    case MathMethod.Sign: return (T)(object)Math.Sign(arg.GetAs<double>(context));

                    case MathMethod.Ln:
                    case MathMethod.Log10:
                    case MathMethod.Exp:
                    case MathMethod.Sqrt:
                    case MathMethod.Sin:
                    case MathMethod.Cos:
                    case MathMethod.Tan:
                    case MathMethod.Asin:
                    case MathMethod.Acos:
                    case MathMethod.Atan:
                    case MathMethod.Sinh:
                    case MathMethod.Cosh:
                    case MathMethod.Tanh:
                        throw new ScriptRuntimeException($"Math.{mathMethod} function can't return int");

                    default: throw new ArgumentException($"Unexpected MathMethod: {mathMethod}");
                }
            }

            switch (mathMethod)
            {
                case MathMethod.Floor: return (T)(object)Math.Floor(arg.GetAs<double>(context));
                case MathMethod.Ceiling: return (T)(object)Math.Ceiling(arg.GetAs<double>(context));
                case MathMethod.Round: return (T)(object)Math.Round(arg.GetAs<double>(context));
                case MathMethod.Abs: return (T)(object)Math.Abs(arg.GetAs<double>(context));
                case MathMethod.Sign: return (T)(object)(double)Math.Sign(arg.GetAs<double>(context));

                case MathMethod.Ln: return (T)(object)Math.Log(arg.GetAs<double>(context));
                case MathMethod.Log10: return (T)(object)Math.Log10(arg.GetAs<double>(context));
                case MathMethod.Sqrt: return (T)(object)Math.Sqrt(arg.GetAs<double>(context));
                case MathMethod.Exp: return (T)(object)Math.Exp(arg.GetAs<double>(context));

                case MathMethod.Sin: return (T)(object)Math.Sin(arg.GetAs<double>(context));
                case MathMethod.Cos: return (T)(object)Math.Cos(arg.GetAs<double>(context));
                case MathMethod.Tan: return (T)(object)Math.Tan(arg.GetAs<double>(context));
                case MathMethod.Asin: return (T)(object)Math.Asin(arg.GetAs<double>(context));
                case MathMethod.Acos: return (T)(object)Math.Acos(arg.GetAs<double>(context));
                case MathMethod.Atan: return (T)(object)Math.Atan(arg.GetAs<double>(context));
                case MathMethod.Sinh: return (T)(object)Math.Sinh(arg.GetAs<double>(context));
                case MathMethod.Cosh: return (T)(object)Math.Cosh(arg.GetAs<double>(context));
                case MathMethod.Tanh: return (T)(object)Math.Tanh(arg.GetAs<double>(context));

                default: throw new ArgumentException($"Unexpected MathMethod: {mathMethod}");
            }
        }

        public Type GetValueType() => valueType;
    }
}
