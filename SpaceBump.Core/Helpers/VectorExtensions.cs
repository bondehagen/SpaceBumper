using System;
using System.Windows;

namespace SpaceBumper
{
    public static class VectorExtensions
    {
        public static double Dot(this Vector source, Vector vector)
        {
            return source.X * vector.X + source.Y * vector.Y;
        }

        public static Vector Clone(this Vector vector)
        {
            return new Vector(vector.X, vector.Y);
        }

        public static bool IsValid(this Vector vector)
        {
            return !double.IsNaN(vector.X + vector.Y);
        }

        public static double GetAngle(this Vector vector)
        {
            return Math.Atan2(vector.Y, vector.X);
        }
    }
}