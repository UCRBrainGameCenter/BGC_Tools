using System;
using System.Collections.Generic;
using System.Reflection;

namespace BGC.Parameters
{
    /// <summary>
    /// Supplies the full, ordered selection/addition type list for a property at runtime.
    /// When a property carries an <see cref="AppendTypeProviderAttribute"/>, the provider
    /// replaces the static <see cref="AppendSelectionAttribute"/>/<see cref="AppendAdditionAttribute"/>
    /// lists as the source consumed by
    /// <see cref="PropertyGroupExtensions.GetSelectionTypes"/> and
    /// <see cref="PropertyGroupExtensions.GetListAdditionTypes"/> (and therefore by UI dropdowns,
    /// deserialization type matching, and default selection). A provider that wants to merge with
    /// the attribute-declared types can read them via
    /// <see cref="PropertyGroupExtensions.GetAttributeSelectionTypes"/> /
    /// <see cref="PropertyGroupExtensions.GetAttributeListAdditionTypes"/>.
    /// </summary>
    public interface IAppendTypeProvider
    {
        IEnumerable<Type> GetSelectionTypes(PropertyInfo property);

        IEnumerable<Type> GetListAdditionTypes(PropertyInfo property);
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AppendTypeProviderAttribute : Attribute
    {
        public readonly Type providerType;

        private IAppendTypeProvider cachedProvider;

        public AppendTypeProviderAttribute(Type providerType)
        {
            if (!typeof(IAppendTypeProvider).IsAssignableFrom(providerType))
            {
                throw new ArgumentException($"ProviderType must implement IAppendTypeProvider: {providerType}");
            }

            this.providerType = providerType;
        }

        public IAppendTypeProvider GetProvider() =>
            cachedProvider ??= (IAppendTypeProvider)Activator.CreateInstance(providerType);
    }
}
