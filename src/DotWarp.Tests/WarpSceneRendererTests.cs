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
			using (WarpSceneRenderer renderer = new WarpSceneRenderer(800, 600))
			{
				Scene scene = MeshellatorLoader.ImportFromFile("Models/3ds/85-nissan-fairlady.3ds");
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
				var bitmap = renderer.Render(scene, camera);

				// Assert.
				Assert.IsNotNull(bitmap);

				// TODO: Temp.
				PngBitmapEncoder e = new PngBitmapEncoder();
				e.Frames.Add(BitmapFrame.Create(bitmap));
				using (Stream stream = File.OpenWrite("output.png"))
					e.Save(stream);
			}
		}
	}
}