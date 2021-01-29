using BGC.Users;
using System;

namespace BGC.Scripting
{
    public class GetUserNameOperation : IValueGetter
    {
        public GetUserNameOperation() { }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableFromType(typeof(string)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve value of User.GetUserName as type {typeof(T).Name}");
            }

            return (T)(object)PlayerData.UserName;
        }

        public Type GetValueType() => typeof(string);
    }
}
