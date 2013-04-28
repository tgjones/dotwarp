# DotWarp

### What is this?

DotWarp is an easy-to-use 3D software rasterization library, which produces 2D rendered images from 3D mesh files. It uses WARP, a 3D software rasterizer built-in to Windows Vista / 7 / 8 / Server 2008 R2 / Server 2012, for the actual rasterization. It is a .NET 4.0 project.

For more information about WARP, see <http://msdn.microsoft.com/en-us/library/gg615082.aspx>.

### System requirements

DotWarp has some fairly specific requirements / prerequisites:

1. .NET 4.0
2. Windows Vista / 7 / 8 / Server 2008 R2 / Server 2012 (WARP is only supported on these platforms)
3. Latest [DirectX End-User Runtime](http://www.microsoft.com/downloads/en/details.aspx?FamilyID=2DA43D38-DB71-4C1B-BC6A-9B6652CD92A3)

### Quick start

1. Install [DotWarp](https://nuget.org/packages/DotWarp/) using NuGet.
2. Read the [wiki](http://github.com/tgjones/dotwarp/wiki) for information on using DotWarp to render 3D scenes.

### Why should I use DotWarp?

* You want to render 3D images in a server-side environment (such as ASP.NET).

### How to use DotWarp

DotWarp relies on [Meshellator](http://github.com/tgjones/meshellator) to import meshes from 3D files (currently, Meshellator only supports .obj and .3ds files).
Materials are loaded from the mesh files, and default lighting is used.

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

		BitmapSource bitmap = renderer.Render(camera);

		// Can save bitmap, or do something else with it.
		PngBitmapEncoder e = new PngBitmapEncoder();
		e.Frames.Add(BitmapFrame.Create(bitmap));
		using (Stream stream = File.OpenWrite("output.png"))
			e.Save(stream);
	}

This renders the following image:

![rendered image](https://github.com/tgjones/dotwarp/raw/master/doc/nissan.jpg)