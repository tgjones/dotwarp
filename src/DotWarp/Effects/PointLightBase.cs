using Nexus;

namespace DotWarp.Effects
{
	public class PointLightBase : Light
	{
		public float ConstantAttenuation { get; set; }
		public float LinearAttenuation { get; set; }
		public float QuadraticAttenuation { get; set; }
		public Point3D Position { get; set; }
		public float Range { get; set; }
	}
}