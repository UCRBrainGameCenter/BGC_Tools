namespace BGC.Scripting
{
    public enum Operator
    {
        Assignment = 0,

        Plus,
        Minus,
        Times,
        Divide,
        Power,
        Modulo,

        CastDouble,
        CastInteger,

        PlusEquals,
        MinusEquals,
        TimesEquals,
        DivideEquals,
        PowerEquals,
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

        MAX,

        MemberAccess,
        Indexing,

        AmbiguousMinus
    }

}
