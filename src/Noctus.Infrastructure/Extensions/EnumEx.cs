﻿using System;
using System.ComponentModel;
using System.Reflection;

namespace Noctus.Infrastructure.Extensions
{
    public static class EnumEx
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute
                = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                    as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
