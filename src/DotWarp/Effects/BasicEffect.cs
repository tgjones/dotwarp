using System;
using DotWarp.Util;
using Nexus;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;

namespace DotWarp.Effects
{
	internal class BasicEffect : Effect
	{
		public EffectTechnique CurrentTechnique
		{
			get { return GetTechniqueByIndex(0); }
		}

		public Matrix3D World { get; set; }
		public Matrix3D View { get; set; }
		public Matrix3D Projection { get; set; }

		public DirectionalLight DirectionalLight0 { get; private set; }
		public DirectionalLight DirectionalLight1 { get; private set; }
		public DirectionalLight DirectionalLight2 { get; private set; }

		public ColorRgbF DiffuseColor { get; set; }
		public ColorRgbF SpecularColor { get; set; }
		public float SpecularPower { get; set; }

		public float Alpha { get; set; }

		public BasicEffect(Device device, ShaderBytecode byteCode)
			: base(device, byteCode)
		{
			DirectionalLight0 = new DirectionalLight();
			DirectionalLight1 = new DirectionalLight();
			DirectionalLight2 = new DirectionalLight();

			EnableDefaultLighting();

			DiffuseColor = ColorsRgbF.White;
			SpecularColor = new ColorRgbF(0.3f, 0.3f, 0.3f);
			SpecularPower = 16;

			Alpha = 1;
		}

		public void EnableDefaultLighting()
		{
			DirectionalLight0.Direction = new Vector3D(-0.5265408f, -0.5735765f, -0.6275069f);
			DirectionalLight0.Color = new ColorRgbF(1f, 0.9607844f, 0.8078432f);
			DirectionalLight0.Direction = new Vector3D(0.7198464f, 0.3420201f, 0.6040227f);
			DirectionalLight0.Color = new ColorRgbF(0.9647059f, 0.7607844f, 0.4078432f);
			DirectionalLight0.Direction = new Vector3D(0.4545195f, -0.7660444f, 0.4545195f);
			DirectionalLight0.Color = new ColorRgbF(0.3231373f, 0.3607844f, 0.3937255f);
		}

		public static BasicEffect Create(Device device)
		{
			ShaderBytecode byteCode = GetByteCode();
			BasicEffect effect = new BasicEffect(device, byteCode);
			byteCode.Dispose();
			return effect;
		}

		public void Begin()
		{
			CommitChanges();
		}

		public void CommitChanges()
		{
			Matrix wvp = ConversionUtility.ToSharpDXMatrix(World*View*Projection);
			GetVariableByName("WorldViewProjection").AsMatrix().SetMatrix(wvp);

			GetVariableByName("CameraPosition").AsVector().Set(ConversionUtility.ToSharpDXVector3(View.Translation));

			GetVariableByName("Light0Direction").AsVector().Set(ConversionUtility.ToSharpDXVector3(DirectionalLight0.Direction));
			GetVariableByName("Light0Color").AsVector().Set(ConversionUtility.ToSharpDXVector3(DirectionalLight0.Color));

			GetVariableByName("Light1Direction").AsVector().Set(ConversionUtility.ToSharpDXVector3(DirectionalLight1.Direction));
			GetVariableByName("Light1Color").AsVector().Set(ConversionUtility.ToSharpDXVector3(DirectionalLight1.Color));

			GetVariableByName("Light2Direction").AsVector().Set(ConversionUtility.ToSharpDXVector3(DirectionalLight2.Direction));
			GetVariableByName("Light2Color").AsVector().Set(ConversionUtility.ToSharpDXVector3(DirectionalLight2.Color));

			GetVariableByName("DiffuseColor").AsVector().Set(ConversionUtility.ToSharpDXVector3(DiffuseColor));
			GetVariableByName("SpecularColor").AsVector().Set(ConversionUtility.ToSharpDXVector3(SpecularColor));
			GetVariableByName("SpecularPower").AsScalar().Set(SpecularPower);

			GetVariableByName("Alpha").AsScalar().Set(Alpha);
		}

		private static ShaderBytecode GetByteCode()
		{
			string errors;
			ShaderBytecode bytes = ShaderBytecode.Compile(BasicEffectCode.GetCode(),
				"fx_4_0", ShaderFlags.WarningsAreErrors, EffectFlags.None,
				null, null, out errors);
			if (!string.IsNullOrEmpty(errors))
				throw new InvalidOperationException("Could not compile effect because of the following error(s): " + errors);
			return bytes;
		}
	}
}