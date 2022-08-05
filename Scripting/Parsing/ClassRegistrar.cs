using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BGC.DataStructures.Generic;

namespace BGC.Scripting.Parsing
{
    public static partial class ClassRegistrar
    {
        public delegate IExpression MemberExpression(IValueGetter value, Token source);
        public delegate IExpression MethodExpression(IValueGetter value, IValueGetter[] args, Token source);
        public delegate IExpression StaticExpression(Token source);
        public delegate IExpression StaticMethodExpression(IValueGetter[] args, Token source);

        private static readonly Dictionary<string, Type> aliasLookup = new Dictionary<string, Type>();

        private static readonly Dictionary<Type, IRegistration> classLookup = new Dictionary<Type, IRegistration>();

        static ClassRegistrar()
        {
            TryRegisterClass(typeof(object), "object");
            TryRegisterClass(typeof(bool), "bool");
            TryRegisterClass(typeof(byte), "byte");
            TryRegisterClass(typeof(sbyte), "sbyte");
            TryRegisterClass(typeof(short), "short");
            TryRegisterClass(typeof(ushort), "ushort");
            TryRegisterClass(typeof(int), "int");
            TryRegisterClass(typeof(uint), "uint");
            TryRegisterClass(typeof(long), "long");
            TryRegisterClass(typeof(ulong), "ulong");
            TryRegisterClass(typeof(float), "float");
            TryRegisterClass(typeof(double), "double");
            TryRegisterClass(typeof(decimal), "decimal");
            TryRegisterClass(typeof(char), "char");

            TryRegisterClass(typeof(string), "string");

            TryRegisterClass(typeof(DateTime));
            TryRegisterClass(typeof(DateTimeOffset));
            TryRegisterClass(typeof(TimeSpan));

            TryRegisterClass(typeof(Random));
            TryRegisterClass(typeof(Math));
            TryRegisterClass(typeof(Mathematics.GeneralMath), "GeneralMath");

            TryRegisterClass(typeof(IList));
            TryRegisterClass(typeof(IEnumerable));

            TryRegisterClass(typeof(IEnumerable<>));
            TryRegisterClass(typeof(IDepletable<>));
            TryRegisterClass(typeof(IList<>));

            TryRegisterClass(typeof(List<>));
            TryRegisterClass(typeof(Queue<>));
            TryRegisterClass(typeof(Stack<>));
            TryRegisterClass(typeof(HashSet<>));
            TryRegisterClass(typeof(Dictionary<,>));
            TryRegisterClass(typeof(DepletableList<>));
            TryRegisterClass(typeof(DepletableBag<>));
            TryRegisterClass(typeof(RingBuffer<>));

            //BGC-Specific
            TryRegisterClass(typeof(Reports.DataFile));
            TryRegisterClass(typeof(Members.SystemAdapter), "System");
            TryRegisterClass(typeof(Members.UserAdapter), "User");
            TryRegisterClass(typeof(Members.AudiometryAdapter), "Audiometry");
            TryRegisterClass(typeof(Members.DebugAdapter), "Debug");
        }


        public static bool TryRegisterClass<T>(string registerAs = "", bool limited = false) =>
            TryRegisterClass(typeof(T), registerAs, limited);

        public static bool TryRegisterClass(
            Type type,
            string registerAs = "",
            bool limited = false)
        {
            if (classLookup.ContainsKey(type))
            {
                return false;
            }

            if (string.IsNullOrEmpty(registerAs))
            {
                registerAs = type.Name;

                if (registerAs.Contains('`'))
                {
                    registerAs = registerAs[0..registerAs.IndexOf('`')];
                }
            }

            if (aliasLookup.ContainsKey(registerAs))
            {
                return false;
            }

            aliasLookup.Add(registerAs, type);

            if (type.IsEnum)
            {
                classLookup.Add(type, new EnumRegistration(type));
            }
            else
            {
                classLookup.Add(type, new ClassRegistration(type, !limited));
            }

            return true;
        }

        public static Type LookUpClass(string className) => aliasLookup.GetValueOrDefault(className);

        public static IEnumerable<(IRegistration registration, Type[] genericArguments)> GetRegisteredClasses(this Type type)
        {
            foreach (Type baseClass in type.GetTypes())
            {
                if (classLookup.TryGetValue(baseClass, out IRegistration registration))
                {
                    yield return (registration, null);
                }

                if (baseClass.IsGenericType)
                {
                    if (classLookup.TryGetValue(baseClass.GetGenericTypeDefinition(), out registration))
                    {
                        yield return (registration, baseClass.GetGenericArguments());
                    }
                }
            }
        }

        public static IExpression GetMemberExpression(
            IValueGetter value,
            string memberName,
            Token source)
        {
            foreach ((IRegistration registration, Type[] genericClassArguments) in value.GetValueType().GetRegisteredClasses())
            {
                if (registration.GetPropertyExpression(value, genericClassArguments, memberName, source) is IExpression propertyExpression)
                {
                    return propertyExpression;
                }
            }

            return null;
        }

        public static IExpression GetMethodExpression(
            IValueGetter value,
            Type[] genericMethodArguments,
            InvocationArgument[] args,
            string methodName,
            Token source)
        {
            foreach ((IRegistration registration, Type[] genericClassArguments) in value.GetValueType().GetRegisteredClasses())
            {
                if (registration.GetMethodExpression(value, genericClassArguments, genericMethodArguments, args, methodName, source) is IExpression methodExpression)
                {
                    return methodExpression;
                }
            }

            return null;
        }

