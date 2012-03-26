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

        public static void Zero(this Vector vector)
        {
            vector.X = 0;
            vector.Y = 0;
        }

        public static Vector Clone(this Vector vector)
        {
            return new Vector(vector.X, vector.Y);
        }

        public static void Validate(this Vector vector)
        {
            if (double.IsNaN(vector.X + vector.Y))
                vector.Zero();
        }

        public static double GetAngle(this Vector vector)
        {
            return Math.Atan2(vector.Y, vector.X);
        }
    }
}