#if TEST_ENABLED
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using SRDebugger.UI.Controls;
using SRF;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Base class for custom tabs that use [Category] and [DisplayName] attributes like SROptions.
    /// Extend this class and add properties/methods with attributes - they will render in their own tab.
    /// </summary>
    public abstract class OptionsTab : DebugTab
    {
        public sealed override IEnumerable<DebugTabSection> GetSections()
        {
            yield break;
        }

        /// <summary>
        /// Gets all option definitions from this class using reflection.
        /// </summary>
        internal List<OptionDefinition> GetOptions()
        {
            var options = new List<OptionDefinition>();
            var type = GetType();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var displayAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
                if (displayAttr == null)
                    continue;

                var category = prop.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Default";

                options.Add(new OptionDefinition
                {
                    Name = displayAttr.DisplayName,
                    Category = category,
                    PropertyInfo = prop,
                    Target = this,
                    IsMethod = false
                });
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (method.GetParameters().Length > 0 || method.ReturnType != typeof(void))
                    continue;
                if (method.IsSpecialName)
                    continue;

                var displayAttr = method.GetCustomAttribute<DisplayNameAttribute>();
                if (displayAttr == null)
                    continue;

                var category = method.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Default";

                options.Add(new OptionDefinition
                {
                    Name = displayAttr.DisplayName,
                    Category = category,
                    MethodInfo = method,
                    Target = this,
                    IsMethod = true
                });
            }

            return options;
        }
    }

    internal class OptionDefinition
    {
        public string Name;
        public string Category;
        public PropertyInfo PropertyInfo;
        public MethodInfo MethodInfo;
        public object Target;
        public bool IsMethod;

        public Type ValueType => PropertyInfo?.PropertyType;
        public bool CanWrite => PropertyInfo?.CanWrite ?? false;

        public object GetValue() => PropertyInfo?.GetValue(Target);
        public void SetValue(object value) => PropertyInfo?.SetValue(Target, value);
        public void Invoke() => MethodInfo?.Invoke(Target, null);
    }
}
#endif
