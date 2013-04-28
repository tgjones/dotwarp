using System.IO;
using System.Windows.Media.Imaging;
using Meshellator;
using Nexus;
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
			using (var renderer = new WarpSceneRenderer(scene, 800, 600))
			{
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
				SaveImage(bitmap, "nissan.jpg");
			}
		}

		[Test]
		public void CanRenderModelWithTextures()
		{
			// Arrange.
			Scene scene = MeshellatorLoader.ImportFromFile("Models/Obj/Tank.obj");
			using (var renderer = new WarpSceneRenderer(scene, 800, 600))
			{
				renderer.Options.TriangleWindingOrderReversed = true;

				Camera camera = new PerspectiveCamera
				{
					FarPlaneDistance = 100000,
					NearPlaneDistance = 1,
					FieldOfView = MathUtility.PI_OVER_4,
					LookDirection = new Vector3D(-1, -0.3f, 1),
					Position = new Point3D(300, 150, -150),
					UpDirection = Vector3D.Up
				};

				// Act.
				var bitmap = renderer.Render(camera);

				// Assert.
				Assert.IsNotNull(bitmap);
				SaveImage(bitmap, "tank.jpg");
			}
		}

		[Test]
		public void CanRenderPrimitive()
		{
			// Arrange.
			Scene scene = MeshellatorLoader.CreateFromTeapot(10, 10);
			using (var renderer = new WarpSceneRenderer(scene, 550, 350))
			{
				renderer.Options.BackgroundColor = Color.FromRgb(200, 200, 200);

				Camera camera = new PerspectiveCamera
				{
					LookDirection = new Vector3D(-1, -0.3f, 1),
					Position = new Point3D(50, 20, -20),
				};

				// Act.
				BitmapSource bitmap = renderer.Render(camera);

				// Assert.
				Assert.IsNotNull(bitmap);
				SaveImage(bitmap, "teapot.jpg");
			}
		}

		private static void SaveImage(BitmapSource bitmap, string filename)
		{
			var e = new JpegBitmapEncoder();
			e.Frames.Add(BitmapFrame.Create(bitmap));
			using (Stream stream = File.OpenWrite(filename))
				e.Save(stream);
		}
	}
}