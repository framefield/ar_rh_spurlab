using System;
using UnityEngine;

namespace ff.ar_rh_spurlab.Positioning
{
    public struct Vector3Double
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3Double(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public static Vector3Double operator +(Vector3Double a, Vector3Double b)
        {
            return new Vector3Double(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3Double operator -(Vector3Double a, Vector3Double b)
        {
            return new Vector3Double(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3Double operator *(Vector3Double a, double b)
        {
            return new Vector3Double(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3Double operator /(Vector3Double a, double b)
        {
            return new Vector3Double(a.X / b, a.Y / b, a.Z / b);
        }

        public static Vector3Double operator *(double b, Vector3Double a)
        {
            return new Vector3Double(a.X * b, a.Y * b, a.Z * b);
        }

        public static implicit operator Vector3(Vector3Double d) => new((float)d.X, (float)d.Y, (float)d.Z);
    }

    public static class GeoMath
    {
        public const double EarthMeanRadiusInKM = 6372.8;
        public const double EarthEquatorialRadiusInKM = 6378.137;
        public const double EarthFirstEccentricitySquared = 0.00669437999014;
        public const double DegreeToRadian = Math.PI / 180;
    }

    [Serializable]
    public struct GeoPosition
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;

        public GeoPosition(double latitude, double longitude, double altitude = 0d)
        {
            Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
        }

        public Vector3Double ToEcef()
        {
            var lat = Latitude * GeoMath.DegreeToRadian;
            var lon = Longitude * GeoMath.DegreeToRadian;

            const double a = GeoMath.EarthEquatorialRadiusInKM * 1000;
            const double e2 = GeoMath.EarthFirstEccentricitySquared;

            var latSin = Math.Sin(lat);
            var latCos = Math.Cos(lat);
            var lonSin = Math.Sin(lon);
            var lonCos = Math.Cos(lon);

            var n = a / Math.Sqrt(1 - e2 * Math.Pow(latSin, 2));

            var x = n * latCos * lonCos;
            var y = n * latCos * lonSin;
            var z = (1 - e2) * n * latSin;

            return new Vector3Double(x, y, z);
        }

        public Vector3Double CalculateEnu(GeoPosition l2)
        {
            var lat = Latitude * GeoMath.DegreeToRadian;
            var lon = Longitude * GeoMath.DegreeToRadian;

            var p1 = ToEcef();
            var p2 = l2.ToEcef();

            var delta = p2 - p1;

            var latSin = Math.Sin(lat);
            var latCos = Math.Cos(lat);
            var lonSin = Math.Sin(lon);
            var lonCos = Math.Cos(lon);

            var e = -lonSin * delta.X + lonCos * delta.Y;
            var n = -lonCos * latSin * delta.X - latSin * lonSin * delta.Y + latCos * delta.Z;

            return new Vector3Double(n, 0, e);
        }

        public bool Equal(GeoPosition a, GeoPosition b, double epsilon = 0.0000001)
        {
            return (Math.Abs(a.Latitude - b.Latitude) <= epsilon) &&
                   (Math.Abs(a.Longitude - b.Longitude) <= epsilon) &&
                   (Math.Abs(a.Altitude - b.Altitude) <= epsilon);
        }
    }
}
