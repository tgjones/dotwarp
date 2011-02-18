using System.IO;
using System.Windows.Media.Imaging;
using Meshellator;
using Nexus;
using Nexus.Graphics;
using Nexus.Graphics.Cameras;

namespace DotWarp.CommandLine
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			string modelFileName = args[0];
			string outFileName = args[1];

			// Arrange.
			Scene scene = MeshellatorLoader.ImportFromFile(modelFileName);
			using (WarpSceneRenderer renderer = new WarpSceneRenderer(scene, 800, 600))
			{
				renderer.Initialize();

				Camera camera = PerspectiveCamera.CreateFromBounds(scene.Bounds,
					new Viewport(0, 0, 800, 600), MathUtility.PI_OVER_4, 0, 0, 1);

				// Act.
				BitmapSource bitmap = renderer.Render(camera);

				PngBitmapEncoder e = new PngBitmapEncoder();
				e.Frames.Add(BitmapFrame.Create(bitmap));
				using (Stream stream = File.OpenWrite(outFileName))
					e.Save(stream);
			}
		}
	}
}
