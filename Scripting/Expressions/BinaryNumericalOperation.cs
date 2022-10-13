using System;

namespace BGC.Scripting
{
    public class BinaryNumericalOperation : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Operator operatorType;
        private readonly Type valueType;

        public static IExpression CreateBinaryNumericalOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            if (!arg1Type.IsExtendedPrimitive() && !arg1Type.IsEnum)
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Left side of operator {operatorToken.operatorType} not of expected primitive type: {arg1.GetValueType().Name}");
            }

            if (!arg2Type.IsExtendedPrimitive() && !arg2Type.IsEnum)
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Right side of operator {operatorToken.operatorType} not of expected primitive type: {arg2.GetValueType().Name}");
            }

            Type promotedType = operatorToken.GetBinaryPromotedType(arg1Type, arg2Type);

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                return new ConstantToken(
                    operatorToken,
                    PerformOperator(litArg1.GetAs<object>(), litArg2.GetAs<object>(), operatorToken.operatorType),
                    promotedType);
            }

            return new BinaryNumericalOperation(arg1, arg2, promotedType, operatorToken);
        }

        private BinaryNumericalOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Type valueType,
            OperatorToken operatorToken)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.valueType = valueType;
            operatorType = operatorToken.operatorType;

            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();
            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (operatorType)
                {
                    case Operator.Plus:
                    case Operator.Minus:
                    case Operator.Times:
                    case Operator.Divide:
                    case Operator.Modulo:

                    case Operator.BitwiseAnd:
                    case Operator.BitwiseOr:
                    case Operator.BitwiseXOr:
                        //Acceptable
                        break;

                    case Operator.BitwiseLeftShift:
                    case Operator.BitwiseRightShift:
                        if (!arg2Type.IsIntegralType())
                        {
                            throw new ScriptParsingException(operatorToken, $"Operator {operatorType} requires the second argument be an integral type. Received {arg2Type}.");
                        }
                        break;


                    default: throw new ArgumentException($"Unexpected Operator {operatorType}");
                }
            }
            else if (arg1Type.IsEnum || arg2Type.IsEnum)
            {
                Type otherType = arg1Type.IsEnum ? arg2Type : arg1Type;
                switch (operatorType)
                {
                    case Operator.Plus:
                    case Operator.Minus:
                        if (!otherType.IsEnumCompatible())
                        {
                            throw new ScriptParsingException(operatorToken, $"Operator {operatorType} cannot be applied to operands of type {arg1} and {arg2}.");
                        }
                        break;

                    case Operator.Times:
                    case Operator.Divide:
                    case Operator.Modulo:
                    case Operator.BitwiseAnd:
                    case Operator.BitwiseOr:
                    case Operator.BitwiseXOr:
                    case Operator.BitwiseLeftShift:
                    case Operator.BitwiseRightShift:
                        throw new ScriptParsingException(operatorToken, $"Operator {operatorType} cannot be applied to operands of type {arg1} and {arg2}.");


                    default: throw new ArgumentException($"Unexpected Operator {operatorType}");
                }
            }
            else
            {
                string operatorName = operatorType switch
                {
                    Operator.Plus => "op_Addition",
                    Operator.Minus => "op_Subtraction",
                    Operator.Times => "op_Multiply",
                    Operator.Divide => "op_Division",
                    Operator.Modulo => "op_Modulus",
                    Operator.BitwiseLeftShift => "op_LeftShift",
                    Operator.BitwiseRightShift => "op_RightShift",
                    Operator.BitwiseXOr => "op_ExclusiveOr",
                    Operator.BitwiseAnd => "op_BitwiseAnd",
                    Operator.BitwiseOr => "op_BitwiseOr",
                    _ => null,
                };

                if (operatorName == null)
                {
                    throw new ArgumentException($"Unexpected Operator: {operatorType}");
                }

                var (canInvoke, error) = arg1Type.CanInvokeStaticMethod(operatorName, arg1Type, arg2Type);
                if (!canInvoke)
                {
                    throw new ScriptParsingException(operatorToken, error);
                }
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptRuntimeException(
                    $"Return value of {operatorType} is {valueType}, but it was accessed as {returnType.Name}");
            }

            object value = PerformOperator(arg1.GetAs<object>(context)!, arg2.GetAs<object>(context)!, operatorType);

            if (!returnType.IsAssignableFrom(value.GetType()))
            {
                value = Convert.ChangeType(value, returnType);
            }

            return (T)value;
        }

        public Type GetValueType() => valueType;

        private static object PerformOperator(object arg1, object arg2, Operator operatorType)
        {
            switch (operatorType)
            {
                case Operator.Plus: return PerformPlus(arg1, arg2);
                case Operator.Minus: return PerformMinus(arg1, arg2);
                case Operator.Times: return PerformTimes(arg1, arg2);
                case Operator.Divide: return PerformDivide(arg1, arg2);
                case Operator.Modulo: return PerformModulo(arg1, arg2);
                case Operator.BitwiseLeftShift: return PerformBitwiseLeftShift(arg1, arg2);
                case Operator.BitwiseRightShift: return PerformBitwiseRightShift(arg1, arg2);
                case Operator.BitwiseXOr: return PerformBitwiseXOr(arg1, arg2);
                case Operator.BitwiseAnd: return PerformAnd(arg1, arg2);
                case Operator.BitwiseOr: return PerformOr(arg1, arg2);
                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        public static object PerformPlus(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case decimal prim2: return prim1 + prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 + prim2;
                            case sbyte prim2: return prim1 + prim2;
                            case short prim2: return prim1 + prim2;
                            case ushort prim2: return prim1 + prim2;
                            case int prim2: return prim1 + prim2;
                            case uint prim2: return prim1 + prim2;
                            case long prim2: return prim1 + prim2;
                            case ulong prim2: return prim1 + prim2;
                            case nint prim2: return prim1 + prim2;
                            case nuint prim2: return prim1 + prim2;
                            case char prim2: return prim1 + prim2;
                            case float prim2: return prim1 + prim2;
                            case double prim2: return prim1 + prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator + to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg1Type.IsEnum)
            {
                int arg1Value = Convert.ToInt32(arg1);
                switch (arg2)
                {
                    case byte prim2: return Enum.ToObject(arg1Type, arg1Value + prim2);
                    case sbyte prim2: return Enum.ToObject(arg1Type, arg1Value + prim2);
                    case short prim2: return Enum.ToObject(arg1Type, arg1Value + prim2);
                    case ushort prim2: return Enum.ToObject(arg1Type, arg1Value + prim2);
                    case int prim2: return Enum.ToObject(arg1Type, arg1Value + prim2);
                    case char prim2: return Enum.ToObject(arg1Type, arg1Value + prim2);
                }

                throw new ArgumentException($"Cannot apply operator + to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg2Type.IsEnum)
            {
                int arg2Value = Convert.ToInt32(arg2);
                switch (arg1)
                {
                    case byte prim1: return Enum.ToObject(arg2Type, arg2Value + prim1);
                    case sbyte prim1: return Enum.ToObject(arg2Type, arg2Value + prim1);
                    case short prim1: return Enum.ToObject(arg2Type, arg2Value + prim1);
                    case ushort prim1: return Enum.ToObject(arg2Type, arg2Value + prim1);
                    case int prim1: return Enum.ToObject(arg2Type, arg2Value + prim1);
                    case char prim1: return Enum.ToObject(arg2Type, arg2Value + prim1);
                }

                throw new ArgumentException($"Cannot apply operator + to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_Addition", arg1, arg2);
        }

        public static object PerformMinus(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case decimal prim2: return prim1 - prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 - prim2;
                            case sbyte prim2: return prim1 - prim2;
                            case short prim2: return prim1 - prim2;
                            case ushort prim2: return prim1 - prim2;
                            case int prim2: return prim1 - prim2;
                            case uint prim2: return prim1 - prim2;
                            case long prim2: return prim1 - prim2;
                            case ulong prim2: return prim1 - prim2;
                            case nint prim2: return prim1 - prim2;
                            case nuint prim2: return prim1 - prim2;
                            case char prim2: return prim1 - prim2;
                            case float prim2: return prim1 - prim2;
                            case double prim2: return prim1 - prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator - to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg1Type.IsEnum)
            {
                int arg1Value = Convert.ToInt32(arg1);
                switch (arg2)
                {
                    case byte prim2: return Enum.ToObject(arg1Type, arg1Value - prim2);
                    case sbyte prim2: return Enum.ToObject(arg1Type, arg1Value - prim2);
                    case short prim2: return Enum.ToObject(arg1Type, arg1Value - prim2);
                    case ushort prim2: return Enum.ToObject(arg1Type, arg1Value - prim2);
                    case int prim2: return Enum.ToObject(arg1Type, arg1Value - prim2);
                    case char prim2: return Enum.ToObject(arg1Type, arg1Value - prim2);
                }

                throw new ArgumentException($"Cannot apply operator - to types {arg1Type.Name} and {arg2Type.Name}");
            }
            else if (arg2Type.IsEnum)
            {
                int arg2Value = Convert.ToInt32(arg2);
                switch (arg1)
                {
                    case byte prim1: return Enum.ToObject(arg2Type, arg2Value - prim1);
                    case sbyte prim1: return Enum.ToObject(arg2Type, arg2Value - prim1);
                    case short prim1: return Enum.ToObject(arg2Type, arg2Value - prim1);
                    case ushort prim1: return Enum.ToObject(arg2Type, arg2Value - prim1);
                    case int prim1: return Enum.ToObject(arg2Type, arg2Value - prim1);
                    case char prim1: return Enum.ToObject(arg2Type, arg2Value - prim1);
                }

                throw new ArgumentException($"Cannot apply operator - to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_Subtraction", arg1, arg2);
        }

        public static object PerformTimes(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case decimal prim2: return prim1 * prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 * prim2;
                            case sbyte prim2: return prim1 * prim2;
                            case short prim2: return prim1 * prim2;
                            case ushort prim2: return prim1 * prim2;
                            case int prim2: return prim1 * prim2;
                            case uint prim2: return prim1 * prim2;
                            case long prim2: return prim1 * prim2;
                            case ulong prim2: return prim1 * prim2;
                            case nint prim2: return prim1 * prim2;
                            case nuint prim2: return prim1 * prim2;
                            case char prim2: return prim1 * prim2;
                            case float prim2: return prim1 * prim2;
                            case double prim2: return prim1 * prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator * to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_Multiply", arg1, arg2);
        }

        public static object PerformDivide(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case decimal prim2: return prim1 / prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 / prim2;
                            case sbyte prim2: return prim1 / prim2;
                            case short prim2: return prim1 / prim2;
                            case ushort prim2: return prim1 / prim2;
                            case int prim2: return prim1 / prim2;
                            case uint prim2: return prim1 / prim2;
                            case long prim2: return prim1 / prim2;
                            case ulong prim2: return prim1 / prim2;
                            case nint prim2: return prim1 / prim2;
                            case nuint prim2: return prim1 / prim2;
                            case char prim2: return prim1 / prim2;
                            case float prim2: return prim1 / prim2;
                            case double prim2: return prim1 / prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator / to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_Division", arg1, arg2);
        }

        public static object PerformModulo(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case decimal prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case decimal prim2: return prim1 % prim2;
                        }
                        break;
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 % prim2;
                            case sbyte prim2: return prim1 % prim2;
                            case short prim2: return prim1 % prim2;
                            case ushort prim2: return prim1 % prim2;
                            case int prim2: return prim1 % prim2;
                            case uint prim2: return prim1 % prim2;
                            case long prim2: return prim1 % prim2;
                            case ulong prim2: return prim1 % prim2;
                            case nint prim2: return prim1 % prim2;
                            case nuint prim2: return prim1 % prim2;
                            case char prim2: return prim1 % prim2;
                            case float prim2: return prim1 % prim2;
                            case double prim2: return prim1 % prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator % to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_Modulus", arg1, arg2);
        }

        public static object PerformBitwiseLeftShift(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 << prim2;
                            case sbyte prim2: return prim1 << prim2;
                            case short prim2: return prim1 << prim2;
                            case ushort prim2: return prim1 << prim2;
                            case int prim2: return prim1 << prim2;
                            case char prim2: return prim1 << prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator << to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_LeftShift", arg1, arg2);
        }

        public static object PerformBitwiseRightShift(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 >> prim2;
                            case sbyte prim2: return prim1 >> prim2;
                            case short prim2: return prim1 >> prim2;
                            case ushort prim2: return prim1 >> prim2;
                            case int prim2: return prim1 >> prim2;
                            case char prim2: return prim1 >> prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator >> to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_RightShift", arg1, arg2);
        }

        public static object PerformBitwiseXOr(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case bool prim1:
                        switch (arg2)
                        {
                            case bool prim2: return prim1 ^ prim2;
                        }
                        break;
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case ulong prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case nuint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case ulong prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case nuint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case ulong prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case nuint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case ulong prim2: return prim1 ^ prim2;
                            case nuint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case ulong prim2: return prim1 ^ prim2;
                            case nuint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 ^ prim2;
                            case sbyte prim2: return prim1 ^ prim2;
                            case short prim2: return prim1 ^ prim2;
                            case ushort prim2: return prim1 ^ prim2;
                            case int prim2: return prim1 ^ prim2;
                            case uint prim2: return prim1 ^ prim2;
                            case long prim2: return prim1 ^ prim2;
                            case ulong prim2: return prim1 ^ prim2;
                            case nint prim2: return prim1 ^ prim2;
                            case nuint prim2: return prim1 ^ prim2;
                            case char prim2: return prim1 ^ prim2;
                        }
                        break;
                    // Floats and Doubles treat ^ as a power operator.
                    case float prim1:
                        switch (arg2)
                        {
                            case byte prim2: return (float)Math.Pow(prim1, prim2);
                            case sbyte prim2: return (float)Math.Pow(prim1, prim2);
                            case short prim2: return (float)Math.Pow(prim1, prim2);
                            case ushort prim2: return (float)Math.Pow(prim1, prim2);
                            case int prim2: return (float)Math.Pow(prim1, prim2);
                            case uint prim2: return (float)Math.Pow(prim1, prim2);
                            case long prim2: return (float)Math.Pow(prim1, prim2);
                            case ulong prim2: return (float)Math.Pow(prim1, prim2);
                            case nint prim2: return (float)Math.Pow(prim1, prim2);
                            case nuint prim2: return (float)Math.Pow(prim1, prim2);
                            case char prim2: return (float)Math.Pow(prim1, prim2); 
                        }
                        break;
                    case double prim1:
                        switch (arg2)
                        {
                            case byte prim2: return Math.Pow(prim1, prim2);
                            case sbyte prim2: return Math.Pow(prim1, prim2);
                            case short prim2: return Math.Pow(prim1, prim2);
                            case ushort prim2: return Math.Pow(prim1, prim2);
                            case int prim2: return Math.Pow(prim1, prim2);
                            case uint prim2: return Math.Pow(prim1, prim2);
                            case long prim2: return Math.Pow(prim1, prim2);
                            case ulong prim2: return Math.Pow(prim1, prim2);
                            case nint prim2: return Math.Pow(prim1, prim2);
                            case nuint prim2: return Math.Pow(prim1, prim2);
                            case char prim2: return Math.Pow(prim1, prim2);
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator ^ to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_ExclusiveOr", arg1, arg2);
        }

        public static object PerformAnd(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case bool prim1:
                        switch (arg2)
                        {
                            case bool prim2: return prim1 & prim2;
                        }
                        break;
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case ulong prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case nuint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case ulong prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case nuint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case ulong prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case nuint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case ulong prim2: return prim1 & prim2;
                            case nuint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case ulong prim2: return prim1 & prim2;
                            case nuint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 & prim2;
                            case sbyte prim2: return prim1 & prim2;
                            case short prim2: return prim1 & prim2;
                            case ushort prim2: return prim1 & prim2;
                            case int prim2: return prim1 & prim2;
                            case uint prim2: return prim1 & prim2;
                            case long prim2: return prim1 & prim2;
                            case ulong prim2: return prim1 & prim2;
                            case nint prim2: return prim1 & prim2;
                            case nuint prim2: return prim1 & prim2;
                            case char prim2: return prim1 & prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator & to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_BitwiseAnd", arg1, arg2);
        }

        public static object PerformOr(object arg1, object arg2)
        {
            Type arg1Type = arg1.GetType();
            Type arg2Type = arg2.GetType();

            if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                switch (arg1)
                {
                    case bool prim1:
                        switch (arg2)
                        {
                            case bool prim2: return prim1 | prim2;
                        }
                        break;
                    case byte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case ulong prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case nuint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case sbyte prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case short prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case ushort prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case ulong prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case nuint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case int prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case uint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case ulong prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case nuint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case long prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case ulong prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case ulong prim2: return prim1 | prim2;
                            case nuint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case nint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case nuint prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case ulong prim2: return prim1 | prim2;
                            case nuint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                    case char prim1:
                        switch (arg2)
                        {
                            case byte prim2: return prim1 | prim2;
                            case sbyte prim2: return prim1 | prim2;
                            case short prim2: return prim1 | prim2;
                            case ushort prim2: return prim1 | prim2;
                            case int prim2: return prim1 | prim2;
                            case uint prim2: return prim1 | prim2;
                            case long prim2: return prim1 | prim2;
                            case ulong prim2: return prim1 | prim2;
                            case nint prim2: return prim1 | prim2;
                            case nuint prim2: return prim1 | prim2;
                            case char prim2: return prim1 | prim2;
                        }
                        break;
                }

                throw new ArgumentException($"Cannot apply operator | to types {arg1Type.Name} and {arg2Type.Name}");
            }

            return arg1Type.InvokeStaticMethod("op_BitwiseOr", arg1, arg2);
        }
    }
}