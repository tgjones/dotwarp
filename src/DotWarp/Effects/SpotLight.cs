using Nexus;

namespace DotWarp.Effects
{
	public class SpotLight : PointLightBase
	{
		public Vector3D Direction { get; set; }
		public float InnerConeAngle { get; set; }
		public float OuterConeAngle { get; set; }
	}
}