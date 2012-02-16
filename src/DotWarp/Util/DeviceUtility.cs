using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace DotWarp.Util
{
	public static class DeviceUtility
	{
		public static Device CreateDevice()
		{
            return new Device(DriverType.Warp, DeviceCreationFlags.None, FeatureLevel.Level_10_1);
		}
	}
}