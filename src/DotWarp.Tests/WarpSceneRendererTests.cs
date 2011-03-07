using System.IO;
using System.Windows.Media.Imaging;
using Meshellator;
using Nexus;
using Nexus.Graphics;
using Nexus.Graphics.Cameras;
using NUnit.Framework;

namespace DotWarp.Tests
{
	[TestFixture]
	public class WarpSceneRendererTests
	{
		[Test]
		public void CanRenderModel()
		{
			// Arrange.
			Scene scene = MeshellatorLoader.ImportFromFile("Models/3ds/85-nissan-fairlady.3ds");
			using (WarpSceneRenderer renderer = new WarpSceneRenderer(scene, 800, 600))
			{
				renderer.Initialize();

				Camera camera = new PerspectiveCamera
				{
					FarPlaneDistance = 100000,
					NearPlaneDistance = 1,
					FieldOfView = MathUtility.PI_OVER_4,
					LookDirection = new Vector3D(-1, -0.3f, 1),
					Position = new Point3D(3000, 1500, -1500),
					UpDirection = Vector3D.Up
				};

				// Act.
				var bitmap = renderer.Render(camera);

				// Assert.
				Assert.IsNotNull(bitmap);

				//PngBitmapEncoder e = new PngBitmapEncoder();
				//e.Frames.Add(BitmapFrame.Create(bitmap));
				//using (Stream stream = File.OpenWrite("output.png"))
				//    e.Save(stream);
			}
		}

		[Test]
		public void CanRenderPrimitive()
		{
			// Arrange.
			Scene scene = MeshellatorLoader.ImportFromFile("Models/3ds/85-nissan-fairlady.3ds");
			using (WarpSceneRenderer renderer = new WarpSceneRenderer(scene, 550, 350))
			{
				renderer.Initialize();

				renderer.Options.BackgroundColor = Color.FromRgb(200, 200, 200);

				Camera camera = new PerspectiveCamera
				{
					LookDirection = new Vector3D(-1, -0.3f, 1),
					Position = new Point3D(2100, 1200, -700),
				};

				BitmapSource bitmap = renderer.Render(camera);

				JpegBitmapEncoder e = new JpegBitmapEncoder();
				e.Frames.Add(BitmapFrame.Create(bitmap));
				using (Stream stream = File.OpenWrite("output.jpg"))
					e.Save(stream);
			}
		}
	}
}