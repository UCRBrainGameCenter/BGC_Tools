namespace BGC.Scripting
{
    public enum Operator
    {
        Assignment = 0,

        Plus,
        Minus,
        Times,
        Divide,
        Modulo,

        BitwiseAnd,
        BitwiseOr,
        BitwiseXOr,

        BitwiseComplement,
        BitwiseLeftShift,
        BitwiseRightShift,

        PlusEquals,
        MinusEquals,
        TimesEquals,
        DivideEquals,
        ModuloEquals,

        Increment,
        Decrement,

        Negate,
        Not,

        IsEqualTo,
        IsNotEqualTo,

        IsGreaterThan,
        IsGreaterThanOrEqualTo,
        IsLessThan,
        IsLessThanOrEqualTo,

        And,
        Or,

        Ternary,

        AndEquals,
        OrEquals,

        BitwiseXOrEquals,
        BitwiseLeftShiftEquals,
        BitwiseRightShiftEquals,

        MAX,

        MemberAccess,
        Indexing,

        AmbiguousMinus
    }
}