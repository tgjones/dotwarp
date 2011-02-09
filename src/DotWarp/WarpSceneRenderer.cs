using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using DotWarp.Effects;
using DotWarp.Util;
using Meshellator;
using Nexus;
using Nexus.Graphics.Cameras;
using Nexus.Util;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;
using Mesh = Meshellator.Mesh;

namespace DotWarp
{
	public class WarpSceneRenderer : IDisposable
	{
		#region Fields

		private readonly int _width;
		private readonly int _height;
		private readonly Device _device;
		private readonly DepthStencilView _depthStencilView;
		private readonly Texture2DDescription _renderTextureDescription;
		private readonly Texture2D _renderTexture;
		private readonly RenderTargetView _renderTargetView;
		private readonly float _aspectRatio;
		private readonly BlendState _opaqueBlendState;
		private readonly BlendState _alphaBlendState;

		#endregion

		#region Constructor

		public WarpSceneRenderer(int width, int height)
		{
			_width = width;
			_height = height;
			_aspectRatio = width / (float)height;

			_device = DeviceUtility.CreateDevice();

			var viewport = new Viewport(0, 0, width, height);
			_device.Rasterizer.SetViewports(viewport);

			_device.Rasterizer.State = new RasterizerState(_device, new RasterizerStateDescription
			{
				CullMode = CullMode.Back,
				FillMode = FillMode.Solid,
				IsMultisampleEnabled = true
			});

			SampleDescription sampleDescription = new SampleDescription(8, 0);
			Texture2DDescription depthStencilDescription = new Texture2DDescription
			{
				Format = Format.D24_UNorm_S8_UInt,
				SampleDescription = sampleDescription,
				Usage = ResourceUsage.Default,
				Width = width,
				Height = height,
				BindFlags = BindFlags.DepthStencil,
				ArraySize = 1,
				CpuAccessFlags = CpuAccessFlags.None,
				MipLevels = 1
			};
			_depthStencilView = new DepthStencilView(_device, new Texture2D(_device, depthStencilDescription));

			_renderTextureDescription = new Texture2DDescription
			{
				Format = Format.R8G8B8A8_UNorm,
				SampleDescription = sampleDescription,
				Usage = ResourceUsage.Default,
				Width = width,
				Height = height,
				BindFlags = BindFlags.RenderTarget,
				ArraySize = 1,
				CpuAccessFlags = CpuAccessFlags.None,
				MipLevels = 1
			};
			_renderTexture = new Texture2D(_device, _renderTextureDescription);
			_renderTargetView = new RenderTargetView(_device, _renderTexture);

			BlendStateDescription opaqueBlendStateDescription = new BlendStateDescription
			{
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.Zero,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.Zero,
				AlphaBlendOperation = BlendOperation.Add,
				IsAlphaToCoverageEnabled = false
			};
			opaqueBlendStateDescription.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
			_opaqueBlendState = new BlendState(_device, opaqueBlendStateDescription);

			BlendStateDescription alphaBlendStateDescription = opaqueBlendStateDescription;
			alphaBlendStateDescription.SourceBlend = BlendOption.One;
			alphaBlendStateDescription.DestinationBlend = BlendOption.InverseSourceAlpha;
			alphaBlendStateDescription.IsBlendEnabled[0] = true;
			_alphaBlendState = new BlendState(_device, alphaBlendStateDescription);
		}

		#endregion

		#region Methods

		public BitmapSource Render(Scene scene, Camera camera)
		{
			// Setup for rendering.
			_device.ClearRenderTargetView(_renderTargetView, System.Drawing.Color.LightBlue);
			_device.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1, 0);
			_device.OutputMerger.SetTargets(_depthStencilView, _renderTargetView);
			_device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

			BasicEffect effect = BasicEffect.Create(_device);
			effect.View = camera.GetViewMatrix();
			effect.Projection = camera.GetProjectionMatrix(_aspectRatio);

			_device.InputAssembler.SetInputLayout(new InputLayout(_device,
				effect.CurrentTechnique.GetPassByIndex(0).Description.Signature,
				VertexPositionNormalTexture.InputElements));

			// Render scene.
			_device.OutputMerger.BlendState = _opaqueBlendState;
			foreach (Mesh mesh in scene.Meshes.Where(m => m.Material.Transparency == 1))
				RenderMesh(mesh, effect);
			_device.OutputMerger.BlendState = _alphaBlendState;
			foreach (Mesh mesh in scene.Meshes.Where(m => m.Material.Transparency < 1))
				RenderMesh(mesh, effect);

