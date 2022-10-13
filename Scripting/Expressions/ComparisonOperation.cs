using System;

namespace BGC.Scripting
{
    public class ComparisonOperation : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Operator operatorType;

        public static IExpression CreateComparisonOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            if (!(arg1Type.IsExtendedPrimitive() || arg1Type.IsEnum))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Left side of operator {operatorToken.operatorType} has incompatible type: {arg1Type.Name}");
            }

            if (!(arg2Type.IsExtendedPrimitive() || arg2Type.IsEnum))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Right side of operator {operatorToken.operatorType} has incompatible type: {arg2Type.Name}");
            }

            //Checks that promotion is possible
            operatorToken.GetBinaryPromotedType(arg1Type, arg2Type);

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                return new LiteralToken<bool>(
                    operatorToken,
                    PerformOperator(litArg1.GetAs<object>(), litArg2.GetAs<object>(), operatorToken.operatorType));
            }

            return new ComparisonOperation(arg1, arg2, operatorToken.operatorType);
        }

        private ComparisonOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Operator operatorType)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.operatorType = operatorType;

            switch (operatorType)
            {
                case Operator.IsGreaterThan:
                case Operator.IsGreaterThanOrEqualTo:
                case Operator.IsLessThan:
                case Operator.IsLessThanOrEqualTo:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableOrConvertableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying {operatorType} as type {typeof(T).Name}");
            }

            return (T)(object)PerformOperator(arg1.GetAs<object>(context)!, arg2.GetAs<object>(context)!, operatorType);
        }

        public Type GetValueType() => typeof(bool);

        private static bool PerformOperator(object arg1, object arg2, Operator operatorType)
        {
            switch (operatorType)
            {
                case Operator.IsGreaterThan: return PerformIsGreaterThan(arg1, arg2);
                case Operator.IsGreaterThanOrEqualTo: return PerformIsGreaterThanOrEqualTo(arg1, arg2);
                case Operator.IsLessThan: return PerformIsLessThan(arg1, arg2);
                case Operator.IsLessThanOrEqualTo: return PerformIsLessThanOrEqualTo(arg1, arg2);

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        public static bool PerformIsGreaterThan(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsPrimitive && arg2Type.IsPrimitive)
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case decimal prim2: return prim1 > prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 > prim2;
                            case sbyte prim2: return prim1 > prim2;
                            case short prim2: return prim1 > prim2;
                            case ushort prim2: return prim1 > prim2;
                            case int prim2: return prim1 > prim2;
                            case uint prim2: return prim1 > prim2;
                            case long prim2: return prim1 > prim2;
                            case ulong prim2: return prim1 > prim2;
                            case nint prim2: return prim1 > prim2;
                            case nuint prim2: return prim1 > prim2;
                            case char prim2: return prim1 > prim2;
                            case float prim2: return prim1 > prim2;
                            case double prim2: return prim1 > prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator > to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg1Type.IsEnum && arg2Type.IsEnum)
            {
                if (arg1Type == arg2Type)
                {
                    int arg1Int = Convert.ToInt32(arg1);
                    int arg2Int = Convert.ToInt32(arg2);
                    return arg1Int > arg2Int;
                }

                throw new ArgumentException($"Cannot apply operator > to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return (bool)arg1Type.InvokeStaticMethod("op_GreaterThan", arg1, arg2);
        }
        
        public static bool PerformIsGreaterThanOrEqualTo(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsPrimitive && arg2Type.IsPrimitive)
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case decimal prim2: return prim1 >= prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >= prim2;
                            case sbyte prim2: return prim1 >= prim2;
                            case short prim2: return prim1 >= prim2;
                            case ushort prim2: return prim1 >= prim2;
                            case int prim2: return prim1 >= prim2;
                            case uint prim2: return prim1 >= prim2;
                            case long prim2: return prim1 >= prim2;
                            case ulong prim2: return prim1 >= prim2;
                            case nint prim2: return prim1 >= prim2;
                            case nuint prim2: return prim1 >= prim2;
                            case char prim2: return prim1 >= prim2;
                            case float prim2: return prim1 >= prim2;
                            case double prim2: return prim1 >= prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator >= to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg1Type.IsEnum && arg2Type.IsEnum)
            {
                if (arg1Type == arg2Type)
                {
                    int arg1Int = Convert.ToInt32(arg1);
                    int arg2Int = Convert.ToInt32(arg2);
                    return arg1Int >= arg2Int;
                }

                throw new ArgumentException($"Cannot apply operator >= to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return (bool)arg1Type.InvokeStaticMethod("op_GreaterThanOrEqual", arg1, arg2);
        }
        
        public static bool PerformIsLessThan(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsPrimitive && arg2Type.IsPrimitive)
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case decimal prim2: return prim1 < prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 < prim2;
                            case sbyte prim2: return prim1 < prim2;
                            case short prim2: return prim1 < prim2;
                            case ushort prim2: return prim1 < prim2;
                            case int prim2: return prim1 < prim2;
                            case uint prim2: return prim1 < prim2;
                            case long prim2: return prim1 < prim2;
                            case ulong prim2: return prim1 < prim2;
                            case nint prim2: return prim1 < prim2;
                            case nuint prim2: return prim1 < prim2;
                            case char prim2: return prim1 < prim2;
                            case float prim2: return prim1 < prim2;
                            case double prim2: return prim1 < prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator < to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg1Type.IsEnum && arg2Type.IsEnum)
            {
                if (arg1Type == arg2Type)
                {
                    int arg1Int = Convert.ToInt32(arg1);
                    int arg2Int = Convert.ToInt32(arg2);
                    return arg1Int < arg2Int;
                }

                throw new ArgumentException($"Cannot apply operator < to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return (bool)arg1Type.InvokeStaticMethod("op_LessThan", arg1, arg2);
        }

        public static bool PerformIsLessThanOrEqualTo(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsPrimitive && arg2Type.IsPrimitive)
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case decimal prim2: return prim1 <= prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 <= prim2;
                            case sbyte prim2: return prim1 <= prim2;
                            case short prim2: return prim1 <= prim2;
                            case ushort prim2: return prim1 <= prim2;
                            case int prim2: return prim1 <= prim2;
                            case uint prim2: return prim1 <= prim2;
                            case long prim2: return prim1 <= prim2;
                            case ulong prim2: return prim1 <= prim2;
                            case nint prim2: return prim1 <= prim2;
                            case nuint prim2: return prim1 <= prim2;
                            case char prim2: return prim1 <= prim2;
                            case float prim2: return prim1 <= prim2;
                            case double prim2: return prim1 <= prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator <= to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg1Type.IsEnum && arg2Type.IsEnum)
            {
                if (arg1Type == arg2Type)
                {
                    int arg1Int = Convert.ToInt32(arg1);
                    int arg2Int = Convert.ToInt32(arg2);
                    return arg1Int <= arg2Int;
                }

                throw new ArgumentException($"Cannot apply operator <= to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return (bool)arg1Type.InvokeStaticMethod("op_LessThanOrEqual", arg1, arg2);
        }
    }
}