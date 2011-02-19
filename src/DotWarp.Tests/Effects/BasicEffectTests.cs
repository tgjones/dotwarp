using DotWarp.Effects;
using DotWarp.Util;
using NUnit.Framework;
using SharpDX.Direct3D11;

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
			BasicEffect basicEffect = new BasicEffect(device, BasicEffectCode.GetCode());

			// Assert.
			Assert.IsNotNull(basicEffect);

			// Clean up.
			basicEffect.Dispose();
			device.Dispose();
		}
	}
}