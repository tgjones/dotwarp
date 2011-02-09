using DotWarp.Effects;
using NUnit.Framework;

namespace DotWarp.Tests.Effects
{
	[TestFixture]
	public class BasicEffectCodeTests
	{
		[Test]
		public void CanLoadCode()
		{
			// Act.
			string code = BasicEffectCode.GetCode();

			// Assert.
			Assert.IsNotNullOrEmpty(code);
		}
	}
}