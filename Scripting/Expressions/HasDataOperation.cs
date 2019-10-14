using BGC.Users;
using System;

namespace BGC.Scripting
{
    public class HasDataOperation : IValueGetter
    {
        private readonly IValueGetter keyArg;

        public HasDataOperation(
            IValueGetter keyArg,
            Token source)
        {
            if (keyArg.GetValueType() != typeof(string))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Key argument of User.HasData must be a string: type {keyArg.GetValueType().Name}");
            }

            this.keyArg = keyArg;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve value of User.HasData as type {typeof(T).Name}");
            }

            return (T)(object)PlayerData.HasKey(keyArg.GetAs<string>(context));
        }

        public Type GetValueType() => typeof(bool);
    }
}
