using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using DotWarp.Effects;
using DotWarp.Util;
using Meshellator;
using Nexus.Graphics.Cameras;
using Nexus.Util;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Mesh = Meshellator.Mesh;

namespace DotWarp
{
	public class WarpSceneRenderer : IDisposable
	{
		#region Fields

		private readonly Scene _scene;
		private readonly int _width;
		private readonly int _height;
		private readonly float _aspectRatio;
		private bool _initialized;
		private Device _device;
		private DeviceContext _deviceContext;
		private ContentManager _contentManager;
		private Texture2D _depthStencilTexture;
		private DepthStencilView _depthStencilView;
		private Texture2DDescription _renderTextureDescription;
		private Texture2D _renderTexture;
		private RenderTargetView _renderTargetView;
		private BlendState _opaqueBlendState;
		private BlendState _alphaBlendState;
		private BasicEffect _effect;
		private InputLayout _inputLayout;
		private Texture2D _resolveTexture;
		private Texture2D _stagingTexture;
		private List<WarpMesh> _meshes;

		#endregion

		#region Properties

		public RenderOptions Options { get; private set; }

		#endregion

		#region Constructor

		public WarpSceneRenderer(Scene scene, int width, int height)
		{
			_scene = scene;
			_width = width;
			_height = height;
			_aspectRatio = width / (float)height;
		}

		#endregion

		#region Methods

		public void Initialize()
		{
			_device = DeviceUtility.CreateDevice();
			_deviceContext = _device.ImmediateContext;

			_contentManager = new ContentManager(_device);

			var viewport = new Viewport(0, 0, _width, _height);
			_deviceContext.Rasterizer.SetViewports(viewport);

			SampleDescription sampleDescription = new SampleDescription(8, 0);
			Texture2DDescription depthStencilDescription = new Texture2DDescription
			{
				Format = Format.D24_UNorm_S8_UInt,
				SampleDescription = sampleDescription,
				Usage = ResourceUsage.Default,
				Width = _width,
				Height = _height,
				BindFlags = BindFlags.DepthStencil,
				ArraySize = 1,
				CpuAccessFlags = CpuAccessFlags.None,
				MipLevels = 1
			};
			_depthStencilTexture = new Texture2D(_device, depthStencilDescription);
			_depthStencilView = new DepthStencilView(_device, _depthStencilTexture);

			_renderTextureDescription = new Texture2DDescription
			{
				Format = Format.R8G8B8A8_UNorm,
				SampleDescription = sampleDescription,
				Usage = ResourceUsage.Default,
				Width = _width,
				Height = _height,
				BindFlags = BindFlags.RenderTarget,
				ArraySize = 1,
				CpuAccessFlags = CpuAccessFlags.None,
				MipLevels = 1
			};
			_renderTexture = new Texture2D(_device, _renderTextureDescription);
			_renderTargetView = new RenderTargetView(_device, _renderTexture);

			RenderTargetBlendDescription opaqueRenderTargetBlendDescription = new RenderTargetBlendDescription
			{
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.Zero,
				BlendOperation = BlendOperation.Add,
				SourceAlphaBlend = BlendOption.One,
				DestinationAlphaBlend = BlendOption.Zero,
				AlphaBlendOperation = BlendOperation.Add,
				RenderTargetWriteMask = ColorWriteMaskFlags.All
			};
			BlendStateDescription opaqueBlendStateDescription = new BlendStateDescription();
			opaqueBlendStateDescription.RenderTarget[0] = opaqueRenderTargetBlendDescription;
			_opaqueBlendState = new BlendState(_device, opaqueBlendStateDescription);

			RenderTargetBlendDescription alphaRenderTargetBlendDescription = opaqueRenderTargetBlendDescription;
			alphaRenderTargetBlendDescription.SourceBlend = BlendOption.One;
			alphaRenderTargetBlendDescription.DestinationBlend = BlendOption.InverseSourceAlpha;
			alphaRenderTargetBlendDescription.IsBlendEnabled = true;
			BlendStateDescription alphaBlendStateDescription = new BlendStateDescription();
			alphaBlendStateDescription.RenderTarget[0] = alphaRenderTargetBlendDescription;
			_alphaBlendState = new BlendState(_device, alphaBlendStateDescription);

			Options = new RenderOptions();

			_effect = new BasicEffect(_device, BasicEffectCode.GetCode());

			_deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			ShaderBytecode passSignature = _effect.Pass.Signature;
			_inputLayout = new InputLayout(_device,
				passSignature,
				VertexPositionNormalTexture.InputElements);
			passSignature.Dispose();
			_deviceContext.InputAssembler.InputLayout = _inputLayout;

			Texture2DDescription resolveTextureDescription = _renderTextureDescription;
			resolveTextureDescription.SampleDescription = new SampleDescription(1, 0);
			resolveTextureDescription.Usage = ResourceUsage.Default;
			resolveTextureDescription.BindFlags = BindFlags.None;
			resolveTextureDescription.CpuAccessFlags = CpuAccessFlags.None;
			_resolveTexture = new Texture2D(_device, resolveTextureDescription);

			Texture2DDescription stagingTextureDescription = _renderTextureDescription;
			stagingTextureDescription.SampleDescription = new SampleDescription(1, 0);
			stagingTextureDescription.Usage = ResourceUsage.Staging;
			stagingTextureDescription.BindFlags = BindFlags.None;
			stagingTextureDescription.CpuAccessFlags = CpuAccessFlags.Read;
			_stagingTexture = new Texture2D(_device, stagingTextureDescription);

			_meshes = new List<WarpMesh>();
			foreach (Mesh mesh in _scene.Meshes)
			{
				if (!mesh.Positions.Any())
					continue;

				WarpMesh warpMesh = new WarpMesh(_device, mesh);
				_meshes.Add(warpMesh);

				warpMesh.Initialize(_contentManager);
			}

			_initialized = true;
		}

