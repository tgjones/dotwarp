using System;
using System.Linq;
using DotWarp.Util;
using SharpDX;
using SharpDX.Toolkit.Content;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace DotWarp
{
	internal class WarpMesh : IDisposable
	{
		private readonly GraphicsDevice _device;
		private readonly Meshellator.Mesh _sourceMesh;
		private bool _initialized;
		private Buffer<VertexPositionNormalTexture> _vertexBuffer;
		private Buffer<short> _indexBuffer;
		private Texture2D _texture;

		public bool IsOpaque
		{
			get { return _sourceMesh.Material.Transparency == 1.0f; }
		}

		public WarpMesh(GraphicsDevice device, Meshellator.Mesh sourceMesh)
		{
			_device = device;
			_sourceMesh = sourceMesh;
		}

		public void Initialize(ContentManager contentManager)
		{
			CreateVertexBuffer();
			CreateIndexBuffer();

			if (!string.IsNullOrEmpty(_sourceMesh.Material.DiffuseTextureName))
				_texture = contentManager.Load<Texture2D>(_sourceMesh.Material.DiffuseTextureName);

			_initialized = true;
		}

		private void CreateVertexBuffer()
		{
			var vertices = new VertexPositionNormalTexture[_sourceMesh.Positions.Count];
			for (int i = 0; i < _sourceMesh.Positions.Count; i++)
			{
				vertices[i].Position = ConversionUtility.ToSharpDXVector3(_sourceMesh.Positions[i]);
				vertices[i].Normal = ConversionUtility.ToSharpDXVector3(_sourceMesh.Normals[i]);
				if (_sourceMesh.TextureCoordinates.Count - 1 >= i)
					vertices[i].TextureCoordinate = new Vector2(_sourceMesh.TextureCoordinates[i].X, _sourceMesh.TextureCoordinates[i].Y);
			}
			_vertexBuffer = Buffer.Vertex.New(_device, vertices);
		}

		private void CreateIndexBuffer()
		{
			var indices = _sourceMesh.Indices.Select(x => (short) x).ToArray();
			_indexBuffer = Buffer.Index.New(_device, indices);
		}

		public void Draw(BasicEffect effect)
		{
			if (!_initialized)
				throw new InvalidOperationException("Initialize must be called before Draw");

			_device.SetVertexBuffer(_vertexBuffer);
			_device.SetIndexBuffer(_indexBuffer, false);

			effect.World = ConversionUtility.ToSharpDXMatrix(_sourceMesh.Transform.Value);
			effect.DiffuseColor = ConversionUtility.ToSharpDXVector3(_sourceMesh.Material.DiffuseColor);
			effect.SpecularColor = ConversionUtility.ToSharpDXVector3(_sourceMesh.Material.SpecularColor);
			effect.SpecularPower = _sourceMesh.Material.Shininess;

			if (!string.IsNullOrEmpty(_sourceMesh.Material.DiffuseTextureName))
			{
				effect.TextureEnabled = true;
				effect.Texture = _texture;
			}
			else
			{
				effect.TextureEnabled = false;
				effect.Texture = null;
			}

			effect.Alpha = _sourceMesh.Material.Transparency;

			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				_device.DrawIndexed(PrimitiveType.TriangleList, _sourceMesh.Indices.Count);
			}

			_device.SetIndexBuffer(null, false);
			_device.SetVertexBuffer((Buffer<VertexPositionNormalTexture>) null);
		}

		public void Dispose()
		{
			if (_vertexBuffer != null)
				_vertexBuffer.Dispose();
			if (_indexBuffer != null)
				_indexBuffer.Dispose();
		}
	}
}