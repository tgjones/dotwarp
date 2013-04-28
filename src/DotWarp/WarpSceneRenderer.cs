using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using DotWarp.Util;
using Meshellator;
using Nexus.Graphics.Cameras;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Content;
using SharpDX.Toolkit.Graphics;
using RasterizerState = SharpDX.Toolkit.Graphics.RasterizerState;

namespace DotWarp
{
	public class WarpSceneRenderer : IDisposable
	{
		#region Fields

		private readonly Scene _scene;
		private readonly int _width;
		private readonly int _height;
		private readonly float _aspectRatio;
		private readonly GraphicsDevice _device;
		private readonly ContentManager _contentManager;
		private readonly DepthStencilBuffer _depthStencilTexture;
		private readonly RenderTarget2D _renderTexture;
		private readonly BasicEffect _effect;
		private readonly VertexInputLayout _inputLayout;
		private readonly List<WarpMesh> _meshes;

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

			_device = GraphicsDevice.New(DriverType.Warp, DeviceCreationFlags.None, FeatureLevel.Level_10_1);

			var serviceProvider = new ServiceProvider();
			serviceProvider.AddService<IGraphicsDeviceService>(new GraphicsDeviceService(_device));

			_contentManager = new ContentManager(serviceProvider);
			_contentManager.Resolvers.Add(new ContentResolver());

			var viewport = new Viewport(0, 0, _width, _height);
			_device.SetViewports(viewport);

			const MSAALevel msaaLevel = MSAALevel.None;
			_depthStencilTexture = DepthStencilBuffer.New(_device, _width, _height, msaaLevel, DepthFormat.Depth24Stencil8);
			_renderTexture = RenderTarget2D.New(_device, _width, _height, msaaLevel, PixelFormat.R8G8B8A8.UNorm);

			Options = new RenderOptions();

			_effect = new BasicEffect(_device);
			_effect.EnableDefaultLighting();

			_inputLayout = VertexInputLayout.New(0, typeof(VertexPositionNormalTexture));
			_device.SetVertexInputLayout(_inputLayout);

			_meshes = new List<WarpMesh>();
			foreach (Mesh mesh in _scene.Meshes)
			{
				if (!mesh.Positions.Any())
					continue;

				var warpMesh = new WarpMesh(_device, mesh);
				_meshes.Add(warpMesh);

				warpMesh.Initialize(_contentManager);
			}
		}

		#endregion

		#region Methods

		public BitmapSource Render(Camera camera)
		{
			// Setup for rendering.
			var rasterizerState = RasterizerState.New(_device, new RasterizerStateDescription
			{
				CullMode = CullMode.Back,
				FillMode = FillMode.Solid,
				IsMultisampleEnabled = true,
				IsFrontCounterClockwise = Options.TriangleWindingOrderReversed
			});
			_device.SetRasterizerState(rasterizerState);
			_device.Clear(_renderTexture, ConversionUtility.ToSharpDXColor(Options.BackgroundColor));
			_device.Clear(_depthStencilTexture, DepthStencilClearFlags.Depth, 1, 0);
			_device.SetRenderTargets(_depthStencilTexture, _renderTexture);

			_effect.LightingEnabled = Options.LightingEnabled;
			_effect.View = ConversionUtility.ToSharpDXMatrix(camera.GetViewMatrix());
			_effect.Projection = ConversionUtility.ToSharpDXMatrix(camera.GetProjectionMatrix(_aspectRatio));

			// Render scene.
			_device.SetBlendState(_device.BlendStates.Opaque);
			foreach (WarpMesh mesh in _meshes.Where(m => m.IsOpaque))
				mesh.Draw(_effect);
			_device.SetBlendState(_device.BlendStates.AlphaBlend);
			foreach (WarpMesh mesh in _meshes.Where(m => !m.IsOpaque))
				mesh.Draw(_effect);
			_device.SetBlendState(null);

			// Extract image from render target.
			_device.SetRenderTargets((RenderTargetView) null);
			_device.SetRasterizerState(null);
			rasterizerState.Dispose();

			using (var memoryStream = new MemoryStream())
			{
				_renderTexture.Save(memoryStream, ImageFileType.Png);

				var bi = new BitmapImage();
				bi.BeginInit();
				bi.CacheOption = BitmapCacheOption.OnLoad;
				bi.StreamSource = memoryStream;
				bi.EndInit();

				return bi;
			}
		}

		public void Dispose()
		{
			if (_device != null)
				_device.SetVertexInputLayout(null);
			if (_meshes != null)
				foreach (WarpMesh mesh in _meshes)
					mesh.Dispose();
			if (_effect != null)
				_effect.Dispose();
			if (_renderTexture != null)
				_renderTexture.Dispose();
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
