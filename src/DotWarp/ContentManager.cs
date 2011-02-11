using System;
using System.Collections.Generic;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D10.Device;
using Resource = SharpDX.Direct3D10.Resource;

namespace DotWarp
{
	public class ContentManager : IDisposable
	{
		private readonly Device _device;
		private readonly Dictionary<string, Resource> _resources;

		public ContentManager(Device device)
		{
			_device = device;
			_resources = new Dictionary<string, Resource>();
		}

		public T Load<T>(string path)
			where T : Resource
		{
			Resource resource;
			if (!_resources.TryGetValue(path, out resource))
			{
				resource = Resource.FromFile<T>(_device, path, new ImageLoadInformation
				{
					BindFlags = BindFlags.ShaderResource,
					Usage = ResourceUsage.Default,
					Format = Format.R8G8B8A8_UNorm
				});
				_resources.Add(path, resource);
			}
			return (T) resource;
		}

		public void Dispose()
		{
			foreach (Resource resource in _resources.Values)
				resource.Dispose();
		}
	}
}