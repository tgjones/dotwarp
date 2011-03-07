using Nexus;

namespace DotWarp.Effects
{
	public abstract class Light
	{
		public bool Enabled { get; set; }
		public ColorRgbF Color { get; set; }

		protected Light()
		{
			Enabled = true;
		}
	}
}