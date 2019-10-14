namespace BGC.Scripting
{
    public enum Keyword
    {
        //Conditionals
        If = 0,
        ElseIf,
        Else,

        //Loops
        While,
        For,
        ForEach,
        In,

        //Flow Control
        Continue,
        Break,
        Return,

        //Declaration Modifiers
        Global,
        Extern,
        Const,

        Void,

        //Base Types
        Bool,
        Double,
        Integer,
        String,

        //Container Types
        List,
        Queue,
        Stack,
        DepletableBag,
        DepletableList,
        RingBuffer,
        Dictionary,
        HashSet,

        //Other Types
        Random,

        //Static Types
        System,
        Debug,
        User,
        Math,

        //Construction keyword
        New,

        MAX
    }
}
