using System;
using SharpDX.Toolkit.Graphics;

namespace DotWarp.Util
{
	internal class GraphicsDeviceService : IGraphicsDeviceService
	{
		private readonly GraphicsDevice _graphicsDevice;

		public GraphicsDevice GraphicsDevice
		{
			get { return _graphicsDevice; }
		}

		public GraphicsDeviceService(GraphicsDevice graphicsDevice)
		{
			_graphicsDevice = graphicsDevice;
		}

		public event EventHandler<EventArgs> DeviceCreated;
		public event EventHandler<EventArgs> DeviceDisposing;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;
	}
}