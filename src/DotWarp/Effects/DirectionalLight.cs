using Nexus;

namespace DotWarp.Effects
{
	public class DirectionalLight
	{
		public bool Enabled { get; set; }
		public Vector3D Direction { get; set; }
		public ColorRgbF Color { get; set; }
	}
}