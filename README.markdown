# DotWarp

### What is this?

DotWarp is an easy-to-use 3D software rasterization library, which produces 2D rendered images from 3D mesh files. It uses WARP, a 3D software rasterizer built-in to Windows Vista / 7 / Server 2008 R2, for the actual rasterization. It is a .NET 4.0 project.

For more information about WARP, see <http://msdn.microsoft.com/en-us/library/dd285359.aspx>.

### System requirements

DotWarp does have some fairly specific requirements / prerequisites:

1. .NET 4.0
2. Windows Vista / 7 / Server 2008 R2 (WARP is only supported on these platforms)

### Quick start

1. Download the latest release from the [downloads page](http://github.com/roastedamoeba/dotwarp/downloads).
   The zip file contains the necessary DLLs.
2. Read the [wiki](http://github.com/roastedamoeba/dotwarp/wiki) for information on using DotWarp to render 3D scenes.

### Why should i use DotWarp

* You want to render 3D images in a server-side environment (such as ASP.NET).

### How to use DotLiquid

DotWarp uses Meshellator to import meshes from 3D files (currently, Meshellator only supports .obj and .3ds files).
Materials are loaded from the mesh files, and default lighting is used.

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

		BitmapSource bitmap = renderer.Render(scene, camera);

		// Can save bitmap, or do something else with it.
		PngBitmapEncoder e = new PngBitmapEncoder();
		e.Frames.Add(BitmapFrame.Create(bitmap));
		using (Stream stream = File.OpenWrite("output.png"))
			e.Save(stream);
	}