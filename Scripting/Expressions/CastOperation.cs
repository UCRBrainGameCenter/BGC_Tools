using System;

namespace BGC.Scripting
{
    public class CastOperation : IValueGetter
    {
        private readonly IValueGetter arg;
        private readonly Type valueType;

        public static IExpression CreateCastOperation(
            IValueGetter arg,
            CastingOperationToken castingOperationToken)
        {
            return new CastOperation(arg, castingOperationToken.type);
        }

        private CastOperation(
            IValueGetter arg,
            Type valueType)
        {
            this.arg = arg;
            this.valueType = valueType;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying ({valueType}) as type {returnType.Name}");
            }

            object value = arg.GetAs<object>(context)!;
            value = CastAs(value, valueType);

            if (!returnType.IsAssignableFrom(valueType))
            {
                value = Convert.ChangeType(value, returnType);
            }

            return (T)value!;
        }

        public static object CastAs(object obj, Type type)
        {
            Type objType = obj.GetType();
            if (objType.IsPrimitive && type.IsPrimitive)
            {
                return CastPrimitive(obj, type);
            }

            return Convert.ChangeType(obj, type);
        }
        
        public static object CastPrimitive(object obj, Type type)
        {
            switch (obj)
            {
                case bool prim:
                    switch (type.FullName)
                    {
                        case "System.Boolean": return (bool)prim;
                    }
                    break;
                case byte prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case sbyte prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case short prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case ushort prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case int prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case uint prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case long prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case ulong prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case nint prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case nuint prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case char prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case decimal prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case float prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
                case double prim:
                    switch (type.FullName)
                    {
                        case "System.Byte": return (byte)prim;
                        case "System.SByte": return (sbyte)prim;
                        case "System.Int16": return (short)prim;
                        case "System.UInt16": return (ushort)prim;
                        case "System.Int32": return (int)prim;
                        case "System.UInt32": return (uint)prim;
                        case "System.Int64": return (long)prim;
                        case "System.UInt64": return (ulong)prim;
                        case "System.IntPtr": return (nint)prim;
                        case "System.UIntPtr": return (nuint)prim;
                        case "System.Char": return (char)prim;
                        case "System.Decimal": return (decimal)prim;
                        case "System.Single": return (float)prim;
                        case "System.Double": return (double)prim;
                    }
                    break;
            }
            throw new ArgumentException($"Cannot cast from {obj.GetType().FullName} to {type.FullName}");
        }

        public Type GetValueType() => valueType;
    }
}