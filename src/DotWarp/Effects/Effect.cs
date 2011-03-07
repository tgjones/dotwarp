using System;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace DotWarp.Effects
{
	internal class Effect : IDisposable
	{
		private readonly ShaderBytecode _vertexShaderBytecode;
		private readonly ShaderBytecode _pixelShaderBytecode;

		public Device Device { get; private set; }
		public DeviceContext DeviceContext { get; private set; }
		public EffectPass Pass { get; private set; }

		public Effect(Device device, string effectCode)
		{
			_vertexShaderBytecode = EffectCompiler.CompileVertexShader(effectCode);
			_pixelShaderBytecode = EffectCompiler.CompilePixelShader(effectCode);

			Device = device;
			DeviceContext = device.ImmediateContext;
			Pass = new EffectPass(device, _vertexShaderBytecode, _pixelShaderBytecode);
		}

		public void Begin()
		{
			CommitChanges();
		}

		public virtual void CommitChanges()
		{
			
		}

		public virtual void Dispose()
		{
			if (_pixelShaderBytecode != null)
				_pixelShaderBytecode.Dispose();
			if (_vertexShaderBytecode != null)
				_vertexShaderBytecode.Dispose();
			if (Pass != null)
				Pass.Dispose();
		}
	}
}