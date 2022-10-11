using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BGC.Scripting.Parsing;

namespace BGC.Scripting
{
    public static class OperationUtility
    {
        public static object InvokeStaticMethod(this Type hostType, string methodName, params object[] parameters)
        {
            //Try to find it
            IEnumerable<MethodInfo> methodInfos = hostType
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(x => x.Name == methodName);

            if (!methodInfos.Any())
            {
                throw new ArgumentException($"Type {hostType.Name} has no static method {methodName}");
            }

            if (Type.DefaultBinder.SelectMethod(
                bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                match: methodInfos.ToArray(),
                types: parameters.Select(obj => obj.GetType()).ToArray(),
                modifiers: new[] { new ParameterModifier(parameters.Length) }) is not MethodInfo methodInfo)
            {
                throw new ArgumentException($"Type {hostType.Name} has no static method overload for {methodName} which takes types {string.Join(", ", parameters.Select(obj => obj.GetType().Name))}");
            }

            return methodInfo.Invoke(null, parameters);
        }

        public static (bool, string) CanInvokeStaticMethod(this Type hostType, string methodName, params Type[] parameterTypes)
        {
            //Try to find it
            IEnumerable<MethodInfo> methodInfos = hostType
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(x => x.Name == methodName);

            if (!methodInfos.Any())
            {
                return (false, $"Type {hostType.Name} has no static method {methodName}");
            }

            if (Type.DefaultBinder.SelectMethod(
                bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                match: methodInfos.ToArray(),
                types: parameterTypes,
                modifiers: new[] { new ParameterModifier(parameterTypes.Length) }) is not MethodInfo methodInfo)
            {
                return (false, $"Type {hostType.Name} has no static method overload for {methodName} which takes types {string.Join(", ", parameterTypes.Select(obj => obj.Name))}");
            }

            return (true, string.Empty);
        }
    }
}