        public static IExpression GetStaticMethodExpression(
            Type type,
            Type[] genericMethodArguments,
            InvocationArgument[] args,
            string methodName,
            Token source)
        {
            foreach ((IRegistration registration, Type[] genericClassArguments) in type.GetRegisteredClasses())
            {
                if (registration.GetStaticMethodExpression(genericClassArguments, genericMethodArguments, args, methodName, source) is IExpression methodExpression)
                {
                    return methodExpression;
                }
            }

            return null;
        }

        public static IExpression GetStaticExpression(
            Type type,
            string propertyName,
            Token source)
        {
            foreach ((IRegistration registration, Type[] genericClassArguments) in type.GetRegisteredClasses())
            {
                if (registration.GetStaticPropertyExpression(genericClassArguments, propertyName, source) is IExpression propertyExpression)
                {
                    return propertyExpression;
                }
            }

            return null;
        }

        private static IEnumerable<Type> GetTypes(this Type type) => type
            .GetBaseTypesAndInterfaces()
            .Prepend(type)
            .Distinct();

        private static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            if (type.BaseType is null || type.BaseType == typeof(object))
            {
                return type.GetInterfaces();
            }

            return type.BaseType.GetBaseTypesAndInterfaces()
                .Prepend(type.BaseType)
                .Concat(type.GetInterfaces())
                .Distinct();
        }

        public interface IRegistration
        {
            Type ClassType { get; }

            //bool HasMember(string memberName);

            IExpression GetMethodExpression(
                IValueGetter value,
                Type[] genericClassArguments,
                Type[] genericMethodArguments,
                InvocationArgument[] args,
                string methodName,
                Token source);

            IExpression GetPropertyExpression(
                IValueGetter value,
                Type[] genericClassArguments,
                string propertyName,
                Token source);

            IExpression GetStaticMethodExpression(
                Type[] genericClassArguments,
                Type[] genericMethodArguments,
                InvocationArgument[] args,
                string methodName,
                Token source);

            IExpression GetStaticPropertyExpression(
                Type[] genericClassArguments,
                string propertyName,
                Token source);
        }

        //Used to block Stripping and AOT Code Generation from stripping these
        public static void UsedOnlyForAOTCodeGeneration()
        {
            new List<bool>();
            new List<bool>(1);
            new List<bool>(new[] { false });
            new List<double>();
            new List<double>(1);
            new List<double>(new[] { 1.0 });
            new List<int>();
            new List<int>(1);
            new List<int>(new[] { 1 });
            new List<string>();
            new List<string>(1);
            new List<string>(new[] { "" });

            new Queue<bool>();
            new Queue<bool>(1);
            new Queue<bool>(new[] { false });
            new Queue<double>();
            new Queue<double>(1);
            new Queue<double>(new[] { 1.0 });
            new Queue<int>();
            new Queue<int>(1);
            new Queue<int>(new[] { 1 });
            new Queue<string>();
            new Queue<string>(1);
            new Queue<string>(new[] { "" });

            new Stack<bool>();
            new Stack<bool>(1);
            new Stack<bool>(new[] { false });
            new Stack<double>();
            new Stack<double>(1);
            new Stack<double>(new[] { 1.0 });
            new Stack<int>();
            new Stack<int>(1);
            new Stack<int>(new[] { 1 });
            new Stack<string>();
            new Stack<string>(1);
            new Stack<string>(new[] { "" });

            new RingBuffer<bool>(1);
            new RingBuffer<bool>(new[] { false });
            new RingBuffer<double>(1);
            new RingBuffer<double>(new[] { 1.0 });
            new RingBuffer<int>(1);
            new RingBuffer<int>(new[] { 1 });
            new RingBuffer<string>(1);
            new RingBuffer<string>(new[] { "" });

            new DepletableBag<bool>();
            new DepletableBag<bool>(new[] { false });
            new DepletableBag<double>();
            new DepletableBag<double>(new[] { 1.0 });
            new DepletableBag<int>();
            new DepletableBag<int>(new[] { 1 });
            new DepletableBag<string>();
            new DepletableBag<string>(new[] { "" });

            new DepletableList<bool>();
            new DepletableList<bool>(new[] { false });
            new DepletableList<double>();
            new DepletableList<double>(new[] { 1.0 });
            new DepletableList<int>();
            new DepletableList<int>(new[] { 1 });
            new DepletableList<string>();
            new DepletableList<string>(new[] { "" });

            new Dictionary<bool, bool>();
            new Dictionary<bool, int>();
            new Dictionary<bool, double>();
            new Dictionary<bool, string>();

            new Dictionary<int, bool>();
            new Dictionary<int, int>();
            new Dictionary<int, double>();
            new Dictionary<int, string>();

            new Dictionary<double, bool>();
            new Dictionary<double, int>();
            new Dictionary<double, double>();
            new Dictionary<double, string>();

            new Dictionary<string, bool>();
            new Dictionary<string, int>();
            new Dictionary<string, double>();
            new Dictionary<string, string>();

            new HashSet<bool>();
            new HashSet<int>();
            new HashSet<double>();
            new HashSet<string>();

            new Reports.DataFile();
            new Reports.DataFile("test");
            new Reports.DataFile("test", new[] { " " }, ",", "\n", false);

            // Include an exception so we can be sure to know if this method is ever called.
            throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
        }
    }
}