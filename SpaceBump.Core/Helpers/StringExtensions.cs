using System;
using System.Linq;

namespace SpaceBumper
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNotNullOrEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        public static string Remove(this string source, params string[] strings)
        {
            return strings.Aggregate(source, (current, s) => current.Remove(s));
        }

        public static string Remove(this string source, string value)
        {
            return source.Replace(value, "");
        }

        public static string Format(this string @instance, params object[] args)
        {
            return instance.IsNullOrEmpty() || args == null
                       ? instance
                       : String.Format(instance, args);
        }
    }
}