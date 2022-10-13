using System;

namespace BGC.Scripting
{
    public class EqualityCompairsonOperation : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Operator operatorType;

        public static IExpression CreateEqualityComparisonOperator(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            if (arg1Type != arg2Type &&
                !arg1Type.AssignableOrConvertableFromType(arg2Type) &&
                !arg2Type.AssignableOrConvertableFromType(arg1Type))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Incompatible Types for {operatorToken.operatorType} operator: {arg1Type.Name} and {arg2Type.Name}");
            }

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                return new LiteralToken<bool>(
                    operatorToken,
                    PerformOperator(litArg1.GetAs<object>(), litArg2.GetAs<object>(), operatorToken.operatorType));
            }

            return new EqualityCompairsonOperation(arg1, arg2, operatorToken.operatorType);
        }


        private EqualityCompairsonOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Operator operatorType)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.operatorType = operatorType;

            switch (operatorType)
            {
                case Operator.IsEqualTo:
                case Operator.IsNotEqualTo:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableOrConvertableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException(
                    $"Return value of {operatorType} is a boolean, but it was accessed as {typeof(T).Name}");
            }

            return (T)(object)PerformOperator(arg1.GetAs<object>(context)!, arg2.GetAs<object>(context)!, operatorType);
        }

        public Type GetValueType() => typeof(bool);

        private static bool PerformOperator(object arg1, object arg2, Operator operatorType)
        {
            switch (operatorType)
            {
                case Operator.IsEqualTo: return PerformEquals(arg1, arg2);
                case Operator.IsNotEqualTo: return PerformNotEquals(arg1, arg2);

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        public static bool PerformEquals(object arg1, object arg2)
        {
            if (arg1 is null && arg2 is null)
            {
                return true;
            }
            else if (arg1 is null || arg2 is null)
            {
                return false;
            }

            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsPrimitive && arg2Type.IsPrimitive)
            {
                switch (arg1)
                {
                    case bool prim1:
                        switch (arg2)
                        {
                            case bool prim2: return prim1 == prim2;
                        }
                        break;
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case decimal prim2: return prim1 == prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 == prim2;
                            case sbyte prim2: return prim1 == prim2;
                            case short prim2: return prim1 == prim2;
                            case ushort prim2: return prim1 == prim2;
                            case int prim2: return prim1 == prim2;
                            case uint prim2: return prim1 == prim2;
                            case long prim2: return prim1 == prim2;
                            case ulong prim2: return prim1 == prim2;
                            case nint prim2: return prim1 == prim2;
                            case nuint prim2: return prim1 == prim2;
                            case char prim2: return prim1 == prim2;
                            case float prim2: return prim1 == prim2;
                            case double prim2: return prim1 == prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator == to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg1Type.IsEnum || arg2Type.IsEnum)
            {
                if (arg1Type == arg2Type)
                {
                    return Convert.ToInt32(arg1) == Convert.ToInt32(arg2);
                }

                throw new ArgumentException($"Cannot apply operator == to types {arg1Type.Name} and {arg2Type.Name}");
            }

            try
            {
                return (bool)arg1Type.InvokeStaticMethod("op_Equality", arg1, arg2);
            }
            catch (ArgumentException)
            {
                return arg1 == arg2;
            }
        }

        public static bool PerformNotEquals(object arg1, object arg2)
        {
            if (arg1 is null && arg2 is null)
            {
                return false;
            }
            else if (arg1 is null || arg2 is null)
            {
                return true;
            }

            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsPrimitive && arg2Type.IsPrimitive)
            {
                switch (arg1)
                {
                    case bool prim1:
                        switch (arg2)
                        {
                            case bool prim2: return prim1 != prim2;
                        }
                        break;
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case decimal prim2: return prim1 != prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 != prim2;
                            case sbyte prim2: return prim1 != prim2;
                            case short prim2: return prim1 != prim2;
                            case ushort prim2: return prim1 != prim2;
                            case int prim2: return prim1 != prim2;
                            case uint prim2: return prim1 != prim2;
                            case long prim2: return prim1 != prim2;
                            case ulong prim2: return prim1 != prim2;
                            case nint prim2: return prim1 != prim2;
                            case nuint prim2: return prim1 != prim2;
                            case char prim2: return prim1 != prim2;
                            case float prim2: return prim1 != prim2;
                            case double prim2: return prim1 != prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator != to types {arg1Type.Name} and {arg2Type.Name}");
            }

            try
            {
                return (bool)arg1Type.InvokeStaticMethod("op_Inequality", arg1, arg2);
            }
            catch (ArgumentException)
            {
                return arg1 != arg2;
            }
        }

    }
}