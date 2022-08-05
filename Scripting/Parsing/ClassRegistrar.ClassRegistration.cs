using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace BGC.Scripting.Parsing
{
    public static partial class ClassRegistrar
    {
        public class ClassRegistration : IRegistration
        {
            public Type ClassType { get; }

            private readonly bool fullRegistration;

            public ClassRegistration(
                Type type,
                bool fullRegistration)
            {
                ClassType = type;

                this.fullRegistration = fullRegistration;
            }

            #region IRegistration

            public IExpression GetMethodExpression(
                IValueGetter value,
                Type[] genericClassArguments,
                Type[] genericMethodArguments,
                InvocationArgument[] args,
                string methodName,
                Token source)
            {
                if (GetInvocationType(genericClassArguments) is not Type invocationType)
                {
                    //Can't construct concrete class
                    return null;
                }

                //Try to find it
                IEnumerable<MethodInfo> methodInfos = invocationType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.Name == methodName)
                    .Where(x => fullRegistration || x.GetCustomAttribute<ScriptingAccessAttribute>() is not null);

                if (!methodInfos.Any())
                {
                    return null;
                }

                MethodInfo selectedMethodInfo = SelectMethod(
                    methodInfos: methodInfos.ToArray(),
                    methodArguments: args,
                    genericMethodArguments: genericMethodArguments);

                if (selectedMethodInfo is null)
                {
                    return null;
                }

                return new RegisteredInstanceMethodOperation(
                    value: value,
                    args: args,
                    methodInfo: selectedMethodInfo,
                    source: source);
            }

            public IExpression GetPropertyExpression(
                IValueGetter value,
                Type[] genericClassArguments,
                string propertyName,
                Token source)
            {
                if (GetInvocationType(genericClassArguments) is not Type invocationType)
                {
                    //Can't construct concrete class
                    return null;
                }

                //Try to find it

                //Check Properties
                PropertyInfo propertyInfo = invocationType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo is not null && (fullRegistration || propertyInfo.GetCustomAttribute<ScriptingAccessAttribute>() is not null))
                {
                    if (propertyInfo.CanWrite)
                    {
                        return new RegisteredSettableInstancePropertyOperation(
                            value: value,
                            propertyInfo: propertyInfo,
                            source: source);
                    }
                    else
                    {
                        return new RegisteredGettableInstancePropertyOperation(
                            value: value,
                            propertyInfo: propertyInfo,
                            source: source);
                    }
                }

                //Check Fields
                FieldInfo fieldInfo = invocationType.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);

                if (fieldInfo is not null && (fullRegistration || fieldInfo.GetCustomAttribute<ScriptingAccessAttribute>() is not null))
                {
                    if (fieldInfo.IsInitOnly)
                    {
                        return new RegisteredGettableInstanceFieldOperation(
                            value: value,
                            fieldInfo: fieldInfo,
                            source: source);
                    }
                    else
                    {
                        return new RegisteredSettableInstanceFieldOperation(
                            value: value,
                            fieldInfo: fieldInfo,
                            source: source);
                    }
                }

                return null;
            }

            public IExpression GetStaticMethodExpression(
                Type[] genericClassArguments,
                Type[] genericMethodArguments,
                InvocationArgument[] args,
                string methodName,
                Token source)
            {
                if (GetInvocationType(genericClassArguments) is not Type invocationType)
                {
                    //Can't construct concrete class
                    return null;
                }

                //Try to find it
                IEnumerable<MethodInfo> methodInfos = invocationType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(x => x.Name == methodName)
                    .Where(x => fullRegistration || x.GetCustomAttribute<ScriptingAccessAttribute>() is not null);

                if (!methodInfos.Any())
                {
                    return null;
                }

                MethodInfo selectedMethodInfo = SelectMethod(
                    methodInfos: methodInfos.ToArray(),
                    methodArguments: args,
                    genericMethodArguments: genericMethodArguments);

                if (selectedMethodInfo is null)
                {
                    return null;
                }

                return new RegisteredStaticMethodOperation(
                    args: args,
                    methodInfo: selectedMethodInfo,
                    source: source);
            }

            public IExpression GetStaticPropertyExpression(
                Type[] genericClassArguments,
                string propertyName,
                Token source)
            {
                if (GetInvocationType(genericClassArguments) is not Type invocationType)
                {
                    //Can't construct concrete class
                    return null;
                }

                //Try to find it
                PropertyInfo propertyInfo = invocationType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (propertyInfo is not null && (fullRegistration || propertyInfo.GetCustomAttribute<ScriptingAccessAttribute>() is not null))
                {
                    if (propertyInfo.CanWrite)
                    {
                        return new RegisteredSettableStaticPropertyOperation(
                            propertyInfo: propertyInfo,
                            source: source);
                    }
                    else
                    {
                        return new RegisteredGettableStaticPropertyOperation(
                            propertyInfo: propertyInfo,
                            source: source);
                    }
                }

                FieldInfo fieldInfo = invocationType.GetField(propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (fieldInfo is not null && (fullRegistration || fieldInfo.GetCustomAttribute<ScriptingAccessAttribute>() is not null))
                {
                    if (fieldInfo.IsInitOnly)
                    {
                        return new RegisteredGettableStaticFieldOperation(
                            fieldInfo: fieldInfo,
                            source: source);
                    }
                    else
                    {
                        return new RegisteredSettableStaticFieldOperation(
                            fieldInfo: fieldInfo,
                            source: source);
                    }
                }

                return null;
            }

            #endregion IRegistration

            private Type GetInvocationType(Type[] genericClassArguments)
            {
                if (ClassType.ContainsGenericParameters)
                {
                    if (genericClassArguments is null || genericClassArguments.Length == 0)
                    {
                        //Can't construct concrete class
                        return null;
                    }

                    return ClassType.MakeGenericType(genericClassArguments);
                }

                return ClassType;
            }

            private static MethodInfo SelectMethod(
                MethodInfo[] methodInfos,
                InvocationArgument[] methodArguments,
                Type[] genericMethodArguments)
            {
                if (genericMethodArguments is null)
                {
                    //Test Non-Generic
                    if (Type.DefaultBinder.SelectMethod(
                        bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                        match: methodInfos,
                        types: methodArguments.GetEffectiveTypes(),
                        modifiers: methodArguments.CreateParameterModifiers()) is MethodInfo methodInfo)
                    {
                        return methodInfo;
                    }
                }
                else
                {
                    //Find Generic
                    if (Type.DefaultBinder.SelectMethod(
                        bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                        match: methodInfos
                            .Where(x => x.ContainsGenericParameters)
                            .Select(x => x.MakeGenericMethod(genericMethodArguments))
                            .ToArray(),
                        types: methodArguments.GetEffectiveTypes(),
                        modifiers: methodArguments.CreateParameterModifiers()) is MethodInfo methodInfo)
                    {
                        return methodInfo;
                    }
                }

                //In principle, here is where we would do generic type inferencing

                return null;
            }
        }
    }
}