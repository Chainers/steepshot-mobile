using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Steepshot.Core.Extensions
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            DisplayAttribute attribute = value.GetType()
                .GetRuntimeField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}