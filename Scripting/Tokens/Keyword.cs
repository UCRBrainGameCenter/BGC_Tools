namespace BGC.Scripting
{
    public enum Keyword
    {
        //Conditionals
        If = 0,
        ElseIf,
        Else,
        Switch,

        //Loops
        While,
        For,
        ForEach,
        In,  //Also a parameter modifier

        //Flow Control
        Continue,
        Break,
        Return,
        Case,
        Default,

        //Declaration Modifiers
        Global,
        Extern,
        Const,

        //Construction keyword
        New,

        //Parameter Modifiers
        Out,
        Ref,
        Params,

        MAX
    }
}