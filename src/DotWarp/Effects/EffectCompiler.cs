using SharpDX.D3DCompiler;

namespace DotWarp.Effects
{
	public static class EffectCompiler
	{
		public static ShaderBytecode CompileVertexShader(string effectCode)
		{
			return ShaderBytecode.Compile(effectCode, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
		}

		public static ShaderBytecode CompilePixelShader(string effectCode)
		{
			return ShaderBytecode.Compile(effectCode, "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);
		}
	}
}