using DotWarp.Effects;
using DotWarp.Util;
using NUnit.Framework;
using SharpDX.Direct3D10;

namespace DotWarp.Tests.Effects
{
	[TestFixture]
	public class BasicEffectTests
	{
		[Test]
		public void CanCreateBasicEffect()
		{
			// Arrange.
			Device device = DeviceUtility.CreateDevice();

			// Act.
			BasicEffect basicEffect = BasicEffect.Create(device);

			// Assert.
			Assert.IsNotNull(basicEffect);

			// Clean up.
			basicEffect.Dispose();
			device.Dispose();
		}
	}
}