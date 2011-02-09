using System;
using System.IO;
using System.Reflection;

namespace DotWarp.Effects
{
	public static class BasicEffectCode
	{
		public static string GetCode()
		{
			// Read effect file into string.
			Assembly assembly = Assembly.GetExecutingAssembly();
			using (Stream resourceStream = assembly.GetManifestResourceStream(typeof(BasicEffect), "BasicEffect.fx"))
			{
				if (resourceStream == null)
					throw new InvalidOperationException();
				using (StreamReader streamReader = new StreamReader(resourceStream))
				{
					return streamReader.ReadToEnd();
				}
			}
		}
	}
}