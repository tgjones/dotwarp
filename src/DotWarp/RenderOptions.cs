using Nexus;

namespace DotWarp
{
	public class RenderOptions
	{
		public bool TriangleWindingOrderReversed { get; set; }
		public Color BackgroundColor { get; set; }
		public bool LightingEnabled { get; set; }

		public RenderOptions()
		{
			TriangleWindingOrderReversed = false;
			BackgroundColor = Colors.White;
			LightingEnabled = true;
		}
	}
}