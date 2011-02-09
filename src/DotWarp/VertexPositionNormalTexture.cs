using System.Runtime.InteropServices;
using Nexus;
using SharpDX.Direct3D10;
using SharpDX.DXGI;

namespace DotWarp
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexPositionNormalTexture
	{
		public Point3D Position;
		public Vector3D Normal;
		public Point2D TextureCoordinate;

		public VertexPositionNormalTexture(Point3D position, Vector3D normal, Point2D textureCoordinate)
		{
			Position = position;
			Normal = normal;
			TextureCoordinate = textureCoordinate;
		}

		public static int SizeInBytes
		{
			get { return Point3D.SizeInBytes + Vector3D.SizeInBytes + Point2D.SizeInBytes; }
		}

		public static InputElement[] InputElements
		{
			get
			{
				return new[]
				{
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
					new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
					new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
				};
			}
		}
	}
}