		public BitmapSource Render(Camera camera)
		{
			if (!_initialized)
				throw new InvalidOperationException("Initialize must be called before Render");

			// Setup for rendering.
			RasterizerState rasterizerState = new RasterizerState(_device, new RasterizerStateDescription
			{
				CullMode = CullMode.Back,
				FillMode = FillMode.Solid,
				IsMultisampleEnabled = true,
				IsFrontCounterClockwise = Options.TriangleWindingOrderReversed
			});
			_deviceContext.Rasterizer.State = rasterizerState;
			_deviceContext.ClearRenderTargetView(_renderTargetView, ConversionUtility.ToDrawingColor(Options.BackgroundColor));
			_deviceContext.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1, 0);
			_deviceContext.OutputMerger.SetTargets(_depthStencilView, _renderTargetView);

			_effect.LightingEnabled = Options.LightingEnabled;
			_effect.View = camera.GetViewMatrix();
			_effect.Projection = camera.GetProjectionMatrix(_aspectRatio);

			// Render scene.
			_deviceContext.OutputMerger.BlendState = _opaqueBlendState;
			foreach (WarpMesh mesh in _meshes.Where(m => m.IsOpaque))
				mesh.Draw(_effect);
			_deviceContext.OutputMerger.BlendState = _alphaBlendState;
			foreach (WarpMesh mesh in _meshes.Where(m => !m.IsOpaque))
				mesh.Draw(_effect);
			_deviceContext.OutputMerger.BlendState = null;

			// Extract image from render target.
			_deviceContext.OutputMerger.SetTargets((RenderTargetView)null);
			_deviceContext.Rasterizer.State = null;
			rasterizerState.Dispose();

			_deviceContext.ResolveSubresource(_renderTexture, 0, _resolveTexture, 0, Format.R8G8B8A8_UNorm);

			_deviceContext.CopyResource(_resolveTexture, _stagingTexture);

			WriteableBitmapWrapper bitmapWrapper = new WriteableBitmapWrapper(_width, _height);
			DataBox db = _deviceContext.MapSubresource(_stagingTexture, 0,
				MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
			PopulateBitmap(db, bitmapWrapper);
			_deviceContext.UnmapSubresource(_stagingTexture, 0);
			db.Data.Dispose();

			bitmapWrapper.Invalidate();
			return bitmapWrapper.InnerBitmap;
		}

		private void PopulateBitmap(DataBox db, WriteableBitmapWrapper bitmapWrapper)
		{
			db.Data.Position = 0;
			for (int y = 0; y < _height; y++)
			{
				db.Data.Position = y * db.RowPitch;
				for (int x = 0; x < _width; x++)
				{
					var c = db.Data.Read<ColorR8G8B8A8>();
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

		public void Dispose()
		{
			_deviceContext.InputAssembler.InputLayout = null;
			if (_meshes != null)
				foreach (WarpMesh mesh in _meshes)
					mesh.Dispose();
			if (_stagingTexture != null)
				_stagingTexture.Dispose();
			if (_resolveTexture != null)
				_resolveTexture.Dispose();
			if (_inputLayout != null)
				_inputLayout.Dispose();
			if (_effect != null)
				_effect.Dispose();
			if (_alphaBlendState != null)
				_alphaBlendState.Dispose();
			if (_opaqueBlendState != null)
				_opaqueBlendState.Dispose();
			if (_renderTargetView != null)
				_renderTargetView.Dispose();
			if (_renderTexture != null)
				_renderTexture.Dispose();
			if (_depthStencilView != null)
				_depthStencilView.Dispose();
			if (_depthStencilTexture != null)
				_depthStencilTexture.Dispose();
			if (_contentManager != null)
				_contentManager.Dispose();
			if (_device != null)
				_device.Dispose();
		}

		#endregion
	}
}
