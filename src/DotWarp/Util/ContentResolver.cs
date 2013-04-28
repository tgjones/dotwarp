using System.IO;
using SharpDX.Toolkit.Content;

namespace DotWarp.Util
{
	internal class ContentResolver : IContentResolver
	{
		public Stream Resolve(string assetName)
		{
			return File.OpenRead(assetName);
		}
	}
}