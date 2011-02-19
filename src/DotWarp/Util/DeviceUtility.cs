using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace DotWarp.Util
{
	public static class DeviceUtility
	{
		private static bool _noHardware;

		public static Device CreateDevice()
		{
			// If we have previously found that there's no compatible hardware device,
			// go straight to creating a WARP device.
			if (_noHardware)
				return CreateDevice(DriverType.Warp);

			// First attempt to create a hardware device. This will fail if the host computer
			// does not have a graphics card that supports feature level 10.1.
			try
			{
				return CreateDevice(DriverType.Hardware);
			}
			catch (SharpDXException)
			{
				_noHardware = true;
				return CreateDevice(DriverType.Warp);
			}
		}

		private static Device CreateDevice(DriverType driverType)
		{
			return new Device(driverType, DeviceCreationFlags.None, FeatureLevel.Level_10_1);
		}
	}
}