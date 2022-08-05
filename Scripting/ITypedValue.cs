using System;

namespace BGC.Scripting
{
    public interface ITypedValue
    {
        Type GetValueType();
    }
}