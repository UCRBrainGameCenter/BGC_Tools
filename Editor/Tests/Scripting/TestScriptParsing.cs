using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using BGC.Scripting;
using BGC.Reports;

namespace BGC.Tests
{
    public class ScriptParsing
    {
        [Test]
        public void InitialScriptTest()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            globalContext.DeclareVariable("localPreInc", typeof(int), 0);
            globalContext.DeclareVariable("localPostInc", typeof(int), 101);
            globalContext.DeclareVariable("globalPreInc", typeof(int), 1000);

            string testScript = @"
            //These initialization expressions should be skipped
            global int localPreInc = 1000;
            global int localPostInc;
            global int globalPreInc = 200;

            //This one will not be skipped
            global int globalPostInc = 201;

            int argInt;

            void SetupFunction(int argument)
            {
                argInt = argument;

                globalPreInc = 200;
                for (int i = 0; i < 10; i++)
                {
                    localPostInc++;
                    ++globalPreInc;
                    globalPostInc++;
                    ++localPreInc;
                }
            }

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                tests.Add(localPreInc == 10);
                tests.Add(localPostInc == 111);
                tests.Add(globalPreInc == 210);
                tests.Add(globalPostInc == 211);
                tests.Add(argInt == 666);

                return tests;
            }";


            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "SetupFunction",
                        returnType: typeof(void),
                        arguments: new ArgumentData("argument", typeof(int))),
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            script.ExecuteFunction("SetupFunction", context, 666);

            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Assert(globalContext.GetExistingValue<int>("localPreInc") == 10,
                $"localPreInc should be {10}, but was {globalContext.GetExistingValue<int>("localPreInc")}");

            Debug.Assert(globalContext.GetExistingValue<int>("localPostInc") == 111,
                $"localPostInc should be {111}, but was {globalContext.GetExistingValue<int>("localPostInc")}");

            Debug.Assert(globalContext.GetExistingValue<int>("globalPreInc") == 210,
                $"globalPreInc should be {210}, but was {globalContext.GetExistingValue<int>("globalPreInc")}");

            Debug.Assert(globalContext.GetExistingValue<int>("globalPostInc") == 211,
                $"globalPostInc should be {211}, but was {globalContext.GetExistingValue<int>("globalPostInc")}");


            Debug.Log($"Ran {tests.Count + 4} Initial tests");
        }

        [Test]
        public void BreakTest()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            //Some global variables  (all default initialize to 0)
            global int countLessThan10;
            global int countLessThanEqualTo15;
            global int countLessThan50;
            global int endI;

            //Local Variable
            int testEndI;

            void SetupFunction()
            {
                for (int i = 0; i < 50; i++)
                {
                    if (i < 10)
                        countLessThan10++;

                    if (i <= 15)
                    {
                        countLessThanEqualTo15++;
                    }

                    if (i < 50) countLessThan50++;

                    if (i == 30)
                    {
                        testEndI = i;
                        break;
                    }
                }
                endI = testEndI;
            }

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                tests.Add(countLessThan10 == 10);
                tests.Add(countLessThanEqualTo15 == 16);
                tests.Add(countLessThan50 == 31);
                tests.Add(endI == 30);

                return tests;
            }";


            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "SetupFunction",
                        returnType: typeof(void)),
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            script.ExecuteFunction("SetupFunction", context);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Break tests");
        }

        [Test]
        public void NaNTest()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            //This is a test of some features of Double
            global double testValue = NaN;
            global string testString = ""Original String""; //Another inline comment
            /* A block comment */

            void SetupFunction()
            {
                if (Math.IsNaN(testValue))
                {
                    testValue = 3.0*4.0 + 2.0 * 6;
                    testValue ^= 2;

                    testString = ""New String"";
                }
            }

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                tests.Add(testValue == 576);
                tests.Add(testString == ""New String"");
                return tests;
            }";


            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "SetupFunction",
                        returnType: typeof(void)),
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            script.ExecuteFunction("SetupFunction", context);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Assert(context.GetExistingValue<double>("testValue") == 576,
                $"testValue should be {576}, but was {context.GetExistingValue<double>("testValue")}");

            Debug.Assert(context.GetExistingValue<string>("testString") == "New String",
                $"testString should be {"New String"}, but was {context.GetExistingValue<string>("testString")}");

            Debug.Log($"Ran {tests.Count + 2} NaN and Comment tests");
        }

        [Test]
        public void WhileTest()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            //This will test some While features
            global int primeFactors = 0;

            // 45750
            int numberToFactorize = 2*3*5*5*5*61;
            global bool matchesExpectation = numberToFactorize == 45750;

            int factor = 2;

            void SetupFunction()
            {
                while (numberToFactorize > 1)
                {
                    while (numberToFactorize % factor == 0)
                    {
                        primeFactors++;
                        numberToFactorize /= factor;
                    }

                    if (factor == 2)
                    {
                        factor += 1;
                    }
                    else
                    {
                        factor += 2;
                    }
                }
            }

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                tests.Add(matchesExpectation == true);
                tests.Add(primeFactors == 6);
                return tests;
            }";


            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "SetupFunction",
                        returnType: typeof(void)),
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            script.ExecuteFunction("SetupFunction", context);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} While tests");
        }

        [Test]
        public void TernaryTest()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            //This will test the ternary operator
            global double assignee1 = 0;
            global double assignee2 = 0;
            global double assignee3 = 0;

            bool test = false;

            void SetupFunction()
            {
                assignee1 = !test ? 1 : 2;
                assignee2 = test ? 1.0 : 2;
                assignee3 = test ? 1.0 : 2.0;
            }

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                tests.Add(assignee1 == 1);
                tests.Add(assignee2 == 2);
                tests.Add(assignee3 == 2);
                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "SetupFunction",
                        returnType: typeof(void)),
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            script.ExecuteFunction("SetupFunction", context);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Ternary tests");
        }

        [Test]
        public void ReturnTest()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScriptA = @"
            double tempA = 0.0;

            bool TestFunction()
            {
                if (tempA < 1.0)
                    return true;
                else
                    return false;
            }";


            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScriptA,
                    new FunctionSignature(
                        identifier: "TestFunction",
                        returnType: typeof(bool)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            bool returnValue = script.ExecuteFunction<bool>("TestFunction", context);

            Debug.Assert(returnValue == true,
                $"return value should be {true}, but was {returnValue}");

            string testScriptB = @"
            double tempA = 0.0;

            bool TestFunction()
            {
                if (tempA < 1.0)
                    return true;
                else
                    return;
            }";

            Assert.Throws<ScriptParsingException>(
                () => ScriptParser.LexAndParseScript(testScriptB,
                    new FunctionSignature("TestFunction", typeof(bool))));
        }

        [Test]
        public void StringConcatenationTest()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            string testString1 = ""Oh Hai"";
            string testString2 = ""!"";
            global string testString3 = testString1 + testString2 + 3;
            global string testString4;
            global int testLength1; 
            global int testLength3; 
            global int testLength4;

            void SetupFunction()
            {
                testString4 += ""Test "";
                testString4 += 4;

                testLength1 = ""Oh Hai"".Length;
                testLength3 = testString3.Length;
                testLength4 = testString4.Length;
            }

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                tests.Add(testString3 == ""Oh Hai!3"");
                tests.Add(testString4 == ""Test 4"");
                tests.Add(testLength1 == 6);
                tests.Add(testLength3 == 8);
                tests.Add(testLength4 == 6);
                return tests;
            }";


            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "SetupFunction",
                        returnType: typeof(void)),
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            script.ExecuteFunction("SetupFunction", context);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} String Concatenation tests");
        }

        [Test]
        public void ListTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            globalContext.DeclareVariable("boolList", typeof(List<bool>), new List<bool>() { true, false, true });

            string testScript = @"
            global List<int> intList = new List<int>();
            global List<double> doubleList = new List<double>();
            global List<string> stringList = new List<string>();

            extern List<bool> boolList;

            global bool testBool = boolList[2];
            global bool intListTest1;
            global bool intListTest2;
            global int intListTest3;

            void SetupFunction()
            {
                boolList[1] = true;

                intList.Add(5);

                intListTest1 = intList.Contains(5);
                intListTest2 = intList.Contains(6);

                intList.Add(6);
                intListTest3 = intList.IndexOf(6);
            }

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                tests.Add(intList != null);
                tests.Add(doubleList != null);
                tests.Add(stringList != null);
                tests.Add(boolList != null);
                tests.Add(boolList[1] == true);
                tests.Add(testBool == true);

                tests.Add(intList.Count == 2);
                tests.Add(intList[0] == 5);
                tests.Add(intListTest1 == true);
                tests.Add(intListTest2 == false);
                tests.Add(intListTest3 == 1);

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "SetupFunction",
                        returnType: typeof(void)),
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            script.ExecuteFunction("SetupFunction", context);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} List tests");
        }

        [Test]
        public void QueueTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            Queue<int> intQueue = new Queue<int>();

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                for (int i = 0; i < 12; i++)
                {
                    intQueue.Enqueue(i);
                }

                tests.Add(intQueue.Dequeue() == 0 && intQueue.Dequeue() == 1);
                tests.Add(intQueue.Contains(4));
                tests.Add(!intQueue.Contains(1));
                tests.Add(intQueue.Peek() == 2);
                tests.Add(intQueue.Count == 10);
                intQueue.Enqueue(100);
                tests.Add(intQueue.Count == 11);

                while (intQueue.Count > 1)
                {
                    intQueue.Dequeue();
                }

                tests.Add(intQueue.Dequeue() == 100);

                return tests;
            }";


            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Queue tests");
        }

        [Test]
        public void StackTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            Stack<int> intStack = new Stack<int>();

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                for (int i = 0; i < 12; i++)
                {
                    intStack.Push(i);
                }

                tests.Add(intStack.Pop() == 11 && intStack.Pop() == 10);
                tests.Add(intStack.Contains(8));
                tests.Add(!intStack.Contains(10));
                tests.Add(intStack.Peek() == 9);
                tests.Add(intStack.Count == 10);
                intStack.Push(100);
                tests.Add(intStack.Count == 11);

                while (intStack.Count > 1)
                {
                    intStack.Pop();
                }

                tests.Add(intStack.Pop() == 0);

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Stack tests");
        }

        [Test]
        public void RingBufferTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            RingBuffer<int> intBuffer = new RingBuffer<int>(10);

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                intBuffer.Push(0);
                intBuffer.Push(0);
                intBuffer.Push(0);

                tests.Add(intBuffer.Count == 3);

                for (int i = 0; i < 12; i++)
                {
                    intBuffer.Push(i);
                }

                tests.Add(intBuffer.Count == 10);
                tests.Add(intBuffer.Head == 11);
                tests.Add(intBuffer.Tail == 2);

                intBuffer.Push(12);
                tests.Add(intBuffer.Head == 12);
                tests.Add(intBuffer.Tail == 3);

                tests.Add(intBuffer.PopBack() == 3);
                tests.Add(intBuffer.Count == 9);


                tests.Add(intBuffer.PeekHead() == 12);
                tests.Add(intBuffer.PeekTail() == 4);
                tests.Add(intBuffer[3] == 9);
                intBuffer.RemoveAt(3);
                tests.Add(intBuffer[3] == 8);
                intBuffer.Add(99);
                tests.Add(intBuffer.Head == 99);

                tests.Add(intBuffer.Contains(99));
                tests.Add(intBuffer.Remove(99));
                tests.Add(intBuffer.Contains(99) == false);

                int index = intBuffer.GetIndex(6);
                tests.Add(index != -1);
                intBuffer.RemoveAt(index);
                tests.Add(intBuffer.GetIndex(6) == -1);


                intBuffer.Clear();
                tests.Add(intBuffer.Count == 0);
                tests.Add(intBuffer.Size == 10);
        
                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} RingBuffer tests");
        }

        [Test]
        public void MathTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"


            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

        
                tests.Add(Math.Floor(10.1) == 10);
                tests.Add(Math.Floor(10.0) == 10);
                tests.Add(Math.Floor(9.9999) == 9);

                tests.Add(Math.Ceiling(10.1) == 11);
                tests.Add(Math.Ceiling(10.0) == 10);
                tests.Add(Math.Ceiling(9.9999) == 10);

                tests.Add(Math.Round(10.1) == 10);
                tests.Add(Math.Round(10.0) == 10);
                tests.Add(Math.Round(9.9999) == 10);

                tests.Add(Math.Ln(Math.E) == 1);

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Math tests");
        }

        [Test]
        public void RandomTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                Random random = new Random();
                int randomSeed = random.Next();
                Debug.Log(""Random Seed: "" + randomSeed);

                Random randomA = new Random(randomSeed);
                Random randomB = new Random(randomSeed);

        
                tests.Add(randomA.Next() == randomB.Next());
                tests.Add(randomA.Next() == randomB.Next());
                tests.Add(randomA.Next() == randomB.Next());
                tests.Add(randomA.Next() == randomB.Next());

                tests.Add(randomA.NextDouble() == randomB.NextDouble());
                tests.Add(randomA.NextDouble() == randomB.NextDouble());
                tests.Add(randomA.NextDouble() == randomB.NextDouble());
                tests.Add(randomA.NextDouble() == randomB.NextDouble());

                int lowerBound = random.Next(10);
                int upperBound = random.Next(lowerBound + 10, lowerBound + 20);

                tests.Add(randomA.Next(lowerBound, upperBound) == randomB.Next(lowerBound, upperBound));
                tests.Add(randomA.Next(lowerBound, upperBound) == randomB.Next(lowerBound, upperBound));
                tests.Add(randomA.Next(lowerBound, upperBound) == randomB.Next(lowerBound, upperBound));
                tests.Add(randomA.Next(lowerBound, upperBound) == randomB.Next(lowerBound, upperBound));

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Random tests");
        }

        [Test]
        public void InitializerTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            bool exampleBoolA = true;
            bool exampleBoolB = false;
        
            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>() { true, exampleBoolA, !exampleBoolB };

                List<int> listTests = new List<int>() {1, 2, 3, 4, 5};

                tests.Add(listTests.Count == 5);
                tests.Add(listTests[2] == 3);

                Queue<double> queueTests = new Queue<double>() { 1, 2.0, 3.5 };
                tests.Add(queueTests.Count == 3);
                tests.Add(queueTests.Dequeue() == 1.0);
                tests.Add(queueTests.Dequeue() == 2.0);
                tests.Add(queueTests.Dequeue() == 3.5);

                Stack<double> stackTests = new Stack<double>() { 3.5, 2.0, 1 };
                tests.Add(stackTests.Count == 3);
                tests.Add(stackTests.Pop() == 1);
                tests.Add(stackTests.Pop() == 2.0);
                tests.Add(stackTests.Pop() == 3.5);

                RingBuffer<double> ringBufferTests = new RingBuffer<double>(5) { 1, 2, 3 };
                tests.Add(ringBufferTests.Count == 3);
                tests.Add(ringBufferTests.Pop() == 3);
                tests.Add(ringBufferTests.PopBack() == 1);
                tests.Add(ringBufferTests.PeekHead() == 2);
                tests.Add(ringBufferTests.PeekTail() == 2);

                DepletableBag<double> depletableBagTests = new DepletableBag<double>() { 1, 2, 3 };
                tests.Add(depletableBagTests.Count == 3);

                DepletableList<string> depletableListTests = new DepletableList<string>() {
                    ""first"", ""second"", ""third""
                };
                tests.Add(depletableListTests.Count == 3);
                tests.Add(depletableListTests.PopNext() == ""first"");

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Initializer tests");
        }

        [Test]
        public void DepletableTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                DepletableBag<double> depletableBagTests = new DepletableBag<double>();
                depletableBagTests.Add(1);
                depletableBagTests.Add(2);
                depletableBagTests.Add(3);
                tests.Add(depletableBagTests.Count == 3);

                double nextValue = depletableBagTests.PopNext();
                tests.Add(nextValue == 1 || nextValue == 2 || nextValue == 3);

                DepletableList<string> depletableListTests = new DepletableList<string>() {
                    ""first"", ""second"", ""third""
                };
                tests.Add(depletableListTests.Count == 3);
                tests.Add(depletableListTests.PopNext() == ""first"");

                Random randomTest1 = new Random(100);
                Random randomTest2 = new Random(100);

                DepletableBag<int> depletableBag1 = new DepletableBag<int>(randomTest1) {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
                DepletableBag<int> depletableBag2 = new DepletableBag<int>(randomTest2) {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

                while (depletableBag1.Count > 0)
                {
                    tests.Add(depletableBag1.PopNext() == depletableBag2.PopNext());
                }

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Depletable tests");
        }

        [Test]
        public void ForEachTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                List<int> items = new List<int>() {1, 2, 3, 4, 5};

                for (int i = 6; i <= 10; i++)
                {
                    items.Add(i);
                }

                int index = 0;
                foreach(int item in items)
                {
                    tests.Add(item == ++index);
                }

                index = 0;
                foreach(int item in new List<int>() {1, 2, 3, 4, 5})
                {
                    tests.Add(item == ++index);
                }
                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} ForEach tests");
        }

        [Test]
        public void ListAssignmentAndNullTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                List<int> items = new List<int>() {1, 2, 3, 4, 5};
                List<int> newItems = null;
                List<int> newItems2;
                List<int> newItems3 = new List<int>(items);

                tests.Add(newItems == newItems2);

                tests.Add(newItems == null);
                tests.Add(newItems != items);

                newItems = items;

                tests.Add(newItems != null);
                tests.Add(newItems == items);
                tests.Add(newItems[0] == 1);
                tests.Add(newItems[4] == 5);
        
                items.Clear();
                tests.Add(newItems.Count == 0);

                tests.Add(newItems3[0] == 1);
                tests.Add(newItems3[4] == 5);

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} List Assignment And Null tests");
        }

        [Test]
        public void RecursionTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            int testValue = 0;

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                tests.Add(FibonacciNumber(-1) == 0);
                tests.Add(FibonacciNumber(0) == 1);
                tests.Add(FibonacciNumber(1) == 1);
                tests.Add(FibonacciNumber(2) == 2);
                tests.Add(FibonacciNumber(3) == 3);
                tests.Add(FibonacciNumber(4) == 5);
                tests.Add(FibonacciNumber(5) == 8);
                tests.Add(FibonacciNumber(6) == 13);


                tests.Add(testValue == 0);

                IncrementBy(10);
                tests.Add(testValue == 10);
                IncrementBy(10);
                tests.Add(testValue == 20);
                IncrementBy(FibonacciNumber(6));
                tests.Add(testValue == 33);
                IncrementBy(FibonacciNumber(FibonacciNumber(4)));
                tests.Add(testValue == 41);

                return tests;
            }

            int FibonacciNumber(int index)
            {
                if (index < 0)
                    return 0;
                if (index < 2)
                    return 1;

                return FibonacciNumber(index - 1) + FibonacciNumber(index - 2);
            }
            
            void IncrementBy(int value)
            {
                testValue += value;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)),
                    new FunctionSignature(
                        identifier: "FibonacciNumber",
                        returnType: typeof(int),
                        arguments: new ArgumentData("index", typeof(int))));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            //int fib25 = script.ExecuteFunction<int>("FibonacciNumber", context, 25);
            //Debug.Log($"25th fibonacci number: {fib25}");

            Debug.Log($"Ran {tests.Count} Recursion tests");
        }

        [Test]
        public void DictionaryRecursionTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            int testValue = 0;
            Dictionary<int,int> cachedValues = new Dictionary<int,int>();

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                tests.Add(FibonacciNumber(-1) == 0);
                tests.Add(FibonacciNumber(0) == 1);
                tests.Add(FibonacciNumber(1) == 1);
                tests.Add(FibonacciNumber(2) == 2);
                tests.Add(FibonacciNumber(3) == 3);
                tests.Add(FibonacciNumber(4) == 5);
                tests.Add(FibonacciNumber(5) == 8);
                tests.Add(FibonacciNumber(6) == 13);


                tests.Add(testValue == 0);

                IncrementBy(10);
                tests.Add(testValue == 10);
                IncrementBy(10);
                tests.Add(testValue == 20);
                IncrementBy(FibonacciNumber(6));
                tests.Add(testValue == 33);
                IncrementBy(FibonacciNumber(FibonacciNumber(4)));
                tests.Add(testValue == 41);

                return tests;
            }

            int FibonacciNumber(int index)
            {
                if (index < 0) return 0;
                if (index < 2) return 1;

                if (!cachedValues.ContainsKey(index))
                {
                    cachedValues.Add(index, FibonacciNumber(index - 1) + FibonacciNumber(index - 2));
                }

                return cachedValues[index];
            }
            
            void IncrementBy(int value)
            {
                testValue += value;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)),
                    new FunctionSignature(
                        identifier: "FibonacciNumber",
                        returnType: typeof(int),
                        arguments: new ArgumentData("index", typeof(int))));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);
            int fib25 = script.ExecuteFunction<int>("FibonacciNumber", context, 25);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"25th fibonacci number: {fib25}");

            Debug.Log($"Ran {tests.Count} Dictionary Recursion tests");
        }

        [Test]
        public void DictionaryTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            int testValue = 0;
            Dictionary<string,double> map = new Dictionary<string,double>();

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                map.Add(""A"",0);
                map.Add(""B"",1);
                map.Add(""C"",2.1);
                map.Add(""D"",3);
                map.Add(""E"",4);
                map.Add(""F"",5);
                map.Add(""G"",6);
                map.Add(""H"",7);
                map.Add(""Trevor"",99);


                tests.Add(map.Count == 9);
                tests.Add(map.ContainsKey(""Trevor""));
                tests.Add(!map.ContainsKey(""I""));
                tests.Add(map[""B""] == 1);
                tests.Add(map[""Trevor""] == 99);
                tests.Add(map.Remove(""Trevor""));
                tests.Add(!map.Remove(""Trevor""));
                tests.Add(!map.ContainsKey(""Trevor""));
                tests.Add(map.ContainsValue(2.1));
                tests.Add(!map.ContainsValue(2));

                Queue<string> keys = new Queue<string>(map.Keys);
                Queue<double> values = new Queue<double>(map.Values);

                foreach(string key in map.Keys)
                {
                    tests.Add(keys.Dequeue() == key);
                }

                foreach(double value in map.Values)
                {
                    tests.Add(values.Dequeue() == value);
                }

                tests.Add(keys.Count == 0);
                tests.Add(values.Count == 0);

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} Dictionary tests");
        }

        [Test]
        public void HashSetTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                List<int> startingFactors = new List<int>() {2, 2, 2, 2, 3, 3, 3, 17};
                int number = 1;
                List<int> calculatedFactors = new List<int>();
                HashSet<int> testedNumbers = new HashSet<int>();

                //Multiply factors together
                foreach(int factor in startingFactors)
                {
                    number *= factor;
                }

                int nextFactor;
                while (number > 1)
                {
                    nextFactor = Factorize(number, testedNumbers);
                    if (nextFactor > 0)
                    {
                        calculatedFactors.Add(nextFactor);
                        number /= nextFactor;
                    }
                }

                tests.Add(calculatedFactors.Count == startingFactors.Count);

                for (int i = 0; i < calculatedFactors.Count; i++)
                {
                    tests.Add(calculatedFactors[i] == startingFactors[i]);
                }

                return tests;
            }


            int Factorize(int number, HashSet<int> testedNumbers)
            {
                int factorToTest = 2;
                while (testedNumbers.Contains(factorToTest))
                {
                    factorToTest++;
                }

                if (number % factorToTest == 0)
                    return factorToTest;

                testedNumbers.Add(factorToTest);
                return 0;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} HashSet tests");
        }

        [Test]
        public void ConstTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            const int testInt = 100;
            const int otherTestInt = testInt + 100;
            const double testDouble = 100.0;

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
            
                tests.Add(testInt == 100);
                tests.Add(otherTestInt == 200);
                tests.Add(testDouble == 100.0);

                if (testInt == 100)
                {
                    tests.Add(true);
                }
                else
                    tests.Add(false);

                const int otherTest = 20;

                tests.Add(otherTest == 20);

                tests.Add(5 * otherTest == testInt);

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} const tests");
        }

        [Test]
        public void GlobalDelcarationErrorTest()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            //Initialized with integer literal
            global double testDouble = 25;
            const double testConstDouble = 50;
            double localDouble = 75;

            void RunTest()
            {
                double testA = testDouble + 1.0;
                double testB = testDouble + 1;
                double testC = testConstDouble + 1;
                double testD = testConstDouble + 1.0;
                double testE = localDouble + 1;
                double testF = localDouble + 1.0;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTest",
                        returnType: typeof(void)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            script.ExecuteFunction("RunTest", context);
        }

        [Test]
        public void ConstantEqualityTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            int RunTests()
            {
                if ( 1.0 != 1 )
                {
                    return 1;
                }

                if ( 1.0 == 1 )
                {
                    //Continue
                }
                else
                {
                    return 2;
                }

                return 0;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(int)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            int failedTest = script.ExecuteFunction<int>("RunTests", context);

            Debug.Assert(failedTest == 0, $"Failed test {failedTest}");
        }

        void RunTests()
        {
            DataFile dataFile = new DataFile("asdf" + "_ % T");
            dataFile.AddField("UserName");
            dataFile.AddField("FirstField");
            dataFile.AddField("SecondField");
            dataFile.AddField("ThirdField");

            dataFile.AddValue("FirstField", "Here is an example Value");
            dataFile.AddValue("UserName", "The First User");
            dataFile.AddValue("ThirdField", "" + 5);

            dataFile.Save();

            dataFile.NextRecord();

            dataFile.AddValue("UserName", "The Second User");
            dataFile.AddValue("FirstField", "Testerson");
            dataFile.AddValue("SecondField", "" + 10);
            dataFile.AddValue("ThirdField", "" + 6);

            dataFile.NextRecord();
            dataFile.Save();
        }


        [Test]
        public void DataFileTestsA()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            void RunTests()
            {
                DataFile dataFile = new DataFile(User.GetUserName() + ""_%T"");

                dataFile.AddField(""UserName"");
                dataFile.AddField(""FirstField"");
                dataFile.AddField(""SecondField"");
                dataFile.AddField(""ThirdField"");

                dataFile.AddValue(""FirstField"", ""Here is an example Value"");
                dataFile.AddValue(""UserName"", ""The First User"");
                dataFile.AddValue(""ThirdField"", """" + 5);

                dataFile.Save();

                dataFile.NextRecord();

                dataFile.AddValue(""UserName"", ""The Second User"");
                dataFile.AddValue(""FirstField"", ""Testerson"");
                dataFile.AddValue(""SecondField"", """" + 10);
                dataFile.AddValue(""ThirdField"", """" + 6);

                dataFile.NextRecord();
                dataFile.Save();
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(void)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);
            script.ExecuteFunction("RunTests", context);
        }

        [Test]
        public void DataFileTestsB()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();

                List<string> fieldNames = new List<string>() { ""UserName"", ""FirstField"", ""SecondField"", ""ThirdField"" };
                DataFile dataFile = new DataFile(""Special Test File"", fieldNames, ""|"", ""\n"", false);

                dataFile.AddValue(""FirstField"", ""Here is an example Value"");
                dataFile.AddValue(""UserName"", ""The First User"");
                dataFile.AddValue(""ThirdField"", """" + 5);

                dataFile.Save();

                dataFile.NextRecord();

                dataFile.AddValue(""UserName"", ""The Second User"");
                dataFile.AddValue(""FirstField"", ""Testerson"");
                dataFile.AddValue(""SecondField"", """" + 10);
                dataFile.AddValue(""ThirdField"", """" + 6);

                dataFile.NextRecord();
                dataFile.Save();

                dataFile = null;
                fieldNames = null;

                fieldNames = new List<string>() { ""UserName"", ""FirstField"", ""ThirdField"", ""FourthField"" };
                dataFile = new DataFile(""Special Test File"", fieldNames, ""|"", ""\n"", true);

                dataFile.SetRecordNumber(0);

                tests.Add(dataFile.GetValue(""UserName"") == ""The First User"");
                tests.Add(dataFile.GetValue(1) == ""Here is an example Value"");
                tests.Add(dataFile.GetValue(2) == """");
                tests.Add(dataFile.GetValue(3) == ""5"");

                dataFile.SetRecordNumber(1);
                tests.Add(dataFile.GetValue(0) == ""The Second User"");
                tests.Add(dataFile.GetValue(""FirstField"") == ""Testerson"");
                tests.Add(dataFile.GetValue(""SecondField"") == ""10"");
                tests.Add(dataFile.GetValue(2) == ""10"");
                tests.Add(dataFile.GetValue(3) == ""6"");

                dataFile.AddValue(1, ""Overwritten Value"");
                dataFile.AddValue(""FourthField"", ""New Value"");
                dataFile.NextRecord();

                dataFile.AddValue(""UserName"", ""The Third User"");
                dataFile.AddValue(""FirstField"", ""Testerson 2"");
                dataFile.AddValue(""SecondField"", """" + 1);

                dataFile.UpdateFileName(""A New Test File"");

                dataFile.Save();

                return tests;
            }";

            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(
                    script: testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} List tests");
        }

        [Test]
        public void ToStringTests()
        {
            GlobalRuntimeContext globalContext = new GlobalRuntimeContext();

            string testScript = @"
            double testValue = 1054.32179;

            List<bool> RunTests()
            {
                List<bool> tests = new List<bool>();
                tests.Add(testValue.ToString(""E"") == ""1.054322E+003"");
                tests.Add(testValue.ToString(""E0"") == ""1E+003"");
                tests.Add(testValue.ToString(""E1"") == ""1.1E+003"");

                tests.Add(testValue.ToString(""e"") == ""1.054322e+003"");
                tests.Add(testValue.ToString(""e0"") == ""1e+003"");
                tests.Add(testValue.ToString(""e1"") == ""1.1e+003"");

                tests.Add((1054.32179).ToString(""F"") == ""1054.32"");
                tests.Add(testValue.ToString(""F0"") == ""1054"");
                tests.Add(testValue.ToString(""F1"") == ""1054.3"");

                tests.Add(testValue.ToString(""N"") == ""1,054.32"");
                tests.Add(testValue.ToString(""N0"") == ""1,054"");
                tests.Add(testValue.ToString(""N1"") == ""1,054.3"");

                return tests;
            }";


            Script script;

            try
            {
                script = ScriptParser.LexAndParseScript(testScript,
                    new FunctionSignature(
                        identifier: "RunTests",
                        returnType: typeof(List<bool>)));
            }
            catch (ScriptParsingException parseEx)
            {
                throw new Exception(
                    message: $"Parsing exception on Line {parseEx.line}, Column {parseEx.column}: {parseEx.Message}",
                    innerException: parseEx);
            }

            ScriptRuntimeContext context = script.PrepareScript(globalContext);

            List<bool> tests = script.ExecuteFunction<List<bool>>("RunTests", context);

            for (int i = 0; i < tests.Count; i++)
            {
                Debug.Assert(tests[i], $"Failed test {i}");
            }

            Debug.Log($"Ran {tests.Count} ToString tests");
        }
    }
}
