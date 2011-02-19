using System.Collections.Generic;
using System.Runtime.InteropServices;
using DotWarp.Util;
using Nexus;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace DotWarp.Effects
{
	internal class BasicEffect : Effect
	{
		private readonly Dictionary<Texture2D, ShaderResourceView> _textureViews;
		private readonly Buffer _vertexConstantBuffer, _pixelConstantBuffer;
		private readonly SamplerState _samplerState;

		public Matrix3D World { get; set; }
		public Matrix3D View { get; set; }
		public Matrix3D Projection { get; set; }

		public bool LightingEnabled { get; set; }

		public DirectionalLight DirectionalLight0 { get; private set; }
		public DirectionalLight DirectionalLight1 { get; private set; }
		public DirectionalLight DirectionalLight2 { get; private set; }

		public ColorRgbF DiffuseColor { get; set; }
		public ColorRgbF SpecularColor { get; set; }
		public float SpecularPower { get; set; }

		public Texture2D Texture { get; set; }
		public bool TextureEnabled { get; set; }

		public float Alpha { get; set; }

		public BasicEffect(Device device, string effectCode)
			: base(device, effectCode)
		{
			DirectionalLight0 = new DirectionalLight();
			DirectionalLight1 = new DirectionalLight();
			DirectionalLight2 = new DirectionalLight();

			EnableDefaultLighting();

			DiffuseColor = ColorsRgbF.White;
			SpecularColor = new ColorRgbF(0.3f, 0.3f, 0.3f);
			SpecularPower = 16;

			_textureViews = new Dictionary<Texture2D, ShaderResourceView>();

			Alpha = 1;

			_vertexConstantBuffer = new Buffer(device, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ConstantBuffer,
				SizeInBytes = Marshal.SizeOf(typeof(BasicEffectVertexConstants)),
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 0
			});
			_pixelConstantBuffer = new Buffer(device, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ConstantBuffer,
				SizeInBytes = Marshal.SizeOf(typeof(BasicEffectPixelConstants)),
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 0
			});

			_samplerState = new SamplerState(device, new SamplerStateDescription
			{
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
				Filter = Filter.Anisotropic,
				MaximumLod = float.MaxValue,
				MinimumLod = 0
			});
		}

		public void EnableDefaultLighting()
		{
			LightingEnabled = true;

			DirectionalLight0.Enabled = true;
			DirectionalLight0.Direction = new Vector3D(0.5265408f, 0.5735765f, 0.6275069f);
			DirectionalLight0.Color = new ColorRgbF(1f, 0.9607844f, 0.8078432f);
			DirectionalLight1.Enabled = true;
			DirectionalLight1.Direction = new Vector3D(-0.7198464f, -0.3420201f, -0.6040227f);
			DirectionalLight1.Color = new ColorRgbF(0.9647059f, 0.7607844f, 0.4078432f);
			DirectionalLight2.Enabled = true;
			DirectionalLight2.Direction = new Vector3D(-0.4545195f, 0.7660444f, -0.4545195f);
			DirectionalLight2.Color = new ColorRgbF(0.3231373f, 0.3607844f, 0.3937255f);
		}

		public override void CommitChanges()
		{
			BasicEffectVertexConstants vertexConstants = new BasicEffectVertexConstants();

			Matrix wvp = ConversionUtility.ToSharpDXMatrix(World * View * Projection);
			vertexConstants.WorldViewProjection = Matrix.Transpose(wvp);
			vertexConstants.World = Matrix.Transpose(ConversionUtility.ToSharpDXMatrix(World));

			DataStream vertexDataStream = new DataStream(Marshal.SizeOf(typeof(BasicEffectVertexConstants)), true, true);
			Marshal.StructureToPtr(vertexConstants, vertexDataStream.DataPointer, false);
			vertexDataStream.Position = 0;
			DataBox vertexDataBox = new DataBox(0, 0, vertexDataStream);
			DeviceContext.UpdateSubresource(vertexDataBox, _vertexConstantBuffer, 0);
			vertexDataStream.Dispose();

			BasicEffectPixelConstants pixelConstants = new BasicEffectPixelConstants();

			pixelConstants.CameraPosition = ConversionUtility.ToSharpDXVector3(View.Translation);

			pixelConstants.LightingEnabled = LightingEnabled;

			pixelConstants.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);

			pixelConstants.Light0Enabled = DirectionalLight0.Enabled;
			pixelConstants.Light0Direction = ConversionUtility.ToSharpDXVector3(DirectionalLight0.Direction);
			pixelConstants.Light0Color = ConversionUtility.ToSharpDXVector3(DirectionalLight0.Color);

			pixelConstants.Light1Enabled = DirectionalLight1.Enabled;
			pixelConstants.Light1Direction = ConversionUtility.ToSharpDXVector3(DirectionalLight1.Direction);
			pixelConstants.Light1Color = ConversionUtility.ToSharpDXVector3(DirectionalLight1.Color);

			pixelConstants.Light2Enabled = DirectionalLight2.Enabled;
			pixelConstants.Light2Direction = ConversionUtility.ToSharpDXVector3(DirectionalLight2.Direction);
			pixelConstants.Light2Color = ConversionUtility.ToSharpDXVector3(DirectionalLight2.Color);

			pixelConstants.DiffuseColor = ConversionUtility.ToSharpDXVector3(DiffuseColor);
			pixelConstants.SpecularColor = ConversionUtility.ToSharpDXVector3(SpecularColor);
			pixelConstants.SpecularPower = SpecularPower;

			DeviceContext.PixelShader.SetSampler(0, _samplerState);
			if (Texture != null)
			{
				ShaderResourceView view;
				if (!_textureViews.TryGetValue(Texture, out view))
				{
					view = new ShaderResourceView(Device, Texture);
					_textureViews.Add(Texture, view);
				}
				DeviceContext.PixelShader.SetShaderResource(0, view);
			}
			else
			{
				DeviceContext.PixelShader.SetShaderResource(0, null);
			}
			pixelConstants.TextureEnabled = TextureEnabled;

			pixelConstants.Alpha = Alpha;

			DataStream dataStream = new DataStream(Marshal.SizeOf(typeof(BasicEffectPixelConstants)), true, true);
			Marshal.StructureToPtr(pixelConstants, dataStream.DataPointer, false);
			dataStream.Position = 0;
			DataBox dataBox = new DataBox(0, 0, dataStream);
			DeviceContext.UpdateSubresource(dataBox, _pixelConstantBuffer, 0);
			dataStream.Dispose();

			DeviceContext.VertexShader.SetConstantBuffer(0, _vertexConstantBuffer);
			DeviceContext.PixelShader.SetConstantBuffer(0, _pixelConstantBuffer);
		}

		public override void Dispose()
		{
			foreach (ShaderResourceView view in _textureViews.Values)
				view.Dispose();
			if (_vertexConstantBuffer != null)
				_vertexConstantBuffer.Dispose();
			if (_pixelConstantBuffer != null)
				_pixelConstantBuffer.Dispose();
			if (_samplerState != null)
				_samplerState.Dispose();
			base.Dispose();
		}

		[StructLayout(LayoutKind.Explicit, Size = 128)]
		private struct BasicEffectVertexConstants
		{
			[FieldOffset(0)]
			public Matrix WorldViewProjection;
			[FieldOffset(64)]
			public Matrix World;
		}

		[StructLayout(LayoutKind.Explicit, Size = 176)]
		private struct BasicEffectPixelConstants
		{
			[FieldOffset(0)]
			public Vector3 CameraPosition;

			[FieldOffset(12)]
			public bool LightingEnabled;

			[FieldOffset(16)]
			public Vector3 AmbientLightColor;

			[FieldOffset(28)]
			public bool Light0Enabled;
			[FieldOffset(32)]
			public Vector3 Light0Direction;
			[FieldOffset(48)]
			public Vector3 Light0Color;

			[FieldOffset(60)]
			public bool Light1Enabled;
			[FieldOffset(64)]
			public Vector3 Light1Direction;
			[FieldOffset(80)]
			public Vector3 Light1Color;

			[FieldOffset(92)]
			public bool Light2Enabled;
			[FieldOffset(96)]
			public Vector3 Light2Direction;
			[FieldOffset(112)]
			public Vector3 Light2Color;

			[FieldOffset(128)]
			public Vector3 DiffuseColor;
			[FieldOffset(144)]
			public Vector3 SpecularColor;
			[FieldOffset(156)]
			public float SpecularPower;

			[FieldOffset(160)]
			public bool TextureEnabled;

			[FieldOffset(164)]
			public float Alpha;
		}
	}
}