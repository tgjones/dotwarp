using System;
using System.Runtime.InteropServices;
using DotWarp.Effects;
using DotWarp.Util;
using NUnit.Framework;
using SharpDX.D3DCompiler;
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

		[Test]
		public void VertexConstantBufferIsInExpectedLayout()
		{
			// Arrange.
			string effectCode = BasicEffectCode.GetCode();
			ShaderBytecode bytecode = EffectCompiler.CompileVertexShader(effectCode);

			// Act.
			ShaderReflection reflection = new ShaderReflection(bytecode);
			ConstantBuffer vertexConstantBuffer = reflection.GetConstantBuffer(0);

			// Assert.
			Assert.That(vertexConstantBuffer.Description.Size,
				Is.EqualTo(Marshal.SizeOf(typeof(BasicEffect.BasicEffectVertexConstants))));
			AssertFieldOffset<BasicEffect.BasicEffectVertexConstants>("WorldViewProjection", vertexConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectVertexConstants>("World", vertexConstantBuffer);

			// Clean up.
			reflection.Dispose();
			bytecode.Dispose();
		}

		[Test]
		public void PixelConstantBufferIsInExpectedLayout()
		{
			// Arrange.
			string effectCode = BasicEffectCode.GetCode();
			ShaderBytecode bytecode = EffectCompiler.CompilePixelShader(effectCode);

			// Act.
			ShaderReflection reflection = new ShaderReflection(bytecode);
			ConstantBuffer pixelConstantBuffer = reflection.GetConstantBuffer(0);
			
			// Assert.
			Assert.That(pixelConstantBuffer.Description.Size,
				Is.EqualTo(Marshal.SizeOf(typeof (BasicEffect.BasicEffectPixelConstants))));
			AssertFieldOffset<BasicEffect.BasicEffectPixelConstants>("CameraPosition", pixelConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectPixelConstants>("LightingEnabled", pixelConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectPixelConstants>("AmbientLightColor", pixelConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectPixelConstants>("DiffuseColor", pixelConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectPixelConstants>("SpecularColor", pixelConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectPixelConstants>("SpecularPower", pixelConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectPixelConstants>("TextureEnabled", pixelConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectPixelConstants>("Alpha", pixelConstantBuffer);

			// Clean up.
			reflection.Dispose();
			bytecode.Dispose();
		}

		[Test]
		public void LightConstantBufferIsInExpectedLayout()
		{
			// Arrange.
			string effectCode = BasicEffectCode.GetCode();
			ShaderBytecode bytecode = EffectCompiler.CompilePixelShader(effectCode);

			// Act.
			ShaderReflection reflection = new ShaderReflection(bytecode);
			ConstantBuffer lightConstantBuffer = reflection.GetConstantBuffer(1);

			// Assert.
			Assert.That(lightConstantBuffer.Description.Size,
				Is.EqualTo(Marshal.SizeOf(typeof(BasicEffect.BasicEffectLightConstants))));
			AssertFieldOffset<BasicEffect.BasicEffectLightConstants>("ActiveDirectionalLights", lightConstantBuffer);
			AssertFieldOffset<BasicEffect.BasicEffectLightConstants>("DirectionalLights", lightConstantBuffer);

			// Clean up.
			reflection.Dispose();
			bytecode.Dispose();
		}

		private static void AssertFieldOffset<T>(string name, ConstantBuffer constantBuffer)
		{
			int hlslOffset = constantBuffer.GetVariable(name).Description.StartOffset;
			Assert.That(Marshal.OffsetOf(typeof(T), name), Is.EqualTo(new IntPtr(hlslOffset)));
		}
	}
}