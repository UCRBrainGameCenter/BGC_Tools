using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace BGC.Scripting
{
    public abstract class Statement : IExecutable
    {
        public abstract FlowState Execute(ScopeRuntimeContext context);

        public static IExecutable ParseNextStatement(
            IEnumerator<Token> tokens,
            CompilationContext context)
        {
            Token leadingToken = tokens.Current;

            switch (leadingToken)
            {
                case SeparatorToken sepToken:
                    //Valid operations:
                    //  Just a semicolon (empty statement)
                    //  OpenCurlyBoi (new block)
                    switch (sepToken.separator)
                    {
                        case Separator.Semicolon:
                            //Empty statement.  Kick out.
                            //Skip checking for EOF because it's possible for the last statement to be
                            //  just a semicolon.  This would trigger the EOF exception
                            tokens.CautiousAdvance(checkEOF: false);
                            return null;

                        case Separator.OpenCurlyBoi:
                            tokens.CautiousAdvance();
                            IExecutable statement = new Block(tokens, context);
                            tokens.AssertAndSkip(Separator.CloseCurlyBoi, checkEOF: false);
                            return statement;

                        default:
                            throw new ScriptParsingException(
                                source: sepToken,
                                message: $"Statement cannot begin with a {sepToken.separator}.");
                    }


                case KeywordToken kwToken:
                    //Valid operations:
                    //  Continue, Break
                    //  Return (With or Without return value)
                    //  Declaration (With or Without assignment, with or without global)
                    //  If ( Condition )
                    //  While ( Condition )
                    //  For ( A; B; C )
                    return ParseKeywordStatement(kwToken, tokens, context);

                case LiteralToken _:
                case IdentifierToken _:
                case OperatorToken _:
                    //Valid operations:
                    //  Assignment
                    //  PostIncrement
                    //  PreIncrement
                    IExecutable standardExecutable = Expression.ParseNextExecutableExpression(tokens, context);
                    tokens.AssertAndSkip(Separator.Semicolon, checkEOF: false);
                    return standardExecutable;

                default:
                    throw new Exception($"Unexpected TokenType: {leadingToken}");
            }
        }

        private static IExecutable ParseKeywordStatement(
            KeywordToken kwToken,
            IEnumerator<Token> tokens,
            CompilationContext context)
        {
            //Valid operations:
            //  Continue, Break
            //  Return (With or Without return value)
            //  Declaration (With or Without assignment, with or without global)
            //  If ( Condition )
            //  While ( Condition )
            //  For ( A; B; C )

            bool constDeclaration = false;

            switch (kwToken.keyword)
            {
                case Keyword.If:
                    {
                        tokens.CautiousAdvance();
                        tokens.AssertAndSkip(Separator.OpenParen);
                        IValueGetter ifTest = Expression.ParseNextGetterExpression(tokens, context);
                        tokens.AssertAndSkip(Separator.CloseParen);

                        IExecutable trueStatement = ParseNextStatement(tokens, context);
                        IExecutable falseStatement = null;

                        if (trueStatement == null)
                        {
                            throw new ScriptParsingException(
                                source: kwToken,
                                message: $"No statement returned for If block: {kwToken}");
                        }

                        //Check the next token for Else or Else IF
                        if (tokens.TestAndConditionallySkip(Keyword.Else))
                        {
                            falseStatement = ParseNextStatement(tokens, context);
                        }
                        else if (tokens.TestWithoutSkipping(Keyword.ElseIf))
                        {
                            KeywordToken replacementIf = new KeywordToken(
                                source: tokens.Current,
                                keyword: Keyword.If);

                            falseStatement = ParseKeywordStatement(
                                kwToken: replacementIf,
                                tokens: tokens,
                                context: context);
                        }

                        return new IfStatement(
                            condition: ifTest,
                            trueBlock: trueStatement,
                            falseBlock: falseStatement,
                            keywordToken: kwToken);
                    }

                case Keyword.While:
                    {
                        tokens.CautiousAdvance();
                        //New context is used for loop
                        context = context.CreateChildScope(true);

                        tokens.AssertAndSkip(Separator.OpenParen);
                        IValueGetter conditionTest = Expression.ParseNextGetterExpression(tokens, context);
                        tokens.AssertAndSkip(Separator.CloseParen);

                        IExecutable loopBody = ParseNextStatement(tokens, context);

                        return new WhileStatement(
                            continueExpression: conditionTest,
                            loopBody: loopBody,
                            keywordToken: kwToken);
                    }

                case Keyword.For:
                    {
                        tokens.CautiousAdvance();
                        //New context is used for loop
                        context = context.CreateChildScope(true);

                        //Open Paren
                        tokens.AssertAndSkip(Separator.OpenParen);

                        //Initialization
                        IExecutable initializationStatement = ParseNextStatement(tokens, context);

                        //Semicolon already skipped by statement parsing

                        //Continue Expression
                        IValueGetter continueExpression = Expression.ParseNextGetterExpression(tokens, context);

                        //Semicolon
                        tokens.AssertAndSkip(Separator.Semicolon);

                        //Increment
                        IExecutable incrementStatement = ParseForIncrementer(tokens, context);

                        //Close Paren
                        tokens.AssertAndSkip(Separator.CloseParen);

                        IExecutable loopBody = ParseNextStatement(tokens, context);

                        return new ForStatement(
                            initializationStatement: initializationStatement,
                            continueExpression: continueExpression,
                            incrementStatement: incrementStatement,
                            loopBody: loopBody,
                            keywordToken: kwToken);
                    }

                case Keyword.ForEach:
                    {
                        tokens.CautiousAdvance();

                        //New context is used for loop
                        context = context.CreateChildScope(true);

                        //Open Paren
                        tokens.AssertAndSkip(Separator.OpenParen);

                        //Item Declaration
                        Type itemType = tokens.GetTokenAndAdvance<KeywordToken>().keyword.GetValueType();
                        IdentifierToken identifierToken = tokens.GetTokenAndAdvance<IdentifierToken>();

                        IExecutable declaration = new DeclarationOperation(
                            identifierToken: identifierToken,
                            valueType: itemType,
                            context: context);

                        IValue loopVariable = new IdentifierExpression(
                            identifierToken: identifierToken,
                            context: context);

                        tokens.AssertAndSkip(Keyword.In);

                        //Container
                        IValueGetter containerExpression = Expression.ParseNextGetterExpression(tokens, context);

                        //Close Paren
                        tokens.AssertAndSkip(Separator.CloseParen);

                        IExecutable loopBody = ParseNextStatement(tokens, context);

                        return new ForEachStatement(
                            declarationStatement: declaration,
                            loopVariable: loopVariable,
                            containerExpression: containerExpression,
                            loopBody: loopBody,
                            keywordToken: kwToken);
                    }

                case Keyword.Continue:
                case Keyword.Break:
                    {
                        tokens.CautiousAdvance();
                        tokens.AssertAndSkip(Separator.Semicolon, false);
                        return new ControlStatement(kwToken, context);
                    }

                case Keyword.Return:
                    {
                        tokens.CautiousAdvance();
                        IExecutable returnStatement = new ReturnStatement(
                            keywordToken: kwToken,
                            returnValue: Expression.ParseNextGetterExpression(tokens, context),
                            context: context);
                        tokens.AssertAndSkip(Separator.Semicolon, false);
                        return returnStatement;
                    }

                case Keyword.Extern:
                case Keyword.Global:
                    throw new ScriptParsingException(
                        source: kwToken,
                        message: $"{kwToken.keyword} variable declarations invalid in local context.  Put them outside classes and methods.");


                case Keyword.Const:
                    constDeclaration = true;
                    tokens.CautiousAdvance();
                    goto case Keyword.Bool;

                case Keyword.Bool:
                case Keyword.Double:
                case Keyword.Integer:
                case Keyword.String:
                case Keyword.List:
                case Keyword.Queue:
                case Keyword.Stack:
                case Keyword.DepletableBag:
                case Keyword.DepletableList:
                case Keyword.RingBuffer:
                case Keyword.Dictionary:
                case Keyword.HashSet:
                case Keyword.Random:
                    {
                        Type valueType = tokens.ReadType();

                        IdentifierToken identToken = tokens.GetTokenAndAdvance<IdentifierToken>();

                        if (tokens.TestAndConditionallySkip(Separator.Semicolon, false))
                        {
                            if (constDeclaration)
                            {
                                throw new ScriptParsingException(
                                    source: kwToken,
                                    message: $"const variable declared without a value.  What is the point?");
                            }

                            return new DeclarationOperation(
                                identifierToken: identToken,
                                valueType: valueType,
                                context: context);
                        }
                        else if (tokens.TestAndConditionallySkip(Operator.Assignment))
                        {
                            IValueGetter initializerExpression = Expression.ParseNextGetterExpression(tokens, context);

                            tokens.AssertAndSkip(Separator.Semicolon, false);

                            return DeclarationAssignmentOperation.CreateDelcaration(
                                identifierToken: identToken,
                                valueType: valueType,
                                initializer: initializerExpression,
                                isConstant: constDeclaration,
                                context: context);
                        }

                        throw new ScriptParsingException(
                            source: identToken,
                            message: $"Invalid variable declaration: {kwToken} {identToken} {tokens.Current}");
                    }

                case Keyword.System:
                case Keyword.User:
                case Keyword.Debug:
                case Keyword.Math:
                    {
                        IExecutable identifierStatement =
                            Expression.ParseNextExecutableExpression(tokens, context);
                        tokens.AssertAndSkip(Separator.Semicolon, false);
                        return identifierStatement;
                    }

                case Keyword.ElseIf:
                case Keyword.Else:
                    throw new ScriptParsingException(
                        source: kwToken,
                        message: $"Unpaired {kwToken.keyword} token: {kwToken}");

                default:
                    throw new ScriptParsingException(
                        source: kwToken,
                        message: $"A Statement cannot begin with this keyword: {kwToken}");
            }

        }

        public static IExecutable ParseForIncrementer(
            IEnumerator<Token> tokens,
            CompilationContext context)
        {
            if (tokens.TestWithoutSkipping(Separator.CloseParen))
            {
                return null;
            }

            List<IExecutable> returnStatements = new List<IExecutable>();

            do
            {
                returnStatements.Add(Expression.ParseNextExecutableExpression(tokens, context));
            }
            while (tokens.TestAndConditionallySkip(Separator.Comma));

            if (returnStatements.Count == 1)
            {
                return returnStatements[0];
            }

            return new MultiStatement(returnStatements);
        }
    }
}