			// Extract image from render target.
			_device.OutputMerger.SetTargets((RenderTargetView)null);

			Texture2DDescription resolveTextureDescription = _renderTextureDescription;
			resolveTextureDescription.SampleDescription = new SampleDescription(1, 0);
			resolveTextureDescription.Usage = ResourceUsage.Default;
			resolveTextureDescription.BindFlags = BindFlags.None;
			resolveTextureDescription.CpuAccessFlags = CpuAccessFlags.None;
			Texture2D resolveTexture = new Texture2D(_device, resolveTextureDescription);

			_device.ResolveSubresource(_renderTexture, 0, resolveTexture, 0, Format.R8G8B8A8_UNorm);

			Texture2DDescription stagingTextureDescription = _renderTextureDescription;
			stagingTextureDescription.SampleDescription = new SampleDescription(1, 0);
			stagingTextureDescription.Usage = ResourceUsage.Staging;
			stagingTextureDescription.BindFlags = BindFlags.None;
			stagingTextureDescription.CpuAccessFlags = CpuAccessFlags.Read;
			Texture2D stagingTexture = new Texture2D(_device, stagingTextureDescription);

			_device.CopyResource(resolveTexture, stagingTexture);

			WriteableBitmapWrapper bitmapWrapper = new WriteableBitmapWrapper(_width, _height);
			DataRectangle dr = stagingTexture.Map(0, MapMode.Read, SharpDX.Direct3D10.MapFlags.None);
			PopulateBitmap(dr, bitmapWrapper);
			stagingTexture.Unmap(0);

			bitmapWrapper.Invalidate();
			return bitmapWrapper.InnerBitmap;
		}

		private void PopulateBitmap(DataRectangle dr, WriteableBitmapWrapper bitmapWrapper)
		{
			dr.Data.Position = 0;
			for (int y = 0; y < _height; y++)
			{
				dr.Data.Position = y * dr.Pitch;
				for (int x = 0; x < _width; x++)
				{
					var c = dr.Data.Read<ColorR8G8B8A8>();
					bitmapWrapper.SetPixel(x, y, System.Windows.Media.Color.FromArgb(
						c.A, c.R, c.G, c.B));
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct ColorR8G8B8A8
		{
			public byte R;
			public byte G;
			public byte B;
			public byte A;
		}

		private void RenderMesh(Mesh mesh, BasicEffect effect)
		{
			SetVertexBuffer(mesh);
			SetIndexBuffer(mesh);

			effect.World = mesh.Transform.Value;
			effect.DiffuseColor = mesh.Material.DiffuseColor;
			effect.SpecularColor = mesh.Material.SpecularColor;
			effect.SpecularPower = mesh.Material.Shininess;
			effect.Alpha = mesh.Material.Transparency;

			effect.Begin();
			effect.CurrentTechnique.GetPassByIndex(0).Apply();

			_device.DrawIndexed(mesh.Indices.Count, 0, 0);
		}

		private void SetVertexBuffer(Mesh mesh)
		{
			DataStream vertexStream = new DataStream(
				mesh.Positions.Count*VertexPositionNormalTexture.SizeInBytes,
				false, true);
			for (int i = 0; i < mesh.Positions.Count; i++)
			{
				vertexStream.Write(mesh.Positions[i]);
				vertexStream.Write(mesh.Normals[i]);
				if (mesh.TextureCoordinates.Count - 1 >= i)
					vertexStream.Write(new Point2D(mesh.TextureCoordinates[i].X, mesh.TextureCoordinates[i].Y));
				else
					vertexStream.Write(Point2D.Zero);
			}
			vertexStream.Position = 0;
			Buffer vertexBuffer = new Buffer(_device, vertexStream, (int) vertexStream.Length,
				ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None,
				ResourceOptionFlags.None);

			_device.InputAssembler.SetVertexBuffers(0,
				new VertexBufferBinding(vertexBuffer, VertexPositionNormalTexture.SizeInBytes, 0));
		}

		private void SetIndexBuffer(Mesh mesh)
		{
			DataStream indexStream = new DataStream(mesh.Indices.Count * sizeof(short), false, true);
			for (int i = 0; i < mesh.Indices.Count; i++)
				indexStream.Write((short)mesh.Indices[i]);
			indexStream.Position = 0;
			Buffer indexBuffer = new Buffer(_device, indexStream, (int) indexStream.Length,
				ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None,
				ResourceOptionFlags.None);

			_device.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
		}

		public void Dispose()
		{
			_renderTexture.Dispose();
			_renderTargetView.Dispose();
			_device.Dispose();
		}

		#endregion
	}
}
