# DotWarp

### What is this?

DotWarp is an easy-to-use 3D software rasterization library, which produces 2D rendered images from 3D mesh files. It uses WARP, a 3D software rasterizer built-in to Windows Vista / 7 / Server 2008 R2, for the actual rasterization. It is a .NET 4.0 project.

For more information about WARP, see <http://msdn.microsoft.com/en-us/library/dd285359.aspx>.

### System requirements

DotWarp does have some fairly specific requirements / prerequisites:

1. .NET 4.0
2. Windows Vista / 7 / Server 2008 R2 (WARP is only supported on these platforms)
3. [SlimDX](http://slimdx.org)

### Quick start

1. Download the latest release from the [downloads page](http://github.com/formosatek/dotwarp/downloads).
   The zip file contains the necessary DLLs.
2. Read the [wiki](http://github.com/formosatek/dotwarp/wiki) for information on using DotWarp to render 3D scenes.

### Why should i use DotWarp

* You want to render 3D images in a server-side environment (such as ASP.NET).

### How to use DotLiquid

DotWarp uses Meshellator to import meshes from 3D files (currently, Meshellator only imports .obj and .3ds files).
Materials are loaded from the mesh files, and default lighting is used.

	WriteableBitmap outputImage = new WriteableBitmap(800, 600); 
	ImageRenderer imageRenderer = new ImageRenderer(outputImage, 1); 

	WarpDevice device = new WarpDevice(); 
	device.SetRenderTarget(imageRenderer.GetBuffer()); 
	device.BeginScene();  

	WarpModel model = WarpModel.FromFile(device, "mini.3ds"); 
	model.Draw(); 

	device.EndScene(); 
	imageRenderer.Present();  

	// Save or display outputImage. 