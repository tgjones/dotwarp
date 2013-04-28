using Nexus;
using SharpDX;

namespace DotWarp.Util
{
	internal static class ConversionUtility
	{
		public static Matrix ToSharpDXMatrix(Matrix3D matrix)
		{
			return new Matrix
			{
				M11 = matrix.M11,
				M12 = matrix.M12,
				M13 = matrix.M13,
				M14 = matrix.M14,
				M21 = matrix.M21,
				M22 = matrix.M22,
				M23 = matrix.M23,
				M24 = matrix.M24,
				M31 = matrix.M31,
				M32 = matrix.M32,
				M33 = matrix.M33,
				M34 = matrix.M34,
				M41 = matrix.M41,
				M42 = matrix.M42,
				M43 = matrix.M43,
				M44 = matrix.M44,
			};
		}

		public static Vector3 ToSharpDXVector3(Point3D v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}

		public static Vector3 ToSharpDXVector3(Vector3D v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}

		public static Vector3 ToSharpDXVector3(ColorRgbF c)
		{
			return new Vector3(c.R, c.G, c.B);
		}

		public static Color4 ToSharpDXColor(Nexus.Color c)
		{
			return new Color4(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
		}
	}
}