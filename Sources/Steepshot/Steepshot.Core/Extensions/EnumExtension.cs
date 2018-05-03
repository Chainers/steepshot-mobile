using Steepshot.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Steepshot.Core.Extensions
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum value)
        {
            var attribute = value.GetType()
                .GetRuntimeField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .SingleOrDefault() as DisplayAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static List<string> FlagToStringList(this Enum value)
        {
            var list = new List<string>();
            var type = value.GetType();
            var allFields = type.GetFields();

            foreach (var field in allFields)
            {
                if (!field.IsStatic)
                    continue;

                var en = Enum.Parse(type, field.Name) as Enum;
                if (!value.HasFlag(en))
                    continue;

                if (field.GetCustomAttribute(typeof(EnumMemberAttribute), false) is EnumMemberAttribute attribute)
                    list.Add(attribute.Value);
            }
            return list;
        }
    }
}