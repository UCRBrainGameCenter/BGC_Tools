using System;
using BGC.Scripting.Parsing;

namespace BGC.Scripting
{
    public static class OverloadedOperation
    {
        public static IExpression CreateOverloadedOperator(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type[] types = new Type[] { arg1.GetValueType(), arg2.GetValueType() };

            InvocationArgument[] methodArguments = new InvocationArgument[] {
            new InvocationArgument(arg1, ArgumentType.Standard),
            new InvocationArgument(arg2, ArgumentType.Standard) };

            if (ClassRegistrar.GetStaticMethodExpression(
                    type: types[0],
                    genericMethodArguments: null,
                    args: methodArguments,
                    methodName: GetOverloadName(operatorToken.operatorType),
                    source: operatorToken) is IExpression firstOverload)
            {
                return firstOverload;
            }

            if (types[0] != types[1] &&
                ClassRegistrar.GetStaticMethodExpression(
                    type: types[1],
                    genericMethodArguments: null,
                    args: methodArguments,
                    methodName: GetOverloadName(operatorToken.operatorType),
                    source: operatorToken) is IExpression secondOverload)
            {
                return secondOverload;
            }

            if (operatorToken.operatorType == Operator.IsEqualTo ||
                operatorToken.operatorType == Operator.IsNotEqualTo)
            {
                //Fall back to standard equality comparison
                return EqualityCompairsonOperation.CreateEqualityComparisonOperator(
                    arg1: arg1,
                    arg2: arg2,
                    operatorToken: operatorToken);
            }

            throw new ScriptParsingException(
                source: operatorToken,
                message: $"No overload for operator {operatorToken.operatorType} and types {types[0]} and {types[1]} found.");
        }

        private static string GetOverloadName(Operator overloadedOperator) => overloadedOperator switch
        {
            Operator.Plus => "op_Addition",
            Operator.Minus => "op_Subtraction",
            Operator.Times => "op_Multiply",
            Operator.Divide => "op_Division",
            Operator.Modulo => "op_Modulus",

            Operator.PlusEquals => "op_AdditionAssignment",
            Operator.MinusEquals => "op_SubtractionAssignment",
            Operator.TimesEquals => "op_MultiplicationAssignment",
            Operator.DivideEquals => "op_DivisionAssignment",
            Operator.ModuloEquals => "op_ModulusAssignment",

            Operator.IsEqualTo => "op_Equality",
            Operator.IsNotEqualTo => "op_Inequality",

            Operator.IsGreaterThan => "op_GreaterThan",
            Operator.IsGreaterThanOrEqualTo => "op_GreaterThanOrEqual",
            Operator.IsLessThan => "op_LessThan",
            Operator.IsLessThanOrEqualTo => "op_LessThanOrEqual",

            Operator.And => "op_LogicalAnd",
            Operator.Or => "op_LogicalOr",

            Operator.AndEquals => "op_BitwiseAndAssignment",
            Operator.OrEquals => "op_BitwiseOrAssignment",

            _ => ""
        };
    }
}