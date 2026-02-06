using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BGC.Procedures
{
    /// <summary>
    /// Validates that procedure state types are correctly structured for serialization.
    /// </summary>
    public static class StateValidator
    {
        private static readonly HashSet<Type> AllowedPrimitives = new HashSet<Type>
        {
            typeof(bool),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(string),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid)
        };

        /// <summary>
        /// Validates the state type and throws if invalid.
        /// </summary>
        public static void ValidateOrThrow<TState>() where TState : ProcedureState
        {
            var errors = Validate(typeof(TState));
            if (errors.Count > 0)
            {
                throw new StateValidationException(typeof(TState), errors);
            }
        }

        /// <summary>
        /// Validates the state type and returns a list of errors (empty if valid).
        /// </summary>
        public static List<string> Validate(Type stateType)
        {
            var errors = new List<string>();

            // Must inherit from ProcedureState
            if (!typeof(ProcedureState).IsAssignableFrom(stateType))
            {
                errors.Add($"{stateType.Name} must inherit from ProcedureState");
                return errors; // Can't continue validation
            }

            // Must be a record (check for compiler-generated Clone method)
            if (!IsRecord(stateType))
            {
                errors.Add($"{stateType.Name} must be a record type");
            }

            // Check all public instance properties
            var properties = stateType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                // Skip inherited ProcedureState properties
                if (prop.DeclaringType == typeof(ProcedureState))
                {
                    continue;
                }

                ValidateProperty(prop, errors, stateType.Name);
            }

            return errors;
        }

        private static void ValidateProperty(PropertyInfo prop, List<string> errors, string typeName)
        {
            var propType = prop.PropertyType;

            // Must have a getter
            if (!prop.CanRead)
            {
                errors.Add($"{typeName}.{prop.Name}: Must have a getter");
            }

            // Must have an init or set accessor for deserialization
            if (!prop.CanWrite)
            {
                errors.Add($"{typeName}.{prop.Name}: Must have an init or set accessor");
            }

            // Check for mutable collections
            if (IsMutableCollection(propType))
            {
                errors.Add($"{typeName}.{prop.Name}: Use IReadOnlyList<T> or array instead of {propType.Name}");
            }

            // Check type is serializable
            if (!IsSerializableType(propType))
            {
                errors.Add($"{typeName}.{prop.Name}: Type {propType.Name} is not supported for serialization");
            }
        }

        private static bool IsSerializableType(Type type)
        {
            // Primitives and known types
            if (AllowedPrimitives.Contains(type))
            {
                return true;
            }

            // Enums
            if (type.IsEnum)
            {
                return true;
            }

            // Nullable<T> where T is allowed
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
            {
                return IsSerializableType(underlying);
            }

            // Arrays of allowed types
            if (type.IsArray)
            {
                return IsSerializableType(type.GetElementType());
            }

            // IReadOnlyList<T> where T is allowed
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();

                if (genericDef == typeof(IReadOnlyList<>))
                {
                    return IsSerializableType(type.GetGenericArguments()[0]);
                }

                if (genericDef == typeof(IReadOnlyCollection<>))
                {
                    return IsSerializableType(type.GetGenericArguments()[0]);
                }

                if (genericDef == typeof(IReadOnlyDictionary<,>))
                {
                    var args = type.GetGenericArguments();
                    return IsSerializableType(args[0]) && IsSerializableType(args[1]);
                }
            }

            // Records (nested state types)
            if (IsRecord(type))
            {
                return true;
            }

            return false;
        }

        private static bool IsMutableCollection(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            var genericDef = type.GetGenericTypeDefinition();
            return genericDef == typeof(List<>) ||
                   genericDef == typeof(Dictionary<,>) ||
                   genericDef == typeof(HashSet<>);
        }

        private static bool IsRecord(Type type)
        {
            // Records have a compiler-generated <Clone>$ method
            return type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) != null;
        }
    }

    /// <summary>
    /// Exception thrown when state validation fails.
    /// </summary>
    public class StateValidationException : Exception
    {
        public Type StateType { get; }
        public IReadOnlyList<string> Errors { get; }

        public StateValidationException(Type stateType, List<string> errors)
            : base($"State type {stateType.Name} is invalid:\n- {string.Join("\n- ", errors)}")
        {
            StateType = stateType;
            Errors = errors;
        }
    }
}