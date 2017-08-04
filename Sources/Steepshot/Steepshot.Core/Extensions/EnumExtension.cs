//using System;
//using System.ComponentModel;
//using System.Reflection;

//namespace Sweetshot.Library.Extensions
//{
//    public static class EnumExtension
//    {
//        public static string GetDescription(this Enum value)
//        {
//            var type = value.GetType();
//            var name = Enum.GetName(type, value);
//            if (name != null)
//            {
//                var field = type.GetRuntimeField(name);
//                if (field != null)
//                {
//                    var attr = field.GetCustomAttributes(typeof(DescriptionAttribute)) as DescriptionAttribute;
//                    if (attr != null)
//                        return attr.Description;
//                }
//            }

//            return string.Empty;
//        }
//    }
//}