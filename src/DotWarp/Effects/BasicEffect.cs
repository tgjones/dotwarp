using System.Collections.Generic;
using System.Linq;
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
		private readonly Buffer _vertexConstantBuffer, _pixelConstantBuffer, _lightsConstantBuffer;
		private readonly SamplerState _samplerState;

		public Matrix3D World { get; set; }
		public Matrix3D View { get; set; }
		public Matrix3D Projection { get; set; }

		public bool LightingEnabled { get; set; }

		public List<Light> Lights { get; private set; }

		public ColorRgbF DiffuseColor { get; set; }
		public ColorRgbF SpecularColor { get; set; }
		public float SpecularPower { get; set; }

		public Texture2D Texture { get; set; }
		public bool TextureEnabled { get; set; }

		public float Alpha { get; set; }

		public BasicEffect(Device device, string effectCode)
			: base(device, effectCode)
		{
			Lights = new List<Light>();

			EnableDefaultLighting();

			DiffuseColor = ColorsRgbF.White;
			SpecularColor = new ColorRgbF(0.3f, 0.3f, 0.3f);
			SpecularPower = 16;

			_textureViews = new Dictionary<Texture2D, ShaderResourceView>();

			Alpha = 1;

			_vertexConstantBuffer = CreateConstantBuffer<BasicEffectVertexConstants>(device);
			_pixelConstantBuffer = CreateConstantBuffer<BasicEffectPixelConstants>(device);
			_lightsConstantBuffer = CreateConstantBuffer<BasicEffectLightConstants>(device);

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
		
		private static Buffer CreateConstantBuffer<T>(Device device)
		{
			return new Buffer(device, new BufferDescription
			{
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ConstantBuffer,
				SizeInBytes = Marshal.SizeOf(typeof(T)),
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 0
			});
		}

		public void EnableDefaultLighting()
		{
			LightingEnabled = true;

			Lights.Clear();
			Lights.Add(new DirectionalLight
			{
				Direction = new Vector3D(0.5265408f, 0.5735765f, 0.6275069f),
				Color = new ColorRgbF(1f, 0.9607844f, 0.8078432f)
			});
			Lights.Add(new DirectionalLight
			{
				Direction = new Vector3D(-0.7198464f, -0.3420201f, -0.6040227f),
				Color = new ColorRgbF(0.9647059f, 0.7607844f, 0.4078432f)
			});
			Lights.Add(new DirectionalLight
			{
				Direction = new Vector3D(-0.4545195f, 0.7660444f, -0.4545195f),
				Color = new ColorRgbF(0.3231373f, 0.3607844f, 0.3937255f)
			});
		}

		public override void CommitChanges()
		{
			// Vertex constants.
			var vertexConstants = new BasicEffectVertexConstants();

			Matrix wvp = ConversionUtility.ToSharpDXMatrix(World * View * Projection);
			vertexConstants.WorldViewProjection = Matrix.Transpose(wvp);
			vertexConstants.World = Matrix.Transpose(ConversionUtility.ToSharpDXMatrix(World));

			UpdateDeviceResource(vertexConstants, _vertexConstantBuffer);

			// Pixel constants.

			var pixelConstants = new BasicEffectPixelConstants
			{
				CameraPosition = ConversionUtility.ToSharpDXVector3(View.Translation),
				LightingEnabled = LightingEnabled,
				AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f),
				DiffuseColor = ConversionUtility.ToSharpDXVector3(DiffuseColor),
				SpecularColor = ConversionUtility.ToSharpDXVector3(SpecularColor),
				SpecularPower = SpecularPower
			};

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

			UpdateDeviceResource(pixelConstants, _pixelConstantBuffer);

			// Light constants.

			var lightConstants = new BasicEffectLightConstants
			{
				DirectionalLights = new BasicEffectDirectionalLight[MaxLights]
			};

			var directionalLights = Lights.OfType<DirectionalLight>().ToList();
			lightConstants.ActiveDirectionalLights = directionalLights.Count;
			for (int i = 0; i < directionalLights.Count; ++i)
			{
				lightConstants.DirectionalLights[i].Enabled = directionalLights[i].Enabled;
				lightConstants.DirectionalLights[i].Color = ConversionUtility.ToSharpDXVector3(directionalLights[i].Color);
				lightConstants.DirectionalLights[i].Direction = ConversionUtility.ToSharpDXVector3(directionalLights[i].Direction);
			}

			UpdateDeviceResource(lightConstants, _lightsConstantBuffer);

			// Bind to shader buffers.

			DeviceContext.VertexShader.SetConstantBuffer(0, _vertexConstantBuffer);
			DeviceContext.PixelShader.SetConstantBuffer(0, _pixelConstantBuffer);
			DeviceContext.PixelShader.SetConstantBuffer(1, _lightsConstantBuffer);
		}

		private void UpdateDeviceResource<T>(T constants, Buffer constantBuffer)
		{
			var dataStream = new DataStream(Marshal.SizeOf(typeof(T)), true, true);
			Marshal.StructureToPtr(constants, dataStream.DataPointer, false);
			dataStream.Position = 0;
			var dataBox = new DataBox(0, 0, dataStream);
			DeviceContext.UpdateSubresource(dataBox, constantBuffer, 0);
			dataStream.Dispose();
		}

		public override void Dispose()
		{
			foreach (ShaderResourceView view in _textureViews.Values)
				view.Dispose();
			if (_vertexConstantBuffer != null)
				_vertexConstantBuffer.Dispose();
			if (_pixelConstantBuffer != null)
				_pixelConstantBuffer.Dispose();
			if (_lightsConstantBuffer != null)
				_lightsConstantBuffer.Dispose();
			if (_samplerState != null)
				_samplerState.Dispose();
			base.Dispose();
		}

		// The following structs are more complex than I'd like, because of the need to
		// follow the packing rules from here:
		// http://msdn.microsoft.com/en-us/library/bb509632(VS.85).aspx

		[StructLayout(LayoutKind.Explicit, Size = 128)]
		internal struct BasicEffectVertexConstants
		{
			[FieldOffset(0)]
			public Matrix WorldViewProjection;
			[FieldOffset(64)]
			public Matrix World;
		}

		[StructLayout(LayoutKind.Explicit, Size = 80)]
		internal struct BasicEffectPixelConstants
		{
			[FieldOffset(0)]
			public Vector3 CameraPosition;

			[FieldOffset(12)]
			public bool LightingEnabled;

			[FieldOffset(16)]
			public Vector3 AmbientLightColor;

			[FieldOffset(32)]
			public Vector3 DiffuseColor;

			[FieldOffset(48)]
			public Vector3 SpecularColor;

			[FieldOffset(60)]
			public float SpecularPower;

			[FieldOffset(64)]
			public bool TextureEnabled;

			[FieldOffset(68)]
			public float Alpha;
		}

		[StructLayout(LayoutKind.Explicit, Size = 32)]
		internal struct BasicEffectDirectionalLight
		{
			[FieldOffset(0)]
			public bool Enabled;

			[FieldOffset(4)]
			public Vector3 Direction;

			[FieldOffset(16)]
			public Vector3 Color;
		}

		private const int MaxLights = 16;

		[StructLayout(LayoutKind.Explicit, Size = 4 + (32 * MaxLights) + 12 /* padding */)]
		internal struct BasicEffectLightConstants
		{
			[FieldOffset(0)]
			public int ActiveDirectionalLights;

			[FieldOffset(16), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLights)]
			public BasicEffectDirectionalLight[] DirectionalLights;
		}
	}
}