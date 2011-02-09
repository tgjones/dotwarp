using SharpDX.Direct3D10;

namespace DotWarp.Util
{
	public static class DeviceUtility
	{
		public static Device CreateDevice()
		{
			return new Device1(DriverType.Warp, DeviceCreationFlags.None, FeatureLevel.Level_10_1);
		}
	}
}