using System;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace DotWarp.Effects
{
	internal class EffectPass : IDisposable
	{
		private readonly Device _device;
		private readonly ShaderBytecode _vertexShaderBytecode;
		private readonly DeviceContext _deviceContext;
		private readonly VertexShader _vertexShader;
		private readonly PixelShader _pixelShader;

		public ShaderBytecode Signature
		{
			get { return _vertexShaderBytecode; }
		}

		public EffectPass(Device device, ShaderBytecode vertexShaderBytecode, ShaderBytecode pixelShaderBytecode)
		{
			_device = device;
			_deviceContext = device.ImmediateContext;

			_vertexShaderBytecode = vertexShaderBytecode;

			_vertexShader = new VertexShader(_device, vertexShaderBytecode);
			_pixelShader = new PixelShader(_device, pixelShaderBytecode);
		}

		public void Apply()
		{
			_deviceContext.VertexShader.Set(_vertexShader);
			_deviceContext.PixelShader.Set(_pixelShader);
		}

		public void Dispose()
		{
			_pixelShader.Dispose();
			_vertexShader.Dispose();
		}
	}
}