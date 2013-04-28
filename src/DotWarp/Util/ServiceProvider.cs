using System;
using System.Collections.Generic;

namespace DotWarp.Util
{
	internal class ServiceProvider : IServiceProvider
	{
		private readonly Dictionary<Type, object> _services;

		public ServiceProvider()
		{
			_services = new Dictionary<Type, object>();
		}

		public void AddService<T>(T service)
		{
			_services[typeof(T)] = service;
		}

		public object GetService(Type serviceType)
		{
			return _services[serviceType];
		}
	}
}