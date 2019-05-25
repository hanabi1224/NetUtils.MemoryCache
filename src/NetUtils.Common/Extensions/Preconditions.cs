using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetUtils
{
    public static class Preconditions
    {
        public static T RequireNotNull<T>(this T obj, string parameterName)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (obj == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return obj;
        }

        public static IEnumerable<T> RequireNotNullOrEmpty<T>(this IEnumerable<T> obj, string parameterName)
        {
            return obj.RequireCondition(l => l?.Any() == true, parameterName, "Collection cannot be null or empty");
        }

        public static IEnumerable<T> RequireNotNullItems<T>(this IEnumerable<T> obj, string parameterName)
        {
            obj.RequireNotNull(nameof(obj));

            var message = $"An item of {parameterName} was null.";
            foreach (T item in obj)
            {
                item.RequireNotNull(message);
            }

            return obj;
        }

        public static IEnumerable<string> RequireNotNullOrWhiteSpaceStringItems(this IEnumerable<string> strColl, string parameterName)
        {
            strColl.RequireNotNull(nameof(strColl));

            var message = $"A string in {parameterName} was null or empty or whitespace only.";
            foreach (var str in strColl)
            {
                str.RequireNotNullOrWhiteSpace(message);
            }

            return strColl;
        }

        public static int RequireCondition(this int value, Func<int, bool> condition, string parameterName, string message)
        {
            condition.RequireNotNull(nameof(condition));
            parameterName.RequireNotNull(nameof(parameterName));

            if (!condition(value))
            {
                throw new ArgumentException(message, parameterName);
            }

            return value;
        }

        public static void RequireCondition(bool condition, string parameterName, string message)
        {
            parameterName.RequireNotNull(nameof(parameterName));
            parameterName.RequireNotNull(nameof(message));

            if (!condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        public static T RequireCondition<T>(this T obj, Func<T, bool> condition, string parameterName, string message)
        {
            condition.RequireNotNull(nameof(condition));
            parameterName.RequireNotNull(nameof(parameterName));
            obj.RequireNotNull(parameterName);

            if (!condition(obj))
            {
                throw new ArgumentException(message, parameterName);
            }

            return obj;
        }

        public static string RequireNotNullOrEmpty(this string obj, string parameterName)
        {
            if (string.IsNullOrEmpty(obj))
            {
                throw new ArgumentException("Parameter must not be null or empty.", parameterName);
            }

            return obj;
        }

        public static string RequireNotNullOrWhiteSpace(this string obj, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(obj))
            {
                throw new ArgumentException("Parameter must not be null or whitespace.", parameterName);
            }

            return obj;
        }

        public static float RequireNonNegative(this float value, string parameterName)
        {
            parameterName.RequireNotNull(nameof(parameterName));

            if (value < 0)
            {
                throw new ArgumentException("Parameter must not be negative.", parameterName);
            }

            return value;
        }

        public static double RequireNonNegative(this double value, string parameterName)
        {
            parameterName.RequireNotNull(nameof(parameterName));

            if (value < 0)
            {
                throw new ArgumentException("Parameter must not be negative.", parameterName);
            }

            return value;
        }

        public static float RequireNonNegative(this int value, string parameterName)
        {
            parameterName.RequireNotNull(nameof(parameterName));

            if (value < 0)
            {
                throw new ArgumentException("Parameter must not be negative.", parameterName);
            }

            return value;
        }

        public static int RequirePositive(this int value, string parameterName)
        {
            parameterName.RequireNotNull(nameof(parameterName));

            if (value <= 0)
            {
                throw new ArgumentException("Parameter must be positive.", parameterName);
            }

            return value;
        }

        public static double RequirePositive(this double value, string parameterName)
        {
            parameterName.RequireNotNull(nameof(parameterName));

            if (value <= 0)
            {
                throw new ArgumentException("Parameter must be positive.", parameterName);
            }

            return value;
        }

        public static T RequireNotNullNonPrimitivePublicInstanceTypes<T>(this T obj, string parameterName)
        {
            obj.RequireNotNull(nameof(obj));
            parameterName.RequireNotNull(nameof(parameterName));

            BindingFlags bindingFlags =
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.FlattenHierarchy;
            Type t = typeof(T);
            foreach (PropertyInfo propertyInfo in t.GetProperties(bindingFlags).Where(propertyInfo => !propertyInfo.PropertyType.IsPrimitive))
            {
                var propValue = propertyInfo.GetValue(obj);
                propValue.RequireNotNull($"{parameterName}.{propertyInfo.Name}");
            }

            foreach (FieldInfo fieldInfo in t.GetFields(bindingFlags).Where(fieldInfo => !fieldInfo.FieldType.IsPrimitive))
            {
                var propValue = fieldInfo.GetValue(obj);
                propValue.RequireNotNull($"{parameterName}.{fieldInfo.Name}");
            }

            return obj;
        }

        public static Enum RequireEnumDefined(this Enum obj, Type enumType, string parameterName)
        {
            parameterName.RequireNotNull(nameof(parameterName));
            if (!Enum.IsDefined(enumType, obj))
            {
                throw new ArgumentException($"enum is not defined in {enumType.Name}", parameterName);
            }
            return obj;
        }
    }
}